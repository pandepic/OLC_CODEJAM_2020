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
    public class StartupState : GameState
    {
        protected Sprite _logo;
        protected int _nextGameState = (int)GameStateType.None;

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            var screenWidth = graphics.PresentationParameters.BackBufferWidth;
            var screenHeight = graphics.PresentationParameters.BackBufferHeight;

            _logo = new Sprite(Content.Load<Texture2D>("Assets\\UI\\pandepiclogotrans"));
            _logo.Position = new Vector2(800, 400);
            _logo.SetTransparency(0);
            _logo.BeginFadeEffect(255.0f, 5000.0f);
            _logo.Scale = 0.2f;
            _logo.BeginScalingEffect(1.0f, 5000.0f);
        }

        public override int Update(GameTime gameTime)
        {
            _logo.Update(gameTime);

            return _nextGameState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            _logo.Draw(spriteBatch);
            spriteBatch.End();
        }

        public override void OnMouseMoved(Vector2 originalPosition, GameTime gameTime) { }
        public override void OnMouseDown(MouseButtonID button, GameTime gameTime) { }
        public override void OnMouseClicked(MouseButtonID button, GameTime gameTime)
        {
            _nextGameState = (int)GameStateType.Menu;
        }

        public override void OnMouseScroll(MouseScrollDirection direction, int scrollValue, GameTime gameTime) { }

        public override void OnKeyPressed(Keys key, GameTime gameTime, CurrentKeyState currentKeyState) { }

        public override void OnKeyReleased(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _nextGameState = (int)GameStateType.Menu;
        }

        public override void OnKeyDown(Keys key, GameTime gameTime, CurrentKeyState currentKeyState) { }
        public override void OnTextInput(TextInputEventArgs e, GameTime gameTime, CurrentKeyState currentKeyState) { }
    }
}
