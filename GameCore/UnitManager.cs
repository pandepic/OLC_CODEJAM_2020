using GameCore.AI;
using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PandaMonogame;
using PandaMonogame.UI;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameCore
{
    public class UnitManager
    {
        public Texture2D DragSelectTexture;

        public GraphicsDevice Graphics;

        protected PUIMenu Menu;

        public bool Dragging = false;
        public Vector2 DragStart = Vector2.Zero;
        public Rectangle DragRect = Rectangle.Empty;
        public Rectangle DragRectWorldSpace = Rectangle.Empty;

        public List<Ship> SelectedShips;
        public Dictionary<ShipType, List<Ship>> SelectedShipTypes;
        public StringBuilder SBSelection = new StringBuilder();

        public UnitManager()
        {
        }

        public void Setup(GraphicsDevice graphics, PUIMenu menu)
        {
            Graphics = graphics;
            Menu = menu;

            SelectedShips = new List<Ship>();
            SelectedShipTypes = new Dictionary<ShipType, List<Ship>>();
            SelectHomeShip();

            DragSelectTexture = new RenderTarget2D(graphics, graphics.PresentationParameters.BackBufferWidth, graphics.PresentationParameters.BackBufferHeight);

            graphics.SetRenderTarget((RenderTarget2D)DragSelectTexture);
            graphics.Clear(new Color(120, 120, 120, 120));
            graphics.SetRenderTarget(null);
        }

        ~UnitManager()
        {
            DragSelectTexture.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            if (Dragging)
                CheckSelection();
        }

        public void DrawScreen(SpriteBatch spriteBatch)
        {
            SBSelection.Clear();
            foreach (var kvp in SelectedShipTypes)
            {
                SBSelection.Append("[" + kvp.Key.ToString() + " x" + kvp.Value.Count.ToString() + "] ");
            }

            Menu.GetWidget<PUIWLabel>("lblSelection").Text = SBSelection.ToString();

            if (Dragging)
                spriteBatch.Draw(DragSelectTexture, DragRect, DragRect, Color.White);

            var camPos = GameplayState.Camera.GetPosition() + GameplayState.Camera.GetOrigin();
            var viewDistance = Graphics.PresentationParameters.BackBufferWidth / GameplayState.Camera.Zoom;

            Sprites.DefaultFont.Size = 18;

            for (var i = 0; i <= GameplayState.WorldManager.Asteroids.LastActiveIndex; i++)
            {
                var asteroid = GameplayState.WorldManager.Asteroids.Buffer[i];
                var distance = Vector2.Distance(asteroid.Position, camPos);

                if (distance > viewDistance)
                    continue;

                if (asteroid.ResourceType == ResourceType.None)
                    continue;
                
                var iconPosition = asteroid.Position - new Vector2(asteroid.ResourceSprite.SourceRect.Width / 2, asteroid.ResourceSprite.SourceRect.Height / 2) - new Vector2(0, asteroid.Origin.Y + 50);
                iconPosition = Vector2.Transform(iconPosition, GameplayState.Camera.GetViewMatrix());

                spriteBatch.Draw(asteroid.ResourceSprite.Texture, iconPosition, asteroid.ResourceSprite.SourceRect, Color.White);
                //spriteBatch.DrawString(Sprites.DefaultFont, asteroid.ResourceSprite.Texture, iconPosition, Color.White);
            }

            for (var i = 0; i < GameplayState.WorldManager.Ships.Count; i++)
            {
                var ship = GameplayState.WorldManager.Ships[i];

                if (ship.TargetType != TargetType.Large)
                    continue;

                var distance = Vector2.Distance(ship.Position, camPos);

                if (distance > viewDistance)
                    continue;

                var shipString = "";

                var armourPercent = ship.CurrentArmourHP / ship.BaseArmourHP * 100.0f;
                var shieldPercent = ship.CurrentShieldHP / ship.BaseShieldHP * 100.0f;

                if (armourPercent < 100.0f)
                    shipString += "A: " + armourPercent.ToString("0") + "%";
                if (shieldPercent < 100.0f)
                    shipString += (shipString.Length > 0 ? " " : "") + "S: " + shieldPercent.ToString("0") + "%";

                if (shipString.Length > 0)
                {
                    var shipStringSize = Sprites.DefaultFont.MeasureString(shipString);
                    var textPosition = (ship.Position - new Vector2(shipStringSize.X / 2, shipStringSize.Y / 2) - new Vector2(0, ship.Origin.Y + 50));
                    textPosition = Vector2.Transform(textPosition, GameplayState.Camera.GetViewMatrix());

                    spriteBatch.DrawString(Sprites.DefaultFont, shipString, textPosition, Color.White);
                }
            }
        }

        public void DrawWorld(SpriteBatch spriteBatch)
        {
        }

        public void ClearSelectedShips()
        {
            foreach (var ship in SelectedShips)
            {
                ship.IsSelected = false;
            }

            SelectedShips.Clear();
        }

        public void SelectHomeShip()
        {
            //SelectedShips.Add(WorldManager.PlayerEntity);
            //WorldManager.PlayerEntity.Selected = true;
        }

        public void CheckSelection()
        {
            ClearSelectedShips();

            foreach (var ship in GameplayState.WorldManager.Ships)
            {
                if (!ship.IsPlayerShip || ship.ShipType == ShipType.HomeShip || !ship.IsSelectable)
                    continue;

                if (ship.CollisionRect.Intersects(DragRectWorldSpace))
                {
                    SelectedShips.Add(ship);
                    ship.IsSelected = true;
                }
            }

            UpdateSelectedShipTypes();
        }

        public void UpdateSelectedShipTypes()
        {
            SelectedShipTypes.Clear();

            foreach (var ship in SelectedShips)
            {
                if (!SelectedShipTypes.ContainsKey(ship.ShipType))
                    SelectedShipTypes.Add(ship.ShipType, new List<Ship>());

                SelectedShipTypes[ship.ShipType].Add(ship);
            }
        }

        public Ship SpawnShip(ShipType type, Vector2 position, Ship owner = null)
        {
            Ship newShip = null;

            switch (type)
            {
                case ShipType.Miner:
                    {
                        newShip = new Miner(owner, position);
                    }
                    break;

                case ShipType.Fighter:
                    {
                        newShip = new Fighter(owner, position);
                    }
                    break;

                case ShipType.Bomber:
                    {
                        newShip = new Bomber(owner, position);
                    }
                    break;

                case ShipType.RepairShip:
                    {
                        newShip = new RepairShip(owner, position);
                    }
                    break;

                case ShipType.MissileFrigate:
                    {
                        newShip = new MissileFrigate(owner, position);
                    }
                    break;

                case ShipType.BeamFrigate:
                    {
                        newShip = new BeamFrigate(owner, position);
                    }
                    break;

                case ShipType.SupportCruiser:
                    {
                        newShip = new SupportCruiser(owner, position);
                    }
                    break;

                case ShipType.HeavyCruiser:
                    {
                        newShip = new HeavyCruiser(owner, position);
                    }
                    break;

                case ShipType.Battleship:
                    {
                        newShip = new Battleship(owner, position);
                    }
                    break;

                case ShipType.Carrier:
                    {
                        newShip = new Carrier(owner, position);
                    }
                    break;
            }

            if (newShip == null)
                return newShip;

            if (newShip.IsPlayerShip)
                GameplayState.WorldManager.PlayerShips.Add(newShip);
            else
                GameplayState.WorldManager.EnemyShips.Add(newShip);

            GameplayState.WorldManager.Ships.Add(newShip);

            return newShip;
        } // SpawnShip

        public void DestroyShip(Ship ship)
        {
            ship.IsDead = true;

            GameplayState.WorldManager.PlayerShips.Remove(ship);
            GameplayState.WorldManager.EnemyShips.Remove(ship);
            GameplayState.WorldManager.Ships.Remove(ship);

            if (ship.Owner != null && ship.Owner is Carrier ownerCarrier)
            {
                if (ship is Fighter fighter)
                    ownerCarrier.Fighters.Remove(fighter);
                else if (ship is Bomber bomber)
                    ownerCarrier.Bombers.Remove(bomber);
            }

            if (ship is Carrier carrier)
                carrier.ReparentChildren();

            GameplayState.EffectsManager.AddExplosion(ship, null, 15.0f);

            if (ship.IsSelected)
            {
                SelectedShips.Remove(ship);
                UpdateSelectedShipTypes();
            }

            if (ship.ShipType == ShipType.HomeShip)
            {
                // todo : GAME OVER
            }
        }

        public void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            var mousePosition = MouseManager.GetMousePosition();

            if (Dragging)
            {
                var left = DragStart.X;
                var top = DragStart.Y;
                var right = mousePosition.X;
                var bottom = mousePosition.Y;

                if (right < left)
                {
                    right = DragStart.X;
                    left = mousePosition.X;
                }

                if (bottom < top)
                {
                    bottom = DragStart.Y;
                    top = mousePosition.Y;
                }

                DragRect.X = (int)left;
                DragRect.Y = (int)top;
                DragRect.Width = (int)(right - left);
                DragRect.Height = (int)(bottom - top);

                var worldLeftTop = GameplayState.Camera.ScreenToWorldPosition(new Vector2(left, top));
                var worldRightBottom = GameplayState.Camera.ScreenToWorldPosition(new Vector2(right, bottom));

                DragRectWorldSpace.X = (int)worldLeftTop.X;
                DragRectWorldSpace.Y = (int)worldLeftTop.Y;
                DragRectWorldSpace.Width = (int)(worldRightBottom.X - worldLeftTop.X);
                DragRectWorldSpace.Height = (int)(worldRightBottom.Y - worldLeftTop.Y);
            }
        }

        public void OnMouseDown(MouseButtonID button, GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.Position.X, mouseState.Position.Y);

            if (button == MouseButtonID.Left)
            {
                if (!Dragging)
                {
                    Dragging = true;
                    DragStart = mousePosition;
                    DragRect = Rectangle.Empty;
                    DragRectWorldSpace = Rectangle.Empty;
                    ClearSelectedShips();
                }
            }
        }

        public void OnMouseClicked(MouseButtonID button, GameTime gameTime)
        {
            if (button == MouseButtonID.Left && Dragging)
            {
                Dragging = false;

                if (DragRectWorldSpace.Width == 0 && DragRectWorldSpace.Height == 0)
                {
                    var mousePosition = MouseManager.GetMousePosition();
                    var mouseWorldPos = GameplayState.Camera.ScreenToWorldPosition(mousePosition);

                    DragRectWorldSpace.X = (int)mouseWorldPos.X;
                    DragRectWorldSpace.Y = (int)mouseWorldPos.Y;
                    DragRectWorldSpace.Width = 1;
                    DragRectWorldSpace.Height = 1;

                    CheckSelection();
                }

                if (SelectedShips.Count == 0)
                    SelectHomeShip();
            }

            if (button == MouseButtonID.Right)
            {
                var mousePosition = MouseManager.GetMousePosition();
                var mouseWorldPos = GameplayState.Camera.ScreenToWorldPosition(mousePosition);

                if (SelectedShips.Count == 0)
                {
                    GameplayState.WorldManager.PlayerEntity.StateMachine.GetState<PlayerTravelingState>().Target = mouseWorldPos;
                    GameplayState.WorldManager.PlayerEntity.SetState<PlayerTravelingState>();
                }
                else
                {
                    foreach (var kvp in SelectedShipTypes)
                    {
                        switch (kvp.Key)
                        {
                            case ShipType.Miner:
                                {
                                    Asteroid target = GameplayState.WorldManager.GetAsteroidAtWorldPosition(mouseWorldPos);

                                    if (target != null && target.ResourceType != ResourceType.None)
                                    {
                                        foreach (var miner in kvp.Value)
                                        {
                                            var traveling = miner.StateMachine.GetState<MinerTravelingState>();
                                            traveling.Target = target;
                                            miner.SetState(traveling);
                                        }
                                    }
                                    else
                                    {
                                        var homeShip = GameplayState.WorldManager.GetShipAtWorldPosition(mouseWorldPos, true, new List<ShipType>() { ShipType.HomeShip });

                                        if (homeShip != null)
                                        {
                                            foreach (Miner miner in kvp.Value)
                                            {
                                                miner.CurrentMiningTarget = null;
                                                var returning = miner.GetState<MinerReturningState>();
                                                returning.Target = miner.Owner;
                                                miner.SetState(returning);
                                            }
                                        }
                                    }
                                }
                                break;

                            case ShipType.Fighter:
                            case ShipType.Bomber:
                                {
                                    var newDefendTarget = GameplayState.WorldManager.GetShipAtWorldPosition(mouseWorldPos);

                                    if (newDefendTarget != null)
                                    {
                                        foreach (Ship ship in kvp.Value)
                                        {
                                            AIHelper.SmallDefendTarget(ship, newDefendTarget);
                                        }
                                    }
                                    else
                                    {
                                        foreach (Ship ship in kvp.Value)
                                        {
                                            AIHelper.SmallDefendPosition(ship, mouseWorldPos);
                                        }
                                    }
                                }
                                break;

                            case ShipType.Battleship:
                            case ShipType.Carrier:
                            case ShipType.SupportCruiser:
                            case ShipType.HeavyCruiser:
                            case ShipType.MissileFrigate:
                            case ShipType.BeamFrigate:
                            case ShipType.RepairShip:
                                {
                                    var newDefendTarget = GameplayState.WorldManager.GetShipAtWorldPosition(mouseWorldPos);

                                    if (newDefendTarget != null)
                                    {
                                        foreach (Ship ship in kvp.Value)
                                        {
                                            AIHelper.BigDefendTarget(ship, newDefendTarget);
                                        }
                                    }
                                    else
                                    {
                                        foreach (Ship ship in kvp.Value)
                                        {
                                            AIHelper.BigDefendPosition(ship, mouseWorldPos);
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }
            } // right mouse button
        } // OnMouseClicked

        public void OnMouseScroll(MouseScrollDirection direction, int scrollValue, GameTime gameTime)
        {
        }

        public void OnKeyPressed(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
        }

        public void OnKeyReleased(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
        }

        public void OnKeyDown(Keys key, GameTime gameTime, CurrentKeyState currentKeyState)
        {
        }

        public void OnTextInput(TextInputEventArgs e, GameTime gameTime, CurrentKeyState currentKeyState)
        {
        }
    }
}
