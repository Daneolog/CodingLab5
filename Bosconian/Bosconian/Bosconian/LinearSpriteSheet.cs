using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Bosconian {
    
    /*
     * A generic sprite class which isn't simply called "Sprite" because the name already
     * exists as an interface. This class isn't animated and the drawBox in this class is
     * not equivalent to a bounding box which might respond to collisions. This class
     * shouldn't be instantiated so it was made abstract.
     */
    public abstract class LinearSpriteSheet {

        protected Texture2D image;
        protected Rectangle drawBox;
        protected Rectangle crop;

        //call using base
        public LinearSpriteSheet(Texture2D image, Vector2 pos, int col = 1) {
            this.image = image;
            drawBox = new Rectangle((int) pos.X, (int) pos.Y, image.Width/col, image.Height);
        }

        public abstract void Update();

        //This method might be overloaded
        public virtual void Draw(SpriteBatch batch) {
            batch.Draw(image, drawBox, Color.White);
        }
    }
}
