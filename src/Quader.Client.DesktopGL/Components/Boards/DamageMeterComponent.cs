﻿using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using Quader.Engine;
using Quader.Utils;

namespace Quader.Components.Boards
{
    public class DamageMeterComponent : RenderableComponent, IBoardComponent, IResetable
    {
        public override float Width => 16;
        public override float Height => 1000;

        public Board Board { get; }

        private RenderTarget2D _renderTarget;

        public DamageMeterComponent(Board board)
        {
            Board = board;

            Board.AttackReceived += (sender, attack) =>
            {
                _renderTarget?.RenderFrom(RenderToTexture);
            };
            Board.PieceHardDropped += (sender, move) =>
            {
                _renderTarget?.RenderFrom(RenderToTexture);
            };
        }

        public override void Initialize()
        {
            base.Initialize();

            _renderTarget = RenderTarget.Create(30, (int) Height);
        }

        private void RenderToTexture(Batcher batcher)
        {
            var d = Board.IncomingDamage.ToList();
            int total = 0;

            for (int i = 0; i < d.Count; i++)
            {
                var a = d[i];

                var drawX = 0;
                var drawY = 0;

                drawY += total * 32;

                batcher.DrawRect(drawX, drawY, 16, a * 32, Color.Red);
                batcher.DrawHollowRect(drawX, drawY, 16, a * 32, Color.Yellow, 2);

                total += a;
            }
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (_renderTarget != null)
            {
                var drawPos = new Vector2(
                    Entity.Position.X + 320,
                    Entity.Position.Y - Height /2f + 140
                );

                batcher.Draw(_renderTarget, drawPos, null, Color.White, 0f, Vector2.Zero, 1,
                    SpriteEffects.FlipVertically, 0f);
            }
        }

        public void Reset()
        {
            _renderTarget?.RenderFrom(RenderToTexture);
        }
    }
}