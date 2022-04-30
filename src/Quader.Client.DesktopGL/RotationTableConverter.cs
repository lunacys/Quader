﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nez;
using Quader.Engine;

namespace Quader
{
    public enum PieceStartPosition
    {
        Initial,
        RotationClockwise,
        RotationCounterClockwise,
        Rotation180Deg
    }

    public class RotationEncoding
    {
        [JsonProperty(PropertyName = "sp")]
        public PieceStartPosition StartPosition { get; set; }
        [JsonProperty(PropertyName = "ibo")]
        public int InitialBitsOffset { get; set; }
        [JsonProperty(PropertyName = "cbo")]
        public int[] ClockwiseBitsOffsets { get; set; } = null!;
        [JsonProperty(PropertyName = "ccbo")]
        public int[] CounterClockwiseBitsOffsets { get; set; } = null!;
        [JsonProperty(PropertyName = "ie")]
        public long InitialEncoding { get; set; }
        [JsonProperty(PropertyName = "tc")]
        public long[] TestsClockwise { get; set; } = null!;
        [JsonProperty(PropertyName = "tcc")]
        public long[] TestsCounterClockwise { get; set; } = null!;
    }

    public class RotationPositionEncoding
    {
        [JsonProperty(PropertyName = "pe")]
        public Dictionary<PieceStartPosition, RotationEncoding> PositionEncodings { get; set; } = null!;
    }

    public class RotationSystemTable
    {
        [JsonProperty(PropertyName = "rst")]
        public Dictionary<PieceType, RotationPositionEncoding> RotationSystemTableMap { get; set; } = null!;
        [JsonProperty(PropertyName = "co")]
        public ConverterOptions ConverterOptions { get; set; }
    }

    public class ConverterOptions
    {
        [JsonProperty(PropertyName = "ss")]
        public int SegmentSize { get; set; }
        [JsonProperty(PropertyName = "os")]
        public Dictionary<PieceType, Point>? OffsetSettings { get; set; } = null;
        [JsonProperty(PropertyName = "tc")]
        public int TestCount { get; set; }

        public static ConverterOptions Default => new ConverterOptions
        {
            SegmentSize = 16,
            TestCount = 5,
            OffsetSettings = new Dictionary<PieceType, Point>
            {
                { PieceType.I, new Point(8, 8) },
                { PieceType.O, new Point(14, 14) },
                { PieceType.T, new Point(10, 10) },
                { PieceType.L, new Point(10, 10) },
                { PieceType.J, new Point(10, 10) },
                { PieceType.S, new Point(10, 10) },
                { PieceType.Z, new Point(10, 10) },
            }
        };
    }

    public static class RotationTableConverter
    {
        public static RotationSystemTable FromTexture2D(Texture2D image, ConverterOptions? options = null)
        {
            Color[] data = new Color[image.Width * image.Height];

            image.GetData(data);
            
            if (options == null)
                options = ConverterOptions.Default;
            
            var segmentSize = options.SegmentSize;

            Color[,] newData = new Color[image.Height, image.Width];

            // Convert 1D array to 2D array
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var index = x + image.Width * y;

                    newData[y, x] = data[index];
                }
            }

            var rst = new RotationSystemTable();
            rst.RotationSystemTableMap = new Dictionary<PieceType, RotationPositionEncoding>();

            // Step 1: Take the entire piece table from the image
            var piecesDataRaw = new List<Color[,]>(7);

