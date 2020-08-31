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

        public BasicCamera2D Camera;
        public WorldManager WorldManager;
        protected PUIMenu Menu;

        public bool Dragging = false;
        public Vector2 DragStart = Vector2.Zero;
        public Rectangle DragRect = Rectangle.Empty;
        public Rectangle DragRectWorldSpace = Rectangle.Empty;

        public List<Ship> SelectedShips;

        public UnitManager(GraphicsDevice graphics, BasicCamera2D camera, WorldManager worldManager, PUIMenu menu)
        {
            Camera = camera;
            Graphics = graphics;
            WorldManager = worldManager;
            Menu = menu;

            SelectedShips = new List<Ship>();
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
        }

        public void DrawScreen(SpriteBatch spriteBatch)
        {
            if (Dragging)
                spriteBatch.Draw(DragSelectTexture, DragRect, DragRect, Color.White);
        }

        public void DrawWorld(SpriteBatch spriteBatch)
        {
        }

        public void ClearSelectedShips()
        {
            foreach (var ship in SelectedShips)
            {
                ship.Selected = false;
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

            foreach (var ship in WorldManager.Ships)
            {
                if (ship.CollisionRect.Intersects(DragRectWorldSpace))
                {
                    SelectedShips.Add(ship);
                    ship.Selected = true;
                }
            }
        }

        public void SpawnShip(ShipType type, Vector2 position, Ship owner = null)
        {
            Ship newShip = null;

            switch (type)
            {
                case ShipType.Miner:
                    {
                        newShip = new Miner(owner, position);
                    }
                    break;
            }

            if (newShip != null)
                WorldManager.Ships.Add(newShip);

        } // SpawnShip

        public void OnMouseMoved(Vector2 originalPosition, GameTime gameTime)
        {
            var mousePosition = Screen.GetMousePosition();

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

                var worldLeftTop = Camera.ScreenToWorldPosition(new Vector2(left, top));
                var worldRightBottom = Camera.ScreenToWorldPosition(new Vector2(right, bottom));

                DragRectWorldSpace.X = (int)worldLeftTop.X;
                DragRectWorldSpace.Y = (int)worldLeftTop.Y;
                DragRectWorldSpace.Width = (int)(worldRightBottom.X - worldLeftTop.X);
                DragRectWorldSpace.Height = (int)(worldRightBottom.Y - worldLeftTop.Y);

                CheckSelection();
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

                CheckSelection();

                if (SelectedShips.Count == 0)
                    SelectHomeShip();
            }

            if (button == MouseButtonID.Right)
            {
                var mousePosition = Screen.GetMousePosition();
                var mouseWorldPos = Camera.ScreenToWorldPosition(mousePosition);

                if (SelectedShips.Count == 0)
                {
                    WorldManager.PlayerEntity.StateMachine.GetState<PlayerTravelingState>().Target = mouseWorldPos;
                    WorldManager.PlayerEntity.SetState<PlayerTravelingState>();
                }
                else
                {
                    var selectedTypes = new Dictionary<ShipType, List<Ship>>();

                    foreach (var ship in SelectedShips)
                    {
                        if (!selectedTypes.ContainsKey(ship.Type))
                            selectedTypes.Add(ship.Type, new List<Ship>());

                        selectedTypes[ship.Type].Add(ship);
                    }

                    foreach (var kvp in selectedTypes)
                    {
                        switch (kvp.Key)
                        {
                            case ShipType.Miner:
                                {
                                    Asteroid target = null;

                                    for (var i = 0; i <= WorldManager.Asteroids.LastActiveIndex; i++)
                                    {
                                        var asteroid = WorldManager.Asteroids[i];

                                        if (asteroid.CollisionRect.Contains(mouseWorldPos))
                                            target = asteroid;
                                    }

                                    if (target != null && target.ResourceType != ResourceType.None)
                                    {
                                        foreach (var miner in kvp.Value)
                                        {
                                            var state = miner.StateMachine.GetState<MinerTravelingState>();
                                            state.Target = target;
                                            miner.SetState<MinerTravelingState>();
                                        }
                                    }
                                    else
                                    {
                                        if (WorldManager.PlayerEntity.CollisionRect.Contains(mouseWorldPos))
                                        {
                                            foreach (Miner miner in kvp.Value)
                                            {
                                                miner.CurrentMiningTarget = null;
                                                var patrolFollow = miner.GetState<MinerReturningState>();
                                                patrolFollow.Target = miner.Owner;
                                                miner.SetState<MinerReturningState>();
                                            }
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
