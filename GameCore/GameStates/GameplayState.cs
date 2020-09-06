using GameCore.AI;
using GameCore.Combat;
using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaMonogame;
using PandaMonogame.Assets;
using PandaMonogame.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace GameCore
{
    public class GameplayState : GameState
    {
        protected int _nextGameState = (int)GameStateType.None;

        public static PUIMenu Menu;
        public static PUIFrame MinimapFrame;

        public static BasicCamera2D Camera;
        public static WorldManager WorldManager;
        public static UnitManager UnitManager;
        public static ProjectileManager ProjectileManager;
        public static EffectsManager EffectsManager;
        public static EnemyWaveManager EnemyWaveManager;
        public static UpgradeManager UpgradeManager;

        public static GraphicsDevice Graphics;

        protected bool _mouseDragging = false;
        protected Vector2 _mouseDragPosition = Vector2.Zero;

        protected bool _lockCamera = false;
        public static bool ShowDebug = false;
        public static bool GameOver = false;
        public static bool GameWon = false;

        protected float ScrollSpeed = 1500.0f;

        protected List<float> _zoomLevels = new List<float>()
        {
            0.2f,
            0.5f,
            1.0f,
        };
        protected int _currentZoomLevel = 0;

        protected StringBuilder _sbGeneral = new StringBuilder();

        protected PUIWLabel _lblProductionQueue;
        protected PUIWLabel _lblMetal, _lblGas, _lblWater, _lblCrystal, _lblUranium;
        protected PUIWLabel _lblUpgradesTitle;
        protected PUIWBasicButton _btnIdleMiners, _btnUpgrades;

        protected Dictionary<ResourceType, int> MinerResourceCounts;

        #region python bound methods
        protected void BuildMiner(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Miner);
        }

        protected void BuildFighter(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Fighter);
        }

        protected void BuildBomber(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Bomber);
        }

        protected void BuildRepairShip(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.RepairShip);
        }

        protected void BuildMissileFrigate(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.MissileFrigate);
        }

        protected void BuildBeamFrigate(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.BeamFrigate);
        }

        protected void BuildSupportCruiser(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.SupportCruiser);
        }

        protected void BuildHeavyCruiser(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.HeavyCruiser);
        }

        protected void BuildBattleship(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Battleship);
        }

        protected void BuildCarrier(params object[] args)
        {
            WorldManager.PlayerEntity.BuildShip(ShipType.Carrier);
        }

        protected void IdleMiners(params object[] args)
        {
            UnitManager.SelectIdleMiners = true;
        }

        protected void Upgrades(params object[] args)
        {
            var upgradesFrame = Menu.GetFrame("frmUpgrades");

            upgradesFrame.Visible = !upgradesFrame.Visible;
            upgradesFrame.Active = upgradesFrame.Visible;
        }

        protected void Help(params object[] args)
        {
            var helpFrame = Menu.GetFrame("frmHelp");

            helpFrame.Visible = !helpFrame.Visible;
            helpFrame.Active = helpFrame.Visible;
        }

        protected void ExitToMenu(params object[] args)
        {
            _nextGameState = (int)GameStateType.Menu;
        }
        #endregion

        public override void Load(ContentManager Content, GraphicsDevice graphics)
        {
            GameWon = false;
            GameOver = false;

            Graphics = graphics;

            UpgradeManager = new UpgradeManager();
            Menu = new PUIMenu();

            Menu.AddMethod(BuildMiner);
            Menu.AddMethod(BuildFighter);
            Menu.AddMethod(BuildBomber);
            Menu.AddMethod(BuildRepairShip);
            Menu.AddMethod(BuildMissileFrigate);
            Menu.AddMethod(BuildBeamFrigate);
            Menu.AddMethod(BuildSupportCruiser);
            Menu.AddMethod(BuildHeavyCruiser);
            Menu.AddMethod(BuildBattleship);
            Menu.AddMethod(BuildCarrier);
            Menu.AddMethod(IdleMiners);
            Menu.AddMethod(Upgrades);
            Menu.AddMethod(Help);
            Menu.AddMethod(ExitToMenu);

            Menu.AddMethod(UpgradeManager.UpgradeMinerCap1);
            Menu.AddMethod(UpgradeManager.UpgradeMinerCap2);
            Menu.AddMethod(UpgradeManager.UpgradeMiningRate1);
            Menu.AddMethod(UpgradeManager.UpgradeMiningRate2);
            Menu.AddMethod(UpgradeManager.UpgradeRepairRate);
            Menu.AddMethod(UpgradeManager.UpgradeShieldRegen1);
            Menu.AddMethod(UpgradeManager.UpgradeShieldRegen2);
            Menu.AddMethod(UpgradeManager.UpgradeWarmachine1);
            Menu.AddMethod(UpgradeManager.UpgradeWarmachine2);
            Menu.AddMethod(UpgradeManager.UpgradeHyperdrive);

            Menu.Load(graphics, "GameplayMenuDefinition", "UITemplates");

            MinimapFrame = Menu.GetFrame("frmMinimap");

            GameOver = false;

            Config.Load();
            EntityData.Load();
            WorldData.Load();

            _lblProductionQueue = Menu.GetWidget<PUIWLabel>("lblProductionQueue");
            _lblMetal = Menu.GetWidget<PUIWLabel>("lblMetal");
            _lblGas = Menu.GetWidget<PUIWLabel>("lblGas");
            _lblWater = Menu.GetWidget<PUIWLabel>("lblWater");
            _lblCrystal = Menu.GetWidget<PUIWLabel>("lblCrystal");
            _lblUranium = Menu.GetWidget<PUIWLabel>("lblUranium");
            _lblUpgradesTitle = Menu.GetWidget<PUIWLabel>("lblUpgradesTitle");

            _btnIdleMiners = Menu.GetWidget<PUIWBasicButton>("btnIdleMiners");
            _btnUpgrades = Menu.GetWidget<PUIWBasicButton>("btnUpgrades");

            // create build menu tooltips
            foreach (var kvp in EntityData.ShipTypes)
            {
                if (!kvp.Value.CanBuild)
                    continue;

                var button = Menu.GetWidget<PUIWBasicButton>("btnBuild" + kvp.Value.ShipType.ToString());

                _sbGeneral.Clear();

                _sbGeneral.Append("\n\n").Append(kvp.Value.Description.Replace("\\n", "\n"));

                foreach (var kvpBuild in kvp.Value.BuildCost)
                {
                    var sprite = TexturePacker.GetSprite("ResourcesAtlas", kvpBuild.Key.ToString());

                    _sbGeneral
                        .Append("[")
                        .Append("ResourcesTexture-")
                        .Append(sprite.SourceRect.X).Append(",")
                        .Append(sprite.SourceRect.Y).Append(",")
                        .Append(sprite.SourceRect.Width).Append(",")
                        .Append(sprite.SourceRect.Height)
                        .Append("] ")
                        .Append(kvpBuild.Value)
                        .Append("  ");
                }

                _sbGeneral.Remove(_sbGeneral.Length - 2, 2);

                button.SetTooltip(_sbGeneral.ToString(), _sbGeneral.ToString(), true);
            }

            _sbGeneral.Clear();

            Camera = new BasicCamera2D(
                new Rectangle(0, 0, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight),
                new Rectangle(0, 0, Config.WorldWidth, Config.WorldHeight));

            _currentZoomLevel = _zoomLevels.IndexOf(0.5f);
            Camera.Zoom = _zoomLevels[_currentZoomLevel];

            EffectsManager = new EffectsManager();
            UnitManager = new UnitManager();
            WorldManager = new WorldManager();
            ProjectileManager = new ProjectileManager();
            EnemyWaveManager = new EnemyWaveManager();

            UpgradeManager.Load();

            UnitManager.Setup(graphics, Menu);
            WorldManager.Setup(graphics);
            EnemyWaveManager.Start();

            Camera.CenterPosition(WorldManager.PlayerEntity.Position);

            MinerResourceCounts = new Dictionary<ResourceType, int>();

            foreach (var type in WorldData.ResourceTypes)
                MinerResourceCounts.Add(type, 0);

        } // Load

        public override int Update(GameTime gameTime)
        {
            if (GameOver || GameWon)
            {
                return (int)GameStateType.GameOver;
            }

            var mousePosition = MouseManager.GetMousePosition();
            var mouseWorldPos = Camera.ScreenToWorldPosition(mousePosition);
            var centerWorldPos = Camera.ScreenToWorldPosition(WorldManager.ScreenCenter);

            if (_lockCamera)
                Camera.CenterPosition(WorldManager.PlayerEntity.Position);

            Menu.GetWidget<PUIWLabel>("lblDebug").Text =
                Camera.GetViewRect().ToString() + " : " + Camera.Zoom + "\n" +
                "Asteroids: " + (WorldManager.Asteroids.LastActiveIndex + 1) + "\n" +
                mouseWorldPos.ToString() + "\n" +
                centerWorldPos.ToString() + "\n" +
                WorldManager.PlayerShips.Count.ToString() + " / " + WorldManager.EnemyShips.Count.ToString();

            _sbGeneral.Clear();
            _sbGeneral.Append("Queue: ");
            var queueListedCount = 0;

            for (var i = 0; i < WorldManager.PlayerEntity.BuildQueue.Count && queueListedCount < 3; i++)
            {
                var nextShip = WorldManager.PlayerEntity.BuildQueue[i];
                _sbGeneral.Append(nextShip.ShipType.ToString() + "(" + (nextShip.Duration / 1000.0f).ToString("0.0") + "s), ");
                queueListedCount += 1;
            }

            if (WorldManager.PlayerEntity.BuildQueue.Count > 0)
                _sbGeneral.Remove(_sbGeneral.Length - 2, 2);

            if (queueListedCount < WorldManager.PlayerEntity.BuildQueue.Count)
                _sbGeneral.Append(" +" + (WorldManager.PlayerEntity.BuildQueue.Count - queueListedCount).ToString() +  "");

            _lblProductionQueue.Text = _sbGeneral.ToString();

            Menu.Update(gameTime);
            WorldManager.Update(gameTime);
            UnitManager.Update(gameTime);
            ProjectileManager.Update(gameTime);
            EffectsManager.Update(gameTime);
            EnemyWaveManager.Update(gameTime);

            Menu.GetWidget<PUIWLabel>("lblNextEnemyWaveTimer").Text = "Next Enemy Wave: " + (EnemyWaveManager.NextWaveTimer / 1000.0f).ToString("0");
            Menu.GetWidget<PUIWLabel>("lblNextEnemyWavePosition").Text = "Next Wave Spawn: " + EnemyWaveManager.NextWavePosition.Name;
            Menu.GetWidget<PUIWLabel>("lblEnemyCount").Text = "Enemies Alive: " + WorldManager.EnemyShips.Count.ToString();
            Menu.GetWidget<PUIWLabel>("lblMinerCount").Text = "Miners: " + WorldManager.Miners.Count.ToString() + " / " + (Config.BaseMinerLimit + UpgradeManager.BonusMaxMiners).ToString();

            var idleMiners = 0;

            foreach (var type in WorldData.ResourceTypes)
                MinerResourceCounts[type] = 0;

            foreach (var m in WorldManager.Miners)
            {
                var miner = (Miner)m;

                if (miner.CurrentMiningTarget == null)
                    idleMiners += 1;
                else
                    MinerResourceCounts[miner.CurrentMiningTarget.ResourceType] += 1;
            }

            _btnIdleMiners.ButtonText = "Idle Miners (" + idleMiners.ToString() + ")";

            if (idleMiners == 0)
            {
                _btnIdleMiners.Visible = false;
                _btnIdleMiners.Active = false;
            }
            else
            {
                _btnIdleMiners.Visible = true;
                _btnIdleMiners.Active = true;
            }

            _lblMetal.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Metal].ToString() + " (" + MinerResourceCounts[ResourceType.Metal].ToString() + ")";
            _lblGas.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Gas].ToString() + " (" + MinerResourceCounts[ResourceType.Gas].ToString() + ")";
            _lblWater.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Water].ToString() + " (" + MinerResourceCounts[ResourceType.Water].ToString() + ")";
            _lblCrystal.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Crystal].ToString() + " (" + MinerResourceCounts[ResourceType.Crystal].ToString() + ")";
            _lblUranium.Text = WorldManager.PlayerEntity.Inventory.Resources[ResourceType.Uranium].ToString() + " (" + MinerResourceCounts[ResourceType.Uranium].ToString() + ")";

            _btnUpgrades.ButtonText = "Upgrades (" + UpgradeManager.UpgradePoints.ToString() + ")";
            _lblUpgradesTitle.Text = "Upgrades (" + UpgradeManager.UpgradePoints.ToString() + " points)";

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
                UnitManager.DrawScreen(spriteBatch);
                Menu.Draw(spriteBatch);
                PUITooltipManager.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        public override void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            var mousePosition = MouseManager.GetMousePosition();

            Menu.OnMouseMoved(originalPosition, gameTime);

            if (!Menu.Focused)
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
            var mousePosition = MouseManager.GetMousePosition();
            
            Menu.OnMouseDown(button, gameTime);

            if (!Menu.Focused)
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
            var mousePosition = MouseManager.GetMousePosition();

            Menu.OnMouseClicked(button, gameTime);

            if (!Menu.Focused)
            {
                if (_mouseDragging && button == MouseButtonID.Middle)
                    _mouseDragging = false;

                UnitManager.OnMouseClicked(button, gameTime);
            }
        }

        public override void OnMouseScroll(MouseScrollDirection direction, int scrollValue, GameTime gameTime)
        {
            if (Menu.Focused)
            {
                Menu.OnMouseScroll(direction, scrollValue, gameTime);
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
            Menu.OnKeyPressed(key, gameTime, currentKeyState);
        }

        public override void OnKeyReleased(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            if (Menu.Focused)
            {
                Menu.OnKeyReleased(key, gameTime, currentKeyState);
                return;
            }

            if (key == Keys.OemTilde)
            {
#if DEBUG
                ShowDebug = !ShowDebug;
                Menu.GetFrame("debugFrame").Visible = ShowDebug;
                Menu.GetFrame("debugFrame").Active = ShowDebug;
#endif
            }
            else if (key == Keys.Space)
            {
                //_lockCamera = !_lockCamera;
                Camera.CenterPosition(WorldManager.PlayerEntity.Position);
            }
            else if (key == Keys.P)
            {
#if DEBUG
                WorldManager.PlayerEntity.Inventory.AddAll(1000);
#endif
            }
            else if (key == Keys.B)
            {
                var productionFrame = Menu.GetFrame("frmProduction");

                productionFrame.Visible = !productionFrame.Visible;
                productionFrame.Active = productionFrame.Visible;
            }
            else if (key == Keys.Tab)
            {
                var helpFrame = Menu.GetFrame("frmHelp");

                helpFrame.Visible = !helpFrame.Visible;
                helpFrame.Active = helpFrame.Visible;
            }
            else if (key == Keys.U)
            {
                var upgradesFrame = Menu.GetFrame("frmUpgrades");

                upgradesFrame.Visible = !upgradesFrame.Visible;
                upgradesFrame.Active = upgradesFrame.Visible;
            }

        } // OnKeyReleased

        public override void OnKeyDown(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
            if (Menu.Focused)
            {
                Menu.OnKeyDown(key, gameTime, currentKeyState);
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
            Menu.OnTextInput(e, gameTime, currentKeyState);
        }
    }
}
