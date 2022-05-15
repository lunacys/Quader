﻿using System;
using Nez;
using Quader.Engine;

namespace Quader.Components.Boards
{
    public class ScoreHandlerComponent : RenderableComponent, IUpdatable, IBoardComponent
    {
        public override float Width => 500;
        public override float Height => 500;
        public Board Board { get; }

        public float Pps { get; private set; }
        public int TotalPieces { get; private set; }

        public ScoreHandlerComponent(Board board)
        {
            Board = board;

            Board.PieceHardDropped += (sender, boardMove) =>
            {
                TotalPieces++;

                Console.WriteLine($"Combo: {boardMove.Combo}. B2B: {boardMove.BackToBack}. Lines Cleared: {boardMove.LinesCleared}. Modificators: {boardMove.Modificators}");
            };
        }

        public void Restart()
        {

        }

        public void Update()
        {
            if (TotalPieces != 0)
            {
                Pps = TotalPieces / Time.TimeSinceSceneLoad;
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            batcher.DrawString(Graphics.Instance.BitmapFont, 
                $"TP: {TotalPieces}\n" +
                $"PPS: {Pps:F1}\n" +
                $"Pieces on the board: {Board.PiecesOnBoard}\n" +
                $"Current Gravity: {Board.CurrentGravity:F3}\n" +
                $"Current Lock: {Board.CurrentLock:F2}", 
                Entity.Position, Microsoft.Xna.Framework.Color.Red);
        }
    }
}