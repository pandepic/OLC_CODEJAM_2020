using GameCore.AI;
using GameCore.Entities;
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
        protected UnitManager _unitManager;

        protected bool _mouseDragging = false;
        protected Vector2 _mouseDragPosition = Vector2.Zero;

        protected bool _lockCamera = false;
        protected bool _showDebug = false;

        protected float ScrollSpeed = 1500.0f;

        protected List<float> _zoomLevels = new List<float>()
        {
            0.2f,
            0.4f,
            1.0f,
        };
        protected int _currentZoomLevel = 0;

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            _menu.Load(graphics, "GameplayMenuDefinition", "UITemplates");

            _camera = new BasicCamera2D(
                new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight),
                new Rectangle(0, 0, WorldManager.WorldWidth, WorldManager.WorldHeight));

            _currentZoomLevel = _zoomLevels.IndexOf(1.0f);
            _camera.Zoom = _zoomLevels[_currentZoomLevel];

            _unitManager = new UnitManager(graphics, _camera, _menu);
            _worldManager = new WorldManager(graphics, _camera, _unitManager);
            _worldManager.PlayerEntity.UnitManager = _unitManager;

            _camera.CenterPosition(_worldManager.PlayerEntity.Position);
        }

        public override int Update(GameTime gameTime)
        {
            var mousePosition = Screen.GetMousePosition();
            var mouseWorldPos = _camera.ScreenToWorldPosition(mousePosition);
            var centerWorldPos = _camera.ScreenToWorldPosition(_worldManager.ScreenCenter);

            if (_lockCamera)
                _camera.CenterPosition(_worldManager.PlayerEntity.Position);

            _menu.GetWidget<PUIWLabel>("lblDebug").Text =
                _camera.GetViewRect().ToString() + " : " + _camera.Zoom + "\n" +
                "Asteroids: " + (_worldManager.Asteroids.LastActiveIndex + 1) + "\n" +
                mouseWorldPos.ToString() + "\n" +
                centerWorldPos.ToString();

            var inventoryString = new StringBuilder();

            foreach (var kvp in _worldManager.PlayerEntity.Inventory.Resources)
                inventoryString.Append(kvp.Key.ToString() + ": " + kvp.Value.ToString() + "\n");

            _menu.GetWidget<PUIWLabel>("lblInventory").Text = inventoryString.ToString();

            _menu.Update(gameTime);
            _worldManager.Update(gameTime);
            _unitManager.Update(gameTime);

            return _nextGameState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            // screen space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            {
                _worldManager.DrawScreen(spriteBatch);
            }
            spriteBatch.End();

            // world space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _camera.GetViewMatrix());
            {
                _worldManager.DrawWorld(spriteBatch);
                _unitManager.DrawWorld(spriteBatch);
            }
            spriteBatch.End();

            // screen space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            {
                _menu.Draw(spriteBatch);
                _unitManager.DrawScreen(spriteBatch);
            }
            spriteBatch.End();
        }

        public override void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            var mousePosition = Screen.GetMousePosition();

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

                _unitManager.OnMouseMoved(originalPosition, gameTime);
            }
        }

        public override void OnMouseDown(MouseButtonID button, GameTime gameTime)
        {
            var mousePosition = Screen.GetMousePosition();

            _menu.OnMouseDown(button, gameTime);

            if (!_menu.Focused)
            {
                if (!_mouseDragging && button == MouseButtonID.Middle)
                {
                    _mouseDragging = true;
                    _mouseDragPosition = mousePosition;
                }

                _unitManager.OnMouseDown(button, gameTime);
            }
        }

        public override void OnMouseClicked(MouseButtonID button, GameTime gameTime)
        {
            var mousePosition = Screen.GetMousePosition();

            _menu.OnMouseClicked(button, gameTime);

            if (!_menu.Focused)
            {
                if (_mouseDragging && button == MouseButtonID.Middle)
                    _mouseDragging = false;

                _unitManager.OnMouseClicked(button, gameTime);
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
                _currentZoomLevel += 1;

                if (_currentZoomLevel >= _zoomLevels.Count)
                    _currentZoomLevel = _zoomLevels.Count - 1;

                _camera.Zoom = _zoomLevels[_currentZoomLevel];
                _camera.CheckBoundingBox();
            }
            else
            {
                _currentZoomLevel -= 1;

                if (_currentZoomLevel < 0)
                    _currentZoomLevel = 0;

                _camera.Zoom = _zoomLevels[_currentZoomLevel];
                _camera.CheckBoundingBox();
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
            else if (key == Keys.Space)
            {
                _lockCamera = !_lockCamera;
            }
            else if (key == Keys.P)
            {
                _worldManager.PlayerEntity.Inventory.AddResource(ResourceType.Metal, 100);
                _worldManager.PlayerEntity.Inventory.AddResource(ResourceType.Gas, 50);
            }
            else if (key == Keys.O)
            {
                _worldManager.PlayerEntity.BuildShip(ShipType.Miner);
            }
        }

        public override void OnKeyDown(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            if (_menu.Focused)
            {
                _menu.OnKeyDown(key, gameTime, currentKeyState);
                return;
            }

            switch (key)
            {
                case Keys.W:
                case Keys.Up:
                    _camera.OffsetPosition(new Vector2(0, -ScrollSpeed / _camera.Zoom) * gameTime.DeltaTime());
                    break;

                case Keys.S:
                case Keys.Down:
                    _camera.OffsetPosition(new Vector2(0, ScrollSpeed / _camera.Zoom) * gameTime.DeltaTime());
                    break;

                case Keys.A:
                case Keys.Left:
                    _camera.OffsetPosition(new Vector2(-ScrollSpeed / _camera.Zoom, 0) * gameTime.DeltaTime());
                    break;

                case Keys.D:
                case Keys.Right:
                    _camera.OffsetPosition(new Vector2(ScrollSpeed / _camera.Zoom, 0) * gameTime.DeltaTime());
                    break;
            }
        }

        public override void OnTextInput(TextInputEventArgs e, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnTextInput(e, gameTime, currentKeyState);
        }
    }
}
