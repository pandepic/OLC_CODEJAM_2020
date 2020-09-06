using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PandaMonogame
{
    public class BasicCamera2D
    {
        public float Rotation { get; set; } = 0f;
        public float Zoom { get; set; } = 1f;
        public float Z { get; set; } = 1f;

        public Rectangle BoundingBox { get; set; } = Rectangle.Empty;
        protected Rectangle _view = Rectangle.Empty;

        protected Vector2 _origin = Vector2.Zero;
        protected Vector2 _position = Vector2.Zero;

        public Vector2 Velocity = Vector2.Zero;

        public BasicCamera2D() { }

        public BasicCamera2D(Rectangle view, Rectangle boundingBox)
        {
            if (view != null)
            {
                this._view = view;
                _origin = new Vector2((float)view.Width / 2f, (float)view.Height / 2f);

                _position.X = _view.X;
                _position.Y = _view.Y;
            }

            this.BoundingBox = boundingBox;
        }

        public void Update(GameTime gameTime)
        {
            if (Velocity != Vector2.Zero)
            {
                var delta = gameTime.DeltaTime();

                _position += Velocity * delta;
            }

            _view.X = (int)_position.X;
            _view.Y = (int)_position.Y;

            CheckBoundingBox();
        }

        public Rectangle GetViewRect()
        {
            return _view;
        }

        public float GetZFromZoom(float zoom, float targetZ)
        {
            return 1 / zoom + targetZ;
        }
        public float GetZoomFromZ(float z, float targetZ)
        {
            if (z - targetZ == 0)
            {
                return 0;
            }
            return 1 / (z - targetZ);
        }

        public Matrix GetViewMatrix(float z = 0f)
        {
            float zoomZ = GetZoomFromZ(Z, z);
            return  Matrix.CreateTranslation(new Vector3(-_position, 0.0f)) *
                    Matrix.CreateTranslation(new Vector3(-_origin, 0.0f)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom, Zoom, 1) *
                    Matrix.CreateScale(zoomZ, zoomZ, 1) *
                    Matrix.CreateTranslation(new Vector3(_origin, 0.0f));
        }

        public Vector2 GetPosition()
        {
            return _position;
        }

        public Vector2 GetOrigin()
        {
            return _origin;
        }

        public Rectangle GetBoundingBox()
        {
            return BoundingBox;
        }

        public void SetPosition(Vector2 position)
        {
            if (position == null)
                return;

            _position = position;

            CheckBoundingBox();
        }

        public void OffsetPosition(int x, int y)
        {
            _position.X += x;
            _position.Y += y;

            CheckBoundingBox();
        }

        public void OffsetPosition(Vector2 vector)
        {
            //OffsetPosition((int)vector.X, (int)vector.Y);
            _position += vector;
            CheckBoundingBox();
        }

        public void CenterPosition(Vector2 position)
        {
            if (position == null)
                return;

            _position.X = position.X - _view.Width / 2;
            _position.Y = position.Y - _view.Height / 2;

            CheckBoundingBox();
        }

        public void CenterEntity(Entity2D entity)
        {
            if (entity == null)
                return;

            _position.X = entity.Position.X + entity.Width / 2 - _view.Width / 2;
            _position.Y = entity.Position.Y + entity.Height / 2 - _view.Height / 2;

            CheckBoundingBox();
        }

        public void CheckBoundingBox()
        {
            if (BoundingBox.IsEmpty || BoundingBox == null)
                return;

            if (_position.X < BoundingBox.Left)
                _position.X = BoundingBox.Left;
            if (_position.Y < BoundingBox.Top)
                _position.Y = BoundingBox.Top;
            if (_position.X > BoundingBox.Right)
                _position.X = BoundingBox.Right;
            if (_position.Y > BoundingBox.Bottom)
                _position.Y = BoundingBox.Bottom;

            _view.X = (int)_position.X;
            _view.Y = (int)_position.Y;

            //if (_view.X < BoundingBox.X)
            //    SetViewPositionX(BoundingBox.X);
            //if (_view.X + _view.Width > BoundingBox.X + BoundingBox.Width)
            //    SetViewPositionX((BoundingBox.X + BoundingBox.Width) - _view.Width);
            //if (_view.Y < BoundingBox.Y)
            //    SetViewPositionY(BoundingBox.Y);
            //if ((_view.Y + _view.Height) > (BoundingBox.Y + BoundingBox.Height))
            //    SetViewPositionY((BoundingBox.Y + BoundingBox.Height) - _view.Height);
        }

        public bool WorldRectInView(Rectangle rect)
        {
            var worldCamRectXY = ScreenToWorldPosition(new Vector2(_view.X, _view.Y));
            var worldCamRectWH = ScreenToWorldPosition(new Vector2(_view.Width, _view.Height));
            var worldCamRect = new Rectangle()
            {
                X = (int)worldCamRectXY.X,
                Y = (int)worldCamRectXY.Y,
                Width = (int)worldCamRectWH.X,
                Height = (int)worldCamRectWH.Y,
            };

            return worldCamRect.Intersects(rect);
        }

        public Rectangle GetCameraWorldRect()
        {
            var worldCamRectXY = GetCameraWorldRectXY();
            var worldCamRectWH = GetCameraWorldRectWH();
            var worldCamRect = new Rectangle()
            {
                X = (int)worldCamRectXY.X,
                Y = (int)worldCamRectXY.Y,
                Width = (int)worldCamRectWH.X,
                Height = (int)worldCamRectWH.Y,
            };

            return worldCamRect;
        }

        public Vector2 GetCameraWorldRectXY()
        {
            return ScreenToWorldPosition(new Vector2(0, 0));
        }

        public Vector2 GetCameraWorldRectWH()
        {
            var xy = GetCameraWorldRectXY();
            var wh = ScreenToWorldPosition(new Vector2(_view.Width, _view.Height));
            return wh - xy;
        }

        public Vector2 ScreenToWorldPosition(Vector2 pos)
        {
            return Vector2.Transform(pos, Matrix.Invert(GetViewMatrix()));
        }
    }
}
