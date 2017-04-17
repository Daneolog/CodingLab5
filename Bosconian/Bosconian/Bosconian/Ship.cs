using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bosconian {

    public abstract class Ship : LinearSpriteSheet {

        public Vector2 position {
            set {
                drawBox.X = (int)value.X;
                drawBox.Y = (int)value.Y;
            }
            get {return new Vector2(drawBox.X, drawBox.Y);}
        }

        protected const int UP = 0;
        protected const int DOWN = 1;
        protected const int LEFT = 2;
        protected const int RIGHT = 3;
        protected const int UP_LEFT = 4;
        protected const int UP_RIGHT = 5;
        protected const int DOWN_LEFT = 6;
        protected const int DOWN_RIGHT = 7;
        protected int direction = UP;

        public Ship(Vector2 pos) : base(Game.textures[(int) Game.TextureNames.SHIP], pos, 8) {
            crop = new Rectangle(0, 0, image.Width/8, image.Height);
        }

        public void setDirection(int dir) {
            direction = dir;
        }

        public int getDirection() {
            return direction;
        }

        public void Move() {
            int velocity = 2;
            switch (direction) {
                case UP: drawBox.Y -= velocity; break;
                case DOWN: drawBox.Y += velocity; break;
                case LEFT: drawBox.X -= velocity; break;
                case RIGHT: drawBox.X += velocity; break;
                case UP_RIGHT:
                    drawBox.Y -= velocity;
                    drawBox.X += velocity;
                    break;
                case UP_LEFT:
                    drawBox.Y -= velocity;
                    drawBox.X -= velocity;
                    break;
                case DOWN_RIGHT:
                    drawBox.Y += velocity;
                    drawBox.X += velocity;
                    break;
                case DOWN_LEFT:
                    drawBox.Y += velocity;
                    drawBox.X -= velocity;
                    break;
            }
        }

        public override void Draw(SpriteBatch batch) {
            crop.X = crop.Width * direction;
            batch.Draw(image, drawBox, crop, Color.White);
        }
    }
}
