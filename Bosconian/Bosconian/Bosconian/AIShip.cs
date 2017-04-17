using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bosconian {

    class AIShip : Ship {

        public AIShip(Vector2 pos) : base(pos) {
            //do nothing
        }

        //returns true if on target
        public bool target(Vector2 pos) {

            int x = drawBox.X;
            int y = drawBox.Y;
            int velocity = 0;
            int speed = 0;

            int dx = (int) Math.Abs(pos.X - x);
            int dy = (int) Math.Abs(pos.Y - y);
            if (dx > 8 || dy > 8) {
                velocity = speed * 2;
            } else if (dx > 4 || dy > 4) {
                velocity = speed;
            } else return true;

            if (pos.X < x && pos.Y < y) direction = 4;
            else if (pos.X > x && pos.Y < y) direction = 5;
            else if (pos.X < x && pos.Y > y) direction = 6;
            else if (pos.X > x && pos.Y > y) direction = 7;
            else if (pos.Y < y) direction = 0;
            else if (pos.Y > y) direction = 1;
            else if (pos.X < x) direction = 2;
            else if (pos.X > x) direction = 3;
            return false;
        }

        public override void Update() {
            Move();
        }
    }
}
