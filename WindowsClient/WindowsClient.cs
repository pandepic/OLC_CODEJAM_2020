using GameCore;
using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaMonogame;
using PandaMonogame.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsClient
{
    public class WindowsClient : Game
    {
        protected GraphicsDeviceManager _graphics;
        protected SpriteBatch _spriteBatch;

        protected GameTime _lastGameTime = null;
        protected GameState _currentGameState = null;

        protected readonly KeyboardManager _keyboardManager = new KeyboardManager();
        protected readonly MouseManager _mouseManager = new MouseManager();

        protected bool _assetsLoaded = false;

        TimeSpan _frameCounterElapsedTime = TimeSpan.Zero;
        int _frameCounter = 0;
        long _drawMS, _updateMS;
        readonly string _windowTitle = "WAR MACHINE - OLC CODEJAM 2020";

        public WindowsClient()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                GraphicsProfile = GraphicsProfile.HiDef
            };
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Content.RootDirectory = "";
            IsMouseVisible = true;

            //PandaMonogameConfig.Logging = true;

            ModManager.Instance.Init(Content);
            ModManager.Instance.LoadList("Mods", "mods.xml", "assets.xml");
            ModManager.Instance.ImportAssets();
            ModManager.Instance.AssetManager.Import("Assets\\assets.xml");

            SettingsManager.Instance.Load("Assets\\settings.xml");

#if DEBUG
            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
#endif

            _graphics.PreferredBackBufferWidth = SettingsManager.Instance.GetSetting<int>("window", "width");
            _graphics.PreferredBackBufferHeight = SettingsManager.Instance.GetSetting<int>("window", "height");
            _graphics.ApplyChanges();

            ModManager.Instance.SoundManager.SetVolume((int)SoundType.Music, SettingsManager.Instance.GetSetting<float>("sound", "musicvolume"));
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.SoundEffect, SettingsManager.Instance.GetSetting<float>("sound", "sfxvolume"));
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.UI, SettingsManager.Instance.GetSetting<float>("sound", "uivolume"));

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Sprites.Load(GraphicsDevice);
            PUITooltipManager.Setup(GraphicsDevice, Sprites.DefaultFont);
            WorldData.Load();
            EntityData.Load();

            //ChangeGameState((int)GameStateType.Startup);
            ChangeGameState((int)GameStateType.Menu);

            Window.TextInput += Window_TextInput;
        }

        protected void Window_TextInput(object sender, TextInputEventArgs e)
        {
            if (_lastGameTime == null)
                return;

            _keyboardManager.TextInput(e, _lastGameTime);
        }

        protected override void UnloadContent()
        {
            PandaUtil.Cleanup();
        }

        protected override void Update(GameTime gameTime)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            ModManager.Instance.SoundManager.Update();

            _lastGameTime = gameTime;

            if (IsActive)
            {
                _keyboardManager.Update(gameTime);
                _mouseManager.Update(gameTime);
            }

            int newState = _currentGameState.Update(gameTime);

            if (newState == (int)GameStateType.Exit)
            {
                Exit();
                return;
            }

            if (newState != (int)GameStateType.None)
                ChangeGameState(newState);

            // fps counter
            _frameCounter++;
            _frameCounterElapsedTime += gameTime.ElapsedGameTime;

            if (_frameCounterElapsedTime >= TimeSpan.FromSeconds(1))
            {
                Window.Title = string.Format("{0} {1} fps - Draw {2}ms Update {3}ms", _windowTitle, _frameCounter, _drawMS, _updateMS);
                _frameCounter = 0;
                _frameCounterElapsedTime -= TimeSpan.FromSeconds(1);
            }

            watch.Stop();
            _updateMS = watch.ElapsedMilliseconds;
        }

        private void ChangeGameState(int newState)
        {
            //ModManager.Instance.AssetManager.Clear();

            switch (newState)
            {
                case (int)GameStateType.Startup:
                    {
                        _currentGameState = new StartupState();
                    }
                    break;

                case (int)GameStateType.Menu:
                    {
                        _currentGameState = new MainMenuState();
                    }
                    break;

                case (int)GameStateType.Play:
                    {
                        _currentGameState = new GameplayState();
                    }
                    break;

                case (int)GameStateType.Settings:
                    {
                        return;
                    }
                    break;
            }

            _currentGameState.Load(Content, GraphicsDevice);
            _keyboardManager.OnKeyDown = new KEYBOARD_EVENT(_currentGameState.OnKeyDown);
            _keyboardManager.OnKeyPressed = new KEYBOARD_EVENT(_currentGameState.OnKeyPressed);
            _keyboardManager.OnKeyReleased = new KEYBOARD_EVENT(_currentGameState.OnKeyReleased);
            _keyboardManager.OnTextInput = new TEXTINPUT_EVENT(_currentGameState.OnTextInput);
            _mouseManager.OnMouseClicked = new MOUSEBUTTON_EVENT(_currentGameState.OnMouseClicked);
            _mouseManager.OnMouseDown = new MOUSEBUTTON_EVENT(_currentGameState.OnMouseDown);
            _mouseManager.OnMouseMoved = new MOUSEPOSITION_EVENT(_currentGameState.OnMouseMoved);
            _mouseManager.OnMouseScroll = new MOUSESCROLL_EVENT(_currentGameState.OnMouseScroll);
        } // changeGameState

        protected override void Draw(GameTime gameTime)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            _currentGameState.Draw(gameTime, GraphicsDevice, _spriteBatch);

            watch.Stop();
            _drawMS = watch.ElapsedMilliseconds;
        }
    }
}
