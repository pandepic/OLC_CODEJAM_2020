using GameCore.AI;
using GameCore.Combat;
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
        public static ProjectileManager ProjectileManager;

        protected bool _mouseDragging = false;
        protected Vector2 _mouseDragPosition = Vector2.Zero;

        protected bool _lockCamera = false;
        public static bool ShowDebug = false;

        protected float ScrollSpeed = 1500.0f;

        protected List<float> _zoomLevels = new List<float>()
        {
            0.2f,
            0.5f,
            1.0f,
        };
        protected int _currentZoomLevel = 0;

        protected PUIWLabel _lblMetal, _lblGas, _lblWater, _lblCrystal, _lblUranium;

        #region python bound methods
        protected void BuildMiner(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Miner);
        }

        protected void BuildFighter(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Fighter);
        }
        #endregion

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            _menu.AddMethod(BuildMiner);
            _menu.AddMethod(BuildFighter);
            _menu.Load(graphics, "GameplayMenuDefinition", "UITemplates");

            LoadProductionCostLabel(ShipType.Miner, _menu.GetWidget<PUIWLabel>("lblMinerCost"));
            LoadProductionCostLabel(ShipType.Fighter, _menu.GetWidget<PUIWLabel>("lblFighterCost"));

            _lblMetal = _menu.GetWidget<PUIWLabel>("lblMetal");
            _lblGas = _menu.GetWidget<PUIWLabel>("lblGas");
            _lblWater = _menu.GetWidget<PUIWLabel>("lblWater");
            _lblCrystal = _menu.GetWidget<PUIWLabel>("lblCrystal");
            _lblUranium = _menu.GetWidget<PUIWLabel>("lblUranium");

            Camera = new BasicCamera2D(
                new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight),
                new Rectangle(0, 0, WorldManager.WorldWidth, WorldManager.WorldHeight));

            _currentZoomLevel = _zoomLevels.IndexOf(0.5f);
            Camera.Zoom = _zoomLevels[_currentZoomLevel];

            UnitManager = new UnitManager();
            WorldManager = new WorldManager();
            ProjectileManager = new ProjectileManager();

            UnitManager.Setup(graphics, _menu);
            WorldManager.Setup(graphics);

            Camera.CenterPosition(WorldManager.PlayerEntity.Position);
        } // Load

        public void LoadProductionCostLabel(ShipType type, PUIWLabel label)
        {
            var cost = "";

            foreach (var kvp in EntityData.ShipTypes[type].BuildCost)
                cost += kvp.Key.ToString() + ": " + kvp.Value.ToString() + ", ";

            cost = cost.Remove(cost.LastIndexOf(","));

            label.Text = cost;
        } // LoadProductionCostLabel

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
                centerWorldPos.ToString() + "\n" +
                WorldManager.PlayerShips.Count.ToString() + " / " + WorldManager.EnemyShips.Count.ToString();

            _lblMetal.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Metal].ToString();
            _lblGas.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Gas].ToString();
            _lblWater.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Water].ToString();
            _lblCrystal.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Crystal].ToString();
            _lblUranium.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Uranium].ToString();

            _menu.Update(gameTime);
            WorldManager.Update(gameTime);
            UnitManager.Update(gameTime);
            ProjectileManager.Update(gameTime);

            return _nextGameState;
        }

        public override void Draw(GameTime gameTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
        {
            graphics.Clear(Color.Black);

            // screen space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            {
                WorldManager.DrawScreen(spriteBatch);
            }
            spriteBatch.End();

            // world space
            spriteBatch.Begin(sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: Camera.GetViewMatrix());
            {
                WorldManager.DrawWorld(spriteBatch);
                UnitManager.DrawWorld(spriteBatch);
                ProjectileManager.Draw(spriteBatch);
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

            if (direction == MouseScrollDirection.Up)
            {
                _currentZoomLevel += 1;

                if (_currentZoomLevel >= _zoomLevels.Count)
                    _currentZoomLevel = _zoomLevels.Count - 1;

                Camera.Zoom = _zoomLevels[_currentZoomLevel];
                Camera.CheckBoundingBox();
            }
            else
            {
                _currentZoomLevel -= 1;

                if (_currentZoomLevel < 0)
                    _currentZoomLevel = 0;

                Camera.Zoom = _zoomLevels[_currentZoomLevel];
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
                ShowDebug = !ShowDebug;
                _menu.GetFrame("debugFrame").Visible = ShowDebug;
                _menu.GetFrame("debugFrame").Active = ShowDebug;
            }
            else if (key == Keys.Space)
            {
                //_lockCamera = !_lockCamera;
                Camera.CenterPosition(WorldManager.PlayerEntity.Position);
            }
            else if (key == Keys.P)
            {
                WorldManager.PlayerEntity.Inventory.AddAll(100);
            }
            else if (key == Keys.B)
            {
                var productionFrame = _menu.GetFrame("frmProduction");

                productionFrame.Visible = !productionFrame.Visible;
                productionFrame.Active = productionFrame.Visible;
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
