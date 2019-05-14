using System;
using System.Collections.Generic;
using JourneyCoreLib.Drawing;
using JourneyCoreLib.Game.Context.Entities;
using JourneyCoreLib.Game.InputWatchers;
using JourneyCoreLib.Rendering.Environment.Tiling;
using JourneyCoreLib.System;
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
        private ButtonWatcher _buttonWatcher;


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

            Font font = new Font(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Fonts\Courier New.ttf");

            _wManager.DrawPersistent(new DrawQueueItem(DrawPriority.UI, (fTime, window) =>
            {
                window.Draw(new Text($"{Math.Sin(_player.Graphic.Rotation * (Math.PI / 180))}", font)
                {
                    Position = new Vector2f(0f, 0f)
                });
                window.Draw(new Text($"{Math.Cos(_player.Graphic.Rotation * (Math.PI / 180))}", font)
                {
                    Position = new Vector2f(0f, 50f)
                });
                window.Draw(new Text(_player.Graphic.Rotation.ToString(), font)
                {
                    Position = new Vector2f(0f, 100f)
                });
            }));
        }



        #region INITIALISATION

        private void InitialiseTiles()
        {
            TileLoader.LoadTiles(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Util\Tiled_TileSheets\JourneyCore-MapTileSet.xml");

        }

        private void InitialiseWindowManager()
        {
            _wManager = new WindowManager("Journey to the Core", new VideoMode(1000, 600, 8), 300, new Vector2f(2f, 2f), 15f);
        }

        private void InitialisePlayer()
        {
            _player = new Entity(_gameContext, "player", "player", new Texture(@"C:\Users\semiv\OneDrive\Documents\Programming\CSharp\JourneyCore\JourneyCoreGame\Assets\Images\Sprites\Human.png"));
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
                Vector2f movement = new Vector2f(RadianMath.SinFromDegrees(_player.Graphic.Rotation), RadianMath.CosFromDegrees(_player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                _player.MoveEntity(movement);
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.A, (key) =>
            {
                Vector2f movement = new Vector2f(RadianMath.CosFromDegrees(_player.Graphic.Rotation) * -1f, RadianMath.SinFromDegrees(_player.Graphic.Rotation) * -1f);

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                _player.MoveEntity(movement);
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.S, (key) =>
            {
                Vector2f movement = new Vector2f(RadianMath.SinFromDegrees(_player.Graphic.Rotation) * -1f, RadianMath.CosFromDegrees(_player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.A) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    movement *= 0.5f;
                }

                _player.MoveEntity(movement);
            });

            _keyWatcher.AddWatchedKeyAction(Keyboard.Key.D, (key) =>
            {
                Vector2f movement = new Vector2f(RadianMath.CosFromDegrees(_player.Graphic.Rotation), RadianMath.SinFromDegrees(_player.Graphic.Rotation));

                if (Keyboard.IsKeyPressed(Keyboard.Key.W) || Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    movement *= 0.5f;
                }

                _player.MoveEntity(movement);
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
