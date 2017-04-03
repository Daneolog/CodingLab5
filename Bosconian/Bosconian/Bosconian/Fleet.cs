using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bosconian {

    public class Fleet {

        public Vector2 cameraPosition {get {return ((Player) ships[0]).cameraPosition;}}
        private Ship[] ships = new Ship[5];
        private int fn = 0; //formation number
        private Vector2[] formation; //coordinates relative to the main ship
        
        public Fleet(Vector2 pos) {
            ships[0] = new Player(pos);
            formation = getFormation(fn);
            for (int i = 1; i < 5; i++) {
                pos.X = ships[0].position.X + formation[i-1].X;
                pos.Y = ships[0].position.Y + formation[i-1].Y;
                ships[i] = new AIShip(pos);
            }
        }

        public Vector2[] getFormation(int i) {
            int x = 16;
            Vector2[] f1 = new Vector2[] {
                new Vector2(-x, x),
                new Vector2(x, x),
                new Vector2(-2*x, 2*x),
                new Vector2(x, x)
            };
            Vector2[] f2 = new Vector2[] {
                new Vector2(-8, x),
                new Vector2(8, x),
                new Vector2(-8, 2*x),
                new Vector2(8, 2*x)
            };
            Vector2[] f3 = new Vector2[] {
                new Vector2(0, 8+x),
                new Vector2(0, 8*2 + x*2),
                new Vector2(0, 8*3 + x*3),
                new Vector2(0, 8*4 + x*4)
            };
            Vector2[] f4 = new Vector2[] {
                new Vector2(-x, x),
                new Vector2(x, x),
                new Vector2(-2*x, x),
                new Vector2(2*x, x)
            };
            Vector2[] f5 = new Vector2[] {
                new Vector2(-2*x, 2*x),
                new Vector2(-x, x),
                new Vector2(-3*x, 3*x),
                new Vector2(x, x)
            };
            Vector2[][] formations = new Vector2[][] {f1, f2, f3, f4, f5};
            return formations[i];
        }

        public Vector2[] getFormationGoal() {
            Vector2[] positions = new Vector2[4];
            for (int i = 0; i < 4; i++) {
                int fx = (int) formation[i].X;
                int fy = (int) formation[i].Y;
                double thet = 0;
                switch (ships[0].getDirection()) {
                    case 1: thet = 180; break;
                    case 2: thet = 270; break;
                    case 3: thet = 90; break;
                    case 4: thet = 315; break;
                    case 5: thet = 45; break;
                    case 6: thet = 225; break;
                    case 7: thet = 135; break;
                }
                thet = thet * 3.14 / 180;
                positions[i].X = ships[0].position.X + fx * (float) Math.Cos(thet) - fy * (float) Math.Sin(thet);
                positions[i].Y = ships[0].position.Y + fx * (float) Math.Sin(thet) + fy * (float) Math.Cos(thet);
            } return positions;
        }

        //need to return the ship's position to continue

        public void Update() {
            //if pressed z, update the formation number and pick out a new formation
            ((Player) ships[0]).setDirectionWithKeys();
            Vector2[] goal = getFormationGoal();
            ships[0].Update();
            for (int i = 1; i < 5; i++) {
                AIShip ship = (AIShip) ships[i];
                if (ship.target(goal[i-1])) { //if close to the target, snap to the grid
                    ship.position = new Vector2(goal[i-1].X, goal[i-1].Y);
                    ship.setDirection(ships[0].getDirection());
                } ships[i].Update();
            }
        }

        public void Draw(SpriteBatch batch) {
            for (int i = 0; i < ships.Length; i++) {
                ships[i].Draw(batch);
            }
        }
    }
}
