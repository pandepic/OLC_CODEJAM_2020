using GameCore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
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

            SettingsManager.Instance.Load("Assets\\settings.xml");

            _graphics.PreferredBackBufferWidth = SettingsManager.Instance.GetSetting<int>("window", "width");
            _graphics.PreferredBackBufferHeight = SettingsManager.Instance.GetSetting<int>("window", "height");
            _graphics.ApplyChanges();

            ModManager.Instance.SoundManager.SetVolume((int)SoundType.Music, SettingsManager.Instance.GetSetting<float>("sound", "musicvolume"));
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.SoundEffect, SettingsManager.Instance.GetSetting<float>("sound", "sfxvolume"));
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.UI, SettingsManager.Instance.GetSetting<float>("sound", "uivolume"));

            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

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

        protected override void Draw(GameTime gameTime)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            GraphicsDevice.Clear(Color.Red);

            watch.Stop();
            _drawMS = watch.ElapsedMilliseconds;
        }
    }
}
