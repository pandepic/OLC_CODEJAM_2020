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

        public static BasicCamera2D Camera;
        public static WorldManager WorldManager;
        public static UnitManager UnitManager;

        protected bool _mouseDragging = false;
        protected Vector2 _mouseDragPosition = Vector2.Zero;

        protected bool _lockCamera = false;
        protected bool _showDebug = false;

        protected float ScrollSpeed = 1500.0f;

        protected List<float> _zLevels = new List<float>()
        {
            1.0f,
            2.0f,
            5.0f
        };
        protected int _currentZLevel = 0;

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            _menu.Load(graphics, "GameplayMenuDefinition", "UITemplates");

            Camera = new BasicCamera2D(
                new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight),
                new Rectangle(0, 0, WorldManager.WorldWidth, WorldManager.WorldHeight));

            _currentZLevel = _zLevels.IndexOf(2.0f);
            Camera.Z = _zLevels[_currentZLevel];

            UnitManager = new UnitManager();
            WorldManager = new WorldManager();

            UnitManager.Setup(graphics, Camera, _menu);
            WorldManager.Setup(graphics, Camera, UnitManager);
            WorldManager.PlayerEntity.UnitManager = UnitManager;

            Camera.CenterPosition(WorldManager.PlayerEntity.Position);
        }

        public override int Update(GameTime gameTime)
        {
            var mousePosition = Screen.GetMousePosition();
            var mouseWorldPos = Camera.ScreenToWorldPosition(mousePosition);
            var centerWorldPos = Camera.ScreenToWorldPosition(WorldManager.ScreenCenter);

            if (_lockCamera)
                Camera.CenterPosition(WorldManager.PlayerEntity.Position);

            _menu.GetWidget<PUIWLabel>("lblDebug").Text =
                Camera.GetViewRect().ToString() + " : " + Camera.Zoom + "\n" +
                "Asteroids: " + (WorldManager.Asteroids.LastActiveIndex + 1) + "\n" +
                mouseWorldPos.ToString() + "\n" +
                centerWorldPos.ToString();

            var inventoryString = new StringBuilder();

            foreach (var kvp in WorldManager.PlayerEntity.Inventory.Resources)
                inventoryString.Append(kvp.Key.ToString() + ": " + kvp.Value.ToString() + "\n");

            _menu.GetWidget<PUIWLabel>("lblInventory").Text = inventoryString.ToString();

            _menu.Update(gameTime);
            WorldManager.Update(gameTime);
            UnitManager.Update(gameTime);

            return _nextGameState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            // screen space
            float backgroundZ = -50f; // Background distance.
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix(backgroundZ));
            {
                WorldManager.DrawScreen(spriteBatch, backgroundZ);
            }
            spriteBatch.End();

            // world space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix());
            {
                WorldManager.DrawWorld(spriteBatch);
                UnitManager.DrawWorld(spriteBatch);
            }
            spriteBatch.End();

            // screen space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            {
                _menu.Draw(spriteBatch);
                UnitManager.DrawScreen(spriteBatch);
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
                    difference /= Camera.Zoom;
                    Camera.OffsetPosition(-difference);
                    _mouseDragPosition = mousePosition;
                }

                UnitManager.OnMouseMoved(originalPosition, gameTime);
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

                UnitManager.OnMouseDown(button, gameTime);
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

                UnitManager.OnMouseClicked(button, gameTime);
            }
        }

        public override void OnMouseScroll(MouseScrollDirection direction, int scrollValue, GameTime gameTime)
        {
            if (_menu.Focused)
            {
                _menu.OnMouseScroll(direction, scrollValue, gameTime);
                return;
            }

            if (direction == MouseScrollDirection.Down)
            {
                _currentZLevel += 1;

                if (_currentZLevel >= _zLevels.Count)
                    _currentZLevel = _zLevels.Count - 1;

                Camera.Z = _zLevels[_currentZLevel];
                Camera.CheckBoundingBox();
            }
            else
            {
                _currentZLevel -= 1;

                if (_currentZLevel < 0)
                    _currentZLevel = 0;

                Camera.Z = _zLevels[_currentZLevel];
                Camera.CheckBoundingBox();
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
                //_lockCamera = !_lockCamera;
                Camera.CenterPosition(WorldManager.PlayerEntity.Position);
            }
            else if (key == Keys.P)
            {
                WorldManager.PlayerEntity.Inventory.AddResource(ResourceType.Metal, 100);
                WorldManager.PlayerEntity.Inventory.AddResource(ResourceType.Gas, 50);
            }
            else if (key == Keys.O)
            {
                WorldManager.PlayerEntity.BuildShip(ShipType.Miner);
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
                    Camera.OffsetPosition(new Vector2(0, -ScrollSpeed / Camera.Zoom) * gameTime.DeltaTime());
                    break;

                case Keys.S:
                case Keys.Down:
                    Camera.OffsetPosition(new Vector2(0, ScrollSpeed / Camera.Zoom) * gameTime.DeltaTime());
                    break;

                case Keys.A:
                case Keys.Left:
                    Camera.OffsetPosition(new Vector2(-ScrollSpeed / Camera.Zoom, 0) * gameTime.DeltaTime());
                    break;

                case Keys.D:
                case Keys.Right:
                    Camera.OffsetPosition(new Vector2(ScrollSpeed / Camera.Zoom, 0) * gameTime.DeltaTime());
                    break;
            }
        }

        public override void OnTextInput(TextInputEventArgs e, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            _menu.OnTextInput(e, gameTime, currentKeyState);
        }
    }
}