            for (int i = 0; i < 7; i++)
            {
                var startX = 0;
                var startY = i * (segmentSize * 4); // 4 rows

                var width = image.Width;
                var height = segmentSize * 4; // 4 rows

                var portion = TakePortion(newData, startX, startY, width, height, new Color(1.0f, 0, 0, 1.0f));
                var pieceType = (PieceType)i; // I, O, T, L, J, S, Z
                piecesDataRaw.Add(portion);

                // Step 2: Split piece table to rows
                var dataRawArr = piecesDataRaw;

                // Proceed every row
                for (int j = 0; j < 4; j++)
                {
                    var pieceStartPos = (PieceStartPosition)j;

                    // Step 3: Create 4 sets of tests for both clockwise and counter-clockwise rotation and 4 types of initial position
                    // 0       - initial position
                    // [1,  5] - clockwise tests
                    // [6, 10] - counter-clockwise tests


                    var offset = options?.OffsetSettings?[pieceType] ?? new Point(0, 0);

                    var initialPosData = TakePortion(dataRawArr[i], 0 + offset.X / 2, 0 + offset.Y / 2, segmentSize - offset.X, segmentSize - offset.Y, Color.White);

                    List<Color[,]> clockwiseTestData = new List<Color[,]>(5);
                    for (int k = 1; k <= options.TestCount; k++)
                    {
                        clockwiseTestData.Add(TakePortion(dataRawArr[i], segmentSize * k + offset.X / 2, 0 + offset.Y / 2, segmentSize - offset.X, segmentSize - offset.Y));
                    }

                    List<Color[,]> counterClockwiseTestData = new List<Color[,]>(5);
                    for (int k = options.TestCount + 1; k <= options.TestCount * 2; k++)
                    {
                        counterClockwiseTestData.Add(TakePortion(dataRawArr[i], segmentSize * k + offset.X / 2, 0 + offset.Y / 2, segmentSize - offset.X, segmentSize - offset.Y));
                    }

                    var re = new RotationEncoding
                    {
                        StartPosition = pieceStartPos,
                        InitialEncoding = EncodeV2(initialPosData, out var initialBitsOffset),
                        TestsClockwise = EncodeV2(clockwiseTestData.ToArray(), out var clockwiseBitsOffsets),
                        TestsCounterClockwise = EncodeV2(counterClockwiseTestData.ToArray(), out var counterClockwiseBitsOffsets),
                        InitialBitsOffset = initialBitsOffset,
                        ClockwiseBitsOffsets = clockwiseBitsOffsets,
                        CounterClockwiseBitsOffsets = counterClockwiseBitsOffsets
                    };

                    var a = Encode(initialPosData);
                    var b = V2ToString(re.InitialEncoding, initialBitsOffset, segmentSize - offset.X, segmentSize - offset.Y);

                    var res = Compare(a, b);
                    if (!res)
                        throw new Exception("Data is not consistent!");

                    if (!rst.RotationSystemTableMap.ContainsKey(pieceType))
                        rst.RotationSystemTableMap[pieceType] = new RotationPositionEncoding
                            { PositionEncodings = new Dictionary<PieceStartPosition, RotationEncoding>() };


                    rst.RotationSystemTableMap[pieceType].PositionEncodings[pieceStartPos] = re;
                }
            }

            rst.ConverterOptions = options;

            return rst;
        }

        private static bool Compare(string[] a, string[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        public static string[] V2ToString(long encoded, int bitsOffset, int width, int height)
        {
            var result = new string[height];

            for (int y = 0; y < height; y++)
            {
                var row = "";

                for (int x = 0; x < width; x++)
                {
                    var i = (width * height) - (x + width * y);

                    if (i >= bitsOffset)
                    {
                        if (((encoded >> i) & 1) == 1)
                        {
                            row += "X";
                        }
                        else
                            row += ".";
                    }
                    else
                    {
                        row += ".";
                    }
                }

                result[y] = row;
            }

            return result;
        }

        private static string[] Encode(Color[,] data)
        {
            var result = new string[data.GetLength(1)];

            for (int y = 0; y < data.GetLength(1); y++)
            {
                var curStr = "";
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    var d = data[y, x];
                    if (d == Color.Black)
                        curStr += "X";
                    else
                        curStr += ".";

                }

                result[y] = curStr;
            }

            return result;
        }

        private static long EncodeV2(Color[,] data, out int bitsOffset)
        {
            long result = 0;
            bitsOffset = 0;
            bool bitsDone = false;

            for (int y = 0; y < data.GetLength(1); y++)
            {
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    var d = data[y, x];
                    if (d == Color.Black)
                    {
                        bitsDone = true;
                        result |= 1;
                    }
                    else
                    {
                        if (!bitsDone)
                            bitsOffset++;
                        
                    }

                    result <<= 1;
                }
            }

            return result;
        }

        private static long[] EncodeV2(Color[][,] data, out int[] bitsOffsets)
        {
            var result = new List<long>();
            bitsOffsets = new int[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                var c = data[i];
                result.Add(EncodeV2(c, out var bitsOffset));

                bitsOffsets[i] = bitsOffset;
            }

            return result.ToArray();
        }

        private static string[][] Encode(Color[][,] data)
        {
            var result = new List<string[]>();

            for (int i = 0; i < data.Length; i++)
            {
                var encoded = Encode(data[i]);
                result.Add(encoded);
            }

            return result.ToArray();
        }

        private static Color[,] TakePortion(Color[,] data, int x, int y, int width, int height, Color? colorToIgnore = null)
        {
            var result = new Color[height, width];

            for (int i = y; i < y + height; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    if (colorToIgnore != null && data[i, j] == colorToIgnore.Value)
                    {
                        result[i - y, j - x] = Color.Transparent;
                    }
                    else
                    {
                        result[i - y, j - x] = data[i, j];
                    }
                }
            }

            return result;
        }
    }
}