using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Bosconian {

    public class Player : Ship {

        public Vector2 cameraPosition {get {return new Vector2(drawBox.Center.X, drawBox.Center.Y);}}

        public Player(Vector2 pos) : base(pos) {
            //do nothing
        }

        public void setDirectionWithKeys() {
            bool up = Game.keys.IsKeyDown(Keys.Up);
            bool down = Game.keys.IsKeyDown(Keys.Down);
            bool left = Game.keys.IsKeyDown(Keys.Left);
            bool right = Game.keys.IsKeyDown(Keys.Right);
            if (up) {
                if (right) direction = UP_RIGHT;
                else if (left) direction = UP_LEFT;
                else direction = UP;
            } else if (down) {
                if (right) direction = DOWN_RIGHT;
                else if (left) direction = DOWN_LEFT;
                else direction = DOWN;
            } else if (right) {
                direction = RIGHT;
            } else if (left) direction = LEFT;
        }

        public override void Update() {
            setDirectionWithKeys();
            Move();
        }

        public override void Draw(SpriteBatch batch) {
            crop.X = crop.Width * direction;
            batch.Draw(image, drawBox, crop, Color.White);
        }
    }
}
