using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaMonogame;
using PandaMonogame.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GameCore
{
    public class GameplayState : GameState
    {
        protected int _nextGameState = (int)GameStateType.None;

        protected PUIMenu _menu = new PUIMenu();

        protected BasicCamera2D _camera;
        protected WorldManager _worldManager;

        protected bool _mouseDragging = false;
        protected Vector2 _mouseDragPosition = Vector2.Zero;

        protected bool _showDebug = true;

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            _menu.Load(graphics, "GameplayMenuDefinition", "UITemplates");

            _camera = new BasicCamera2D(new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight), Rectangle.Empty);

            _worldManager = new WorldManager();
        }

        public override int Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.Position.X, mouseState.Position.Y);
            var worldPos = _camera.ScreenToWorldPosition(mousePosition);

            _menu.GetWidget<PUIWLabel>("lblCamera").Text = _camera.GetViewRect().ToString() + " : " + _camera.Zoom;
            _menu.GetWidget<PUIWLabel>("lblWorld").Text = "Asteroids: " + (_worldManager.Asteroids.LastActiveIndex + 1);
            _menu.GetWidget<PUIWLabel>("lblGeneral").Text = worldPos.ToString() + " : " + _worldManager.PlayerEntity.Rotation;// + " : " + _worldManager.PlayerEntity.TargetRotation;

            _menu.Update(gameTime);
            _worldManager.Update(gameTime);

            return _nextGameState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);
            
            // world space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
            {
                _worldManager.Draw(spriteBatch);
            }
            spriteBatch.End();

            // screen space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            {
                _menu.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        public override void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            _menu.OnMouseMoved(originalPosition, gameTime);

            if (!_menu.Focused)
            {
                if (_mouseDragging)
                {
                    var difference = mousePosition - _mouseDragPosition;
                    difference /= _camera.Zoom;
                    _camera.OffsetPosition(-difference);
                    _mouseDragPosition = mousePosition;
                }
            }
        }

        public override void OnMouseDown(MouseButtonID button, GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            _menu.OnMouseDown(button, gameTime);

            if (!_menu.Focused)
            {
                if (!_mouseDragging && button == MouseButtonID.Left)
                {
                    _mouseDragging = true;
                    _mouseDragPosition = mousePosition;
                }
            }
        }

        public override void OnMouseClicked(MouseButtonID button, GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            _menu.OnMouseClicked(button, gameTime);

            if (!_menu.Focused)
            {
                if (_mouseDragging && button == MouseButtonID.Left)
                    _mouseDragging = false;
                
                if (button == MouseButtonID.Right)
                {
                    var worldPos = _camera.ScreenToWorldPosition(mousePosition);
                    //_worldManager.PlayerEntity.Destination = worldPos;
                    _worldManager.PlayerEntity.SetDestination(worldPos);

                    //_worldManager.PlayerEntity.Rotation = (float)Math.Atan2(
                    //        (double)(worldPos.X - _worldManager.PlayerEntity.Position.X),
                    //        (double)(_worldManager.PlayerEntity.Position.Y - worldPos.Y));
                }
            }
        }

        public override void OnMouseScroll(MouseScrollDirection direction, int scrollValue, GameTime gameTime)
        {
            if (_menu.Focused)
            {
                _menu.OnMouseScroll(direction, scrollValue, gameTime);
                return;
            }

            if (direction == MouseScrollDirection.Up)
            {
                _camera.Zoom += 0.2f;
                if (_camera.Zoom > 4.0f)
                    _camera.Zoom = 4.0f;
            }
            else
            {
                _camera.Zoom -= 0.2f;
                if (_camera.Zoom < 0.2f)
                    _camera.Zoom = 0.2f;
            }
        }

        public override void OnKeyPressed(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnKeyPressed(key, gameTime, currentKeyState);
        }

        public override void OnKeyReleased(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            if (_menu.Focused)
            {
                _menu.OnKeyReleased(key, gameTime, currentKeyState);
                return;
            }

            if (key == Keys.OemTilde)
            {
                _showDebug = !_showDebug;
                _menu.GetFrame("debugFrame").Visible = _showDebug;
                _menu.GetFrame("debugFrame").Active = _showDebug;
            }
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
