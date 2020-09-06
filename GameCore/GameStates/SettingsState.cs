using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaMonogame;
using PandaMonogame.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public class SettingsState : GameState
    {
        protected int _newState = (int)GameStateType.None;
        protected PUIMenu _menu = new PUIMenu();
        protected Sprite _menuBG = null;

        #region python bound methods
        protected void updateMusicVolume(params object[] args)
        {
            var volume = float.Parse(args[0].ToString());
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.Music, volume);
            SettingsManager.Instance.UpdateSetting("sound", "musicvolume", volume.ToString());
        }

        protected void updateSFXVolume(params object[] args)
        {
            var volume = float.Parse(args[0].ToString());
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.SoundEffect, volume);
            SettingsManager.Instance.UpdateSetting("sound", "sfxvolume", volume.ToString());
        }

        protected void updateUIVolume(params object[] args)
        {
            var volume = float.Parse(args[0].ToString());
            ModManager.Instance.SoundManager.SetVolume((int)SoundType.UI, volume);
            SettingsManager.Instance.UpdateSetting("sound", "uivolume", volume.ToString());
        }

        protected void backToMenu(params object[] args)
        {
            _newState = (int)GameStateType.Menu;
        }
        #endregion

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            _menu.AddMethod(updateMusicVolume);
            _menu.AddMethod(updateSFXVolume);
            _menu.AddMethod(updateUIVolume);
            _menu.AddMethod(backToMenu);
            _menu.Load(graphics, "SettingsMenuDefinition", "UITemplates");

            var musicVolume = SettingsManager.Instance.GetSetting<float>("sound", "musicvolume");
            var sfxVolume = SettingsManager.Instance.GetSetting<float>("sound", "sfxvolume");
            var uiVolume = SettingsManager.Instance.GetSetting<float>("sound", "uivolume");

            _menu.GetWidget<PUIWHScrollBar>("scrlMusicVolume").FValue = musicVolume;
            _menu.GetWidget<PUIWHScrollBar>("scrlSFXVolume").FValue = sfxVolume;
            _menu.GetWidget<PUIWHScrollBar>("scrlUIVolume").FValue = uiVolume;

            _menuBG = new Sprite(ModManager.Instance.AssetManager.LoadTexture2D(graphics, "MenuBG"));
            _menuBG.Position = Vector2.Zero;
            _menuBG.Center = Vector2.Zero; // otherwise scaling is weird
            _menuBG.Scale = ((float)graphics.PresentationParameters.BackBufferWidth / (float)_menuBG.Texture.Width);
        }

        public override int Update(GameTime gameTime)
        {
            return _newState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _menuBG.Draw(spriteBatch);
            _menu.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void OnMouseDown(MouseButtonID button, GameTime gameTime)
        {
            _menu.OnMouseDown(button, gameTime);
        }

        public override void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            _menu.OnMouseMoved(originalPosition, gameTime);
        }

        public override void OnMouseClicked(MouseButtonID button, GameTime gameTime)
        {
            _menu.OnMouseClicked(button, gameTime);
        }

        public override void OnKeyDown(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyDown(key, gameTime, currentKeyState);
        }

        public override void OnKeyPressed(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyPressed(key, gameTime, currentKeyState);
        }

        public override void OnKeyReleased(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyReleased(key, gameTime, currentKeyState);
        }

        public override void OnTextInput(TextInputEventArgs e, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnTextInput(e, gameTime, currentKeyState);
        }
    }
}
