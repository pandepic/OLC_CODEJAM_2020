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
    public class MainMenuState : GameState
    {
        protected int _nextGameState = (int)GameStateType.None;

        protected PUIMenu _menu = new PUIMenu();
        protected Sprite _menuBG = null;

        #region Python Methods
        protected void StartNewGame(params object[] args)
        {
            _nextGameState = (int)GameStateType.Loading;
        }

        protected void Settings(params object[] args)
        {
            _nextGameState = (int)GameStateType.Settings;
        }

        protected void Exit(params object[] args)
        {
            _nextGameState = (int)GameStateType.Exit;
        }
        #endregion

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            _menu.AddMethod(StartNewGame);
            _menu.AddMethod(Settings);
            _menu.AddMethod(Exit);
            _menu.Load(graphics, "MainMenuDefinition", "UITemplates");

            _menuBG = new Sprite(ModManager.Instance.AssetManager.LoadTexture2D(graphics, "MenuBG"));
            _menuBG.Position = Vector2.Zero;
            _menuBG.Center = Vector2.Zero; // otherwise scaling is weird
            _menuBG.Scale = ((float)graphics.PresentationParameters.BackBufferWidth / (float)_menuBG.Texture.Width);
        }

        public override int Update(GameTime gameTime)
        {
            return _nextGameState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _menuBG.Draw(spriteBatch);
            _menu.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            _menu.OnMouseMoved(originalPosition, gameTime);
        }
        public override void OnMouseDown(MouseButtonID button, GameTime gameTime)
        {
            _menu.OnMouseDown(button, gameTime);
        }

        public override void OnMouseClicked(MouseButtonID button, GameTime gameTime)
        {
            _menu.OnMouseClicked(button, gameTime);
        }

        public override void OnMouseScroll(MouseScrollDirection direction, int scrollValue, GameTime gameTime)
        {
            _menu.OnMouseScroll(direction, scrollValue, gameTime);
        }

        public override void OnKeyPressed(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyPressed(key, gameTime, currentKeyState);
        }

        public override void OnKeyReleased(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyReleased(key, gameTime, currentKeyState);
        }

        public override void OnKeyDown(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyDown(key, gameTime, currentKeyState);
        }

        public override void OnTextInput(TextInputEventArgs e, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnTextInput(e, gameTime, currentKeyState);
        }
    }
}
