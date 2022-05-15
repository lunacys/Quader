﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez;
using Nez.ImGuiTools;
using Nez.Persistence;
using Nez.Systems;
using Nez.UI;
using Quader.Audio;
using Quader.Config;
using Quader.Debugging.Logging;
using Quader.Debugging.Logging.Loggers;
using Quader.Engine.Pieces;
using Quader.Scenes;
using Quader.Serialization;
using Quader.Skinning;

namespace Quader
{
    public class GameRoot : Core
    {
        private readonly string _configFilePath = "config.json";

        public static JsonSettings DefaultJsonSettings { get; private set; } = null!;

        private readonly ILogger _logger = LoggerFactory.GetLogger<GameRoot>();

        public GameRoot()
            : base(windowTitle: "Quader")
        {
            _logger.Debug("Setting up");

            Window.AllowUserResizing = false;

            IsFixedTimeStep = false;
            TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 240.0);

            _logger.Info("Initializing FMOD");
            
            FMODManager.Init(FMODMode.Core, "Content");

            _logger.Info("Loading Piece Settings");
            DefaultJsonSettings = new JsonSettings
            {
                TypeConverters = new JsonTypeConverter[]
                {
                    new ColorJsonConverter(),
                    new ColorFactoryConverter()
                },
                PrettyPrint = true
            };

            var pieceSettingsFilename = Path.Combine("Content", "data", "default_piece_settings.json");
            PieceSettings pieceSettings;

            if (!File.Exists(pieceSettingsFilename))
            {
                _logger.Warn($"Piece Settings file does not exist ({pieceSettingsFilename}), taking the defaults");
                using var sw = new StreamWriter(pieceSettingsFilename, false);
                pieceSettings = new PieceSettings();
                sw.WriteLine(Json.ToJson(pieceSettings, DefaultJsonSettings));
            }
            else
            {
                using var sr = new StreamReader(pieceSettingsFilename);
                var json = sr.ReadToEnd();
                pieceSettings = Json.FromJson<PieceSettings>(json, DefaultJsonSettings);
                _logger.Info("Successfully loaded Piece Settings from file");
            }

            PieceUtils.PieceSettings = pieceSettings;
        }
        
        protected override void Initialize()
        {
            _logger.Info("Initializing");

            base.Initialize();

            _logger.Info("Creating global ImGUI Manager");
            var imGuiManager = new ImGuiManager();
            Core.RegisterGlobalManager(imGuiManager);

            _logger.Info("Creating skin");
            Skin skin = Skin.CreateDefaultSkin();

            _logger.Info("Loading content files");
            var skinTexture = Content.LoadTexture("Content/skins/default_3.png");
            var boardTexture = Content.LoadTexture("Content/skins/board_default.png");
            skin.Add("board_skin", new BoardSkin(skinTexture, boardTexture));
            Services.AddService(typeof(Skin), skin);

            /*var sound = CoreSystem.LoadStreamedSound("test.mp3");
            var channel = sound.Play();
            channel.Volume = 0.1f;
            channel.Looping = true;*/

            _logger.Info($"Loading Game Config ({_configFilePath})");
            GameConfig gc;

            try
            {
                gc = GameConfig.LoadFromFile(_configFilePath);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Config file was not found, taking the defaults");
                gc = new GameConfig();
            }
            catch (Exception e)
            {
                throw;
            }

            Services.AddService(gc);

            _logger.Info("Changing current scene");
            Scene = new GameplayScene();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();

            _logger.Info("Unloading content");

            var gc = Services.GetService<GameConfig>();
            GameConfig.SaveToFile(gc, _configFilePath);

            FMODManager.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            FMODManager.Update();
        }
    }
}
