﻿using System.Collections.Generic;
using System.Linq;
using ColdClearNet;
using Nez;
using Quader.Components.Boards;
using Quader.Components.Boards.PieceHandlers;
using Quader.Engine;

namespace Quader.Components
{
    public class BoardHolder
    {
        public Board Board { get; }
        public Entity BoardEntity { get; }
        public IEnumerable<Component> Components { get; }
        public IPieceHandler PieceHandler { get; }

        private List<IResetable> _resetableComponents;
        private List<IBoardToggleable> _boardToggleableComponents;
        private List<IBoardComponent> _boardComponents;

        public BoardHolder(Board board, Entity boardEntity, IEnumerable<Component> components, IPieceHandler pieceHandler)
        {
            Board = board;
            BoardEntity = boardEntity;
            Components = components.ToList();
            PieceHandler = pieceHandler;

            _resetableComponents = Components.OfType<IResetable>().ToList();
            _boardToggleableComponents = Components.OfType<IBoardToggleable>().ToList();
            _boardComponents = Components.OfType<IBoardComponent>().ToList();
        }

        public void Start()
        {
            PieceHandler.Start();
        }

        public void Reset()
        {
            foreach (var c in _resetableComponents)
            {
                c.Reset();
            }
        }

        public void Toggle()
        {
            foreach (var c in _boardToggleableComponents)
            {
                c.Toggle();
            }
        }


    }
}