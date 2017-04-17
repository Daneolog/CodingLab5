using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

//Nothing in this file is set. I want to do something in here but I
//currently don't feel like I have the time right now.
namespace Bosconian {

    interface ShipAI {

    }

    class BoomerangShip : LinearSpriteSheet, ShipAI {

        public BoomerangShip(Texture2D image, Vector2 pos) : base(image, pos) {

        }

        public override void Update() {
            throw new NotImplementedException();
        }
    }

    //Ship Movement
    //  (EITHER)
    //  Can back
    //  or
    //  Restricted
    //  
    //  (AI)
    //  Favor Diagnols
    //  Favor Straight
    //  Scatter
    //  
    //  Player
    //  or
    //  ShipAI
    //  
    //  In restricted mode if the player makes an incorrect move
    //  we simply prevent it from occuring. For an AI however,
    //  we need it to fire the first valid option.
}
