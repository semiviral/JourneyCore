using System;
using System.Collections.Generic;
using JourneyCoreLib.Drawing;
using JourneyCoreLib.Game.Context.Entities;
using JourneyCoreLib.Game.Keys;
using JourneyCoreLib.Rendering.Environment.Tiling;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCoreLib.Game
{
    public class GameLoop
    {
        public static Vector2i MapTileSize { get; private set; }

        private WindowManager _wManager;
        private Context.Context _gameContext;
        private TileMap _currentMap;
        private Entity _player;
        private KeyWatcher _keyWatcher;


        public GameLoop()
        {
            _gameContext = new Context.Context(null, "GameContext", "game");

            InitialiseTiles();

            _currentMap = TileMap.LoadTileMap("AdventurersGuild", new Vector2i(8, 8), 1);
            MapTileSize = new Vector2i(_currentMap.PixelTileWidth, _currentMap.PixelTileHeight);

            InitialiseWindowManager();
            InitialiseKeyWatcher();
            InitialisePlayer();
            InitialiseView();
        }



        #region INITIALISATION

        private void InitialiseTiles()
        {
            TileLoader.LoadTiles(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Util\Tiled_TileSheets\JourneyCore-MapTileSet.xml");

        }

        private void InitialiseWindowManager()
        {
            _wManager = new WindowManager("Journey to the Core", new VideoMode(1920, 1080, 8), 300, new Vector2f(2f, 2f), 15f);
        }

        private void InitialisePlayer()
        {
            _player = new Entity(_gameContext, _wManager, "player", "player", new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\Human.png"));
            _wManager.DrawPersistent(new DrawQueueItem(DrawPriority.Foreground, (fTime, window) =>
            {
                _keyWatcher.CheckWatchedKeys();
                window.Draw(_player.Graphic);
            }));
        }

        private void InitialiseView()
        {
            _wManager.SetView(new View(_player.Graphic.Position, new Vector2f(200f, 200f)));

            _player.EntityView.PositionChanged += OnPlayerPositionChanged;
            _player.EntityView.RotationChanged += OnPlayerRotationChanged;
        }

        private void InitialiseKeyWatcher()
        {
            _keyWatcher = new KeyWatcher();

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.W, (key) =>
            {
                _player.MoveEntity(new Vector2f(0f, -1f));
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.A, (key) =>
            {
                _player.MoveEntity(new Vector2f(-1f, 0f));
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.S, (key) =>
            {
                _player.MoveEntity(new Vector2f(0f, 1f));
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.D, (key) =>
            {
                _player.MoveEntity(new Vector2f(1f, 0f));
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.Q, (key) =>
            {
                _player.RotateEntity(180f, false);
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.E, (key) =>
            {
                _player.RotateEntity(180f, true);
            });
        }

        #endregion

        public void Runtime()
        {
            _currentMap.LoadChunkRange(0, 0, 16, 16);

            //Shader transparency = new Shader(null, null, @"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Shaders\transparency.frag");
            //transparency.SetUniform("opacity", 0.5f);
            //transparency.SetUniform("texture", Map.MapTextures);

            RenderStates overlayStates = new RenderStates(TileMap.MapTextures);

            _wManager.DrawPersistent(new DrawQueueItem(DrawPriority.Background, (fTime, window) =>
            {
                window.Draw(_currentMap.VArray, overlayStates);
            }));

            while (_wManager.IsActive)
            {
                _wManager.UpdateWindow();
            }
        }


        public void LoadMap(string mapName)
        {
            _currentMap = TileMap.LoadTileMap(mapName, new Vector2i(8, 8), 2);
        }

        #region EVENT

        private void OnPlayerPositionChanged(object sender, View args)
        {
            _wManager.SetView(args);
        }

        private void OnPlayerRotationChanged(object sender, View args)
        {
            _wManager.SetView(args);
        }

        #endregion
    }
}
