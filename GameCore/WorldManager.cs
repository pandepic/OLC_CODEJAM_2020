using GameCore.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PandaMonogame;
using PandaMonogame.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GameCore
{
    public class WorldManager
    {
        public ObjectPool<Asteroid> Asteroids;
        public List<Ship> Ships = new List<Ship>();
        public List<Ship> PlayerShips = new List<Ship>();
        public List<Ship> EnemyShips = new List<Ship>();
        public List<Ship> DeadShips = new List<Ship>();
        public Player PlayerEntity;

        public Texture2D MinimapPlayer, MinimapEnemy;

        //public Texture2D Planet, Background;
        public Texture2D Background;
        public Vector2 ScreenCenter;

        GraphicsDevice Graphics;

        public WorldManager()
        {
        }

        public void Setup(GraphicsDevice graphics)
        {
            Graphics = graphics;
            ScreenCenter = new Vector2(graphics.PresentationParameters.BackBufferWidth / 2, graphics.PresentationParameters.BackBufferHeight / 2);

            MinimapPlayer = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "MinimapPlayer", true);
            MinimapEnemy = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "MinimapEnemy", true);

            // todo - set world seed
            WorldData.RNG = new Random();

            var bg = 1;// WorldData.RNG.Next(1, 9);

            var planet = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Planet" + WorldData.RNG.Next(1, 11).ToString(), true);
            var background = ModManager.Instance.AssetManager.LoadTexture2D(graphics, "Background" + bg.ToString(), true);

            Background = new RenderTarget2D(graphics, background.Width, background.Height);

            graphics.SetRenderTarget((RenderTarget2D)Background);
            graphics.Clear(Color.Transparent);

            using (SpriteBatch spriteBatch = new SpriteBatch(graphics))
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(background, Vector2.Zero, Color.White); // todo : fix bg not lining up at right edge
                spriteBatch.Draw(planet,
                    new Vector2()
                    {
                        X = background.Width / 2 - planet.Width / 2,
                        Y = background.Height / 2 - planet.Height / 2
                    },
                    Color.White);
                spriteBatch.End();
            }
            graphics.SetRenderTarget(null);

            Asteroids = new ObjectPool<Asteroid>(10000);

            // random asteroid field generation
            for (var x = Config.AsteroidRegionWidth; x < (Config.WorldWidth - Config.AsteroidRegionWidth); x += Config.AsteroidRegionWidth)
            {
                for (var y = Config.AsteroidRegionHeight; y < (Config.WorldHeight - Config.AsteroidRegionHeight); y += Config.AsteroidRegionHeight)
                {
                    if (WorldData.RNG.Next(0, 10) < 2 && Asteroids.LastActiveIndex < (Asteroids.Size - 1))
                    {
                        var newAsteroid = Asteroids.New();
                        newAsteroid.Sprite = TexturePacker.GetSprite("AsteroidsAtlas", "Asteroid" + WorldData.RNG.Next(1, WorldData.AsteroidTypes + 1).ToString());
                        newAsteroid.Position = new Vector2(x + WorldData.RNG.Next(0, 150), y + WorldData.RNG.Next(0, 150));
                        newAsteroid.Origin = new Vector2(newAsteroid.Sprite.SourceRect.Width / 2, newAsteroid.Sprite.SourceRect.Height / 2);
                        newAsteroid.RotationSpeed = (float)PandaUtil.RandomDouble(WorldData.RNG, 0, 8);
                        newAsteroid.Rotation = WorldData.RNG.Next(0, 360);

                        if (WorldData.RNG.Next(0, 10) < 3)
                        {
                            newAsteroid.ResourceType = WorldData.ResourceTypes[WorldData.RNG.Next(0, WorldData.ResourceTypes.Count)];
                            newAsteroid.ResourceCount = WorldData.RNG.Next(50000, 100000);
                            newAsteroid.ResourceSprite = TexturePacker.GetSprite("ResourcesAtlas", newAsteroid.ResourceType.ToString());
                        }
                    }
                }
            }

            PlayerEntity = new Player();
            PlayerEntity.Position = Config.PlayerStartPosition;
            PlayerShips.Add(PlayerEntity);
            Ships.Add(PlayerEntity);

            for (var i = 0; i < Config.StartingMiners; i++)
            {
                GameplayState.UnitManager.SpawnShip(ShipType.Miner, PlayerEntity.Position + new Vector2(WorldData.RNG.Next(-200, 200), WorldData.RNG.Next(-200, 200)), PlayerEntity);
            }

            // TEMP TEST SPAWNING
            using (var fs = ModManager.Instance.AssetManager.GetFileStreamByAsset("Config"))
            {
                var configDoc = XDocument.Load(fs);
                var elTestShips = configDoc.Root.Element("TestShips");

                if (elTestShips != null)
                {
                    foreach (var ship in elTestShips.Elements("Ship"))
                    {
                        var isPlayer = bool.Parse(ship.Attribute("Player").Value);
                        var shipType = ship.Attribute("Type").Value.ToEnum<ShipType>();
                        var count = int.Parse(ship.Attribute("Count").Value);

                        for (var i = 0; i < count; i++)
                        {
                            var position = new Vector2(4200, 4200) + new Vector2(WorldData.RNG.Next(-1000, 1000), WorldData.RNG.Next(-1000, 1000));
                            Ship owner = PlayerEntity;

                            if (!isPlayer)
                            {
                                position = new Vector2(1000, 1000) + new Vector2(WorldData.RNG.Next(-1000, 1000), WorldData.RNG.Next(-1000, 1000));
                                owner = null;
                            }

                            GameplayState.UnitManager.SpawnShip(shipType, position, owner);
                        }
                    }
                }
            }
            // END TEMP TEST SPAWNING
        }

        ~WorldManager()
        {
            Background.Dispose();
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                Asteroids[i].Update(gameTime);
            }

            DeadShips.Clear();

            for (var i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                ship.Update(gameTime);

                if (ship.CurrentArmourHP <= 0)
                    DeadShips.Add(ship);
            }
            
            foreach (var ship in DeadShips)
                GameplayState.UnitManager.DestroyShip(ship);
        }

        public void DrawWorld(SpriteBatch spriteBatch)
        {
            var camPos = GameplayState.Camera.GetPosition() + GameplayState.Camera.GetOrigin();
            var viewDistance = Graphics.PresentationParameters.BackBufferWidth / GameplayState.Camera.Zoom;

            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                var asteroid = Asteroids[i];
                var distance = Vector2.Distance(asteroid.Position, camPos);

                // quick hack to do view culling
                if (distance < viewDistance)
                    asteroid.Draw(spriteBatch);
            }

            GameplayState.EffectsManager.Draw(spriteBatch);

            for (var i = 0; i < Ships.Count; i++)
            {
                var ship = Ships[i];
                var distance = Vector2.Distance(ship.Position, camPos);

                // quick hack to do view culling
                if (distance < viewDistance)
                    ship.Draw(spriteBatch);
            }
        }

        public void DrawScreen(SpriteBatch spriteBatch)
        {
            // todo : fix bg not lining up at right edge
            var worldSize = new Vector2(Config.WorldWidth, Config.WorldHeight);
            var bgSize = new Vector2(Background.Width, Background.Height);
            var bgProportionalSize = (float)bgSize.X / (float)worldSize.X;
            float bgZoom = 1.0f - ((1.0f - GameplayState.Camera.Zoom) * bgProportionalSize);

            var screenPosWorld = GameplayState.Camera.ScreenToWorldPosition(Vector2.Zero);

            var backgroundPos = ((screenPosWorld / worldSize) * bgSize) * bgZoom;

            spriteBatch.Draw(
                        Background,
                        -backgroundPos,
                        null,
                        Color.White,
                        0.0f,
                        Vector2.Zero,
                        bgZoom,
                        SpriteEffects.None,
                        0.0f
                        );

            var minimapSize = new Vector2(GameplayState.MinimapFrame.Width, GameplayState.MinimapFrame.Height);
            var minimapRatio = minimapSize / worldSize;
            var minimapOrigin = new Vector2(MinimapPlayer.Width / 2, MinimapPlayer.Height / 2) * 0.5f;

            foreach (var ship in PlayerShips)
            {
                if (ship.TargetType == TargetType.Small)
                    continue;

                var shipMinimapPos = ship.Position * minimapRatio;

                spriteBatch.Draw(
                        MinimapPlayer,
                        GameplayState.MinimapFrame.Position + shipMinimapPos,
                        null,
                        Color.White,
                        0.0f,
                        minimapOrigin,
                        0.5f,
                        SpriteEffects.None,
                        0.0f
                        );
            }

            foreach (var ship in EnemyShips)
            {
                if (ship.TargetType == TargetType.Small)
                    continue;

                var shipMinimapPos = ship.Position * minimapRatio;

                spriteBatch.Draw(
                        MinimapEnemy,
                        GameplayState.MinimapFrame.Position + shipMinimapPos,
                        null,
                        Color.White,
                        0.0f,
                        minimapOrigin,
                        0.5f,
                        SpriteEffects.None,
                        0.0f
                        );
            }

            //var worldCamRectXY = (GameplayState.Camera.GetCameraWorldRectXY() * (0.5f * GameplayState.Camera.Zoom)) * minimapRatio;
            //var worldCamRectWH = (GameplayState.Camera.GetCameraWorldRectWH() / (0.5f * GameplayState.Camera.Zoom)) * minimapRatio;

            //var worldCamRect = new Rectangle()
            //{
            //    X = (int)worldCamRectXY.X,
            //    Y = (int)worldCamRectXY.Y,
            //    Width = (int)worldCamRectWH.X,
            //    Height = (int)worldCamRectWH.Y,
            //};

            var worldCamRect = GameplayState.Camera.GetCameraWorldRect();

            worldCamRect.X = (int)(worldCamRect.X * minimapRatio.X);
            worldCamRect.Y = (int)(worldCamRect.Y * minimapRatio.Y);
            worldCamRect.Width = (int)(worldCamRect.Width * minimapRatio.X);
            worldCamRect.Height = (int)(worldCamRect.Height * minimapRatio.Y);

            var worldCamDestRect = worldCamRect;
            worldCamDestRect.X += (int)GameplayState.MinimapFrame.Position.X;
            worldCamDestRect.Y += (int)GameplayState.MinimapFrame.Position.Y;

            spriteBatch.Draw(GameplayState.UnitManager.DragSelectTexture, worldCamDestRect, worldCamRect, Color.White);
        }

        public Asteroid GetAsteroidAtWorldPosition(Vector2 position)
        {
            for (var i = 0; i <= Asteroids.LastActiveIndex; i++)
            {
                var asteroid = GameplayState.WorldManager.Asteroids[i];

                if (asteroid.CollisionRect.Contains(position))
                    return asteroid;
            }

            return null;
        }

        public Ship GetShipAtWorldPosition(Vector2 position, bool playerShip = true, List<ShipType> filter = null)
        {
            var targets = Ships;
            //if (!playerShip)
            //    targets = EnemyShips;

            for (var i = 0; i < targets.Count; i++)
            {
                var ship = targets[i];

                if (ship.CollisionRect.Contains(position) && (filter == null || filter.Contains(ship.ShipType)))
                    return ship;
            }

            return null;
        }
    }
}
