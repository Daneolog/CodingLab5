using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bosconian {

    //A camera class from: http://gamedev.stackexchange.com/a/59450
    public class Camera {

        public float Zoom;
        public Vector2 Location;
        public float Rotation;
        private Rectangle Bounds;
        public int Width  {get {return Bounds.Width;}}
        public int Height {get {return Bounds.Height;}}

        public Matrix TransformMatrix {
            get {
                return
                    Matrix.CreateTranslation(new Vector3(-Location.X, -Location.Y, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
            }
        }

        public Camera(Viewport viewport) {
            Bounds = viewport.Bounds;
            Location = new Vector2(0f, 0f);
            Rotation = 0;
            Zoom = 1;
        }
    }
}
