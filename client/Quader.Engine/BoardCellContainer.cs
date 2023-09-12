﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace Quader.Engine
{
    /// <summary>
    /// Helper class that holds the board data in a two dimensional array
    /// </summary>
    internal class BoardCellContainer
    {
        public int Width { get; }
        public int Height { get; }
        
        private readonly BoardCellType[][] _board;
        
        public BoardCellContainer(int width, int height)
        {
            Width = width;
            Height = height;
            
            _board = new BoardCellType[Height][];
            for (int i = 0; i < Height; i++)
            {
                _board[i] = new BoardCellType[Width];
            }
            
            Reset();
        }
        
        /// <summary>
        /// Clears all the cells setting them to BoardCellType.None
        /// </summary>
        public void Reset()
        {
            ForEach((x, y) => _board[y][x] = BoardCellType.None);
        }
        
        /// <summary>
        /// Moves all the board content up by one block
        /// </summary>
        public void MoveUp()
        {
            for (int y = 1; y < Height; y++)
            {
                var empty = new BoardCellType[Width];
                var cur = _board[y];
                var tmp = new BoardCellType[Width];
                Array.Copy(cur, tmp, Width);

                _board[y] = empty;
                _board[y - 1] = tmp;
            }
        }

        /// <summary>
        /// Moves all the board content down by one block starting at specified Y
        /// </summary>
        /// <param name="fromY"></param>
        public void MoveDown(int fromY = 0)
        {
            for (int y = fromY - 1; y >= 0; y--)
            {
                var empty = new BoardCellType[Width];
                var cur = _board[y];
                var tmp = new BoardCellType[Width];
                Array.Copy(cur, tmp, Width);

                _board[y] = empty;
                _board[y + 1] = tmp;
            }
        }
        
        /// <summary>
        /// Clears a single line at specified Y
        /// </summary>
        /// <param name="y"></param>
        public void ClearLine(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                SetCellAt(x, y, BoardCellType.None);
            }
        }

        public void ClearAll()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    _board[y][x] = BoardCellType.None;
                }
            }
        }
        
        public bool IsLineFull(int y)
        {
            for (int i = 0; i < Width; i++)
            {
                var c = GetCellAt(i, y);
                if (c == BoardCellType.None || c == BoardCellType.Solid)
                    return false;
            }

            return true;
        }

        public bool[] ToBoolArray()
        {
            List<bool> res = new List<bool>();

            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    res.Add(GetCellAt(x, y) != BoardCellType.None);
                }
            }

            return res.ToArray();
        }

        /// <summary>
        /// Checks lines that are full and returns Y positions for those lines
        /// </summary>
        /// <param name="bounds">Piece bounds</param>
        /// <returns></returns>
        public int[] CheckLineClears(Rectangle bounds)
        {
            List<int> linesCleared = new List<int>();

            var b = bounds;

            for (int y = Math.Max(b.Top, 0); y < Height; y++)
            {
                var isFull = IsLineFull(y);
                if (isFull)
                {
                    linesCleared.Add(y);
                }
            }

            return linesCleared.ToArray();
        }

        /// <summary>
        /// Clears specified lines
        /// </summary>
        /// <param name="ys"></param>
        public void ClearLines(int[] ys)
        {
            foreach (var y in ys)
            {
                MoveDown(y);
            }
        }

        /// <summary>
        /// Calculates whether any point in the specified array intersects either board bounds on other blocks
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public bool Intersects(Point[] points)
        {
            foreach (var point in points)
            {
                if (IsOutOfBounds(point) || GetCellAt(point.X, point.Y) != BoardCellType.None)
                    return true;
            }

            return false;
        }

        public bool IsOutOfBounds(Point p) => p.X < 0 || p.X >= Width || p.Y >= Height || p.Y < 0;
        
        public BoardCellType GetCellAt(int x, int y) => _board[y][x];
        public void SetCellAt(int x, int y, BoardCellType cell) => _board[y][x] = cell;

        public void SetLine(int y, BoardCellType[] row) => _board[y] = row;
        
        private void ForEach(Action<int, int> action)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    action(x, y);
                }
            }
        }
    }
}