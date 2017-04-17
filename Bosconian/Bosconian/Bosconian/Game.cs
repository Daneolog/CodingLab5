using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Bosconian {
    
    public class Game : Microsoft.Xna.Framework.Game {

        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public Random rand = new Random();
        public static GamePadState gamepad; //Two different static states we
        public static KeyboardState keys;   //have to control the inputs.
        public Vector2 spawn {get{return new Vector2(64f, 64f);}}

        //List of Stars
        List<Point> stars = new List<Point>();
        //List of sitting objects
        //List of Enemy Ships
        //List of projectiles (bullets and missiles)
        public Player ship;
        public Fleet playerFleet;

        public Game() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            //base.Initialize() calls LoadContent
            base.Initialize();
            //ship = new Player(spawn);
            playerFleet = new Fleet(spawn);

            camera = new Camera(GraphicsDevice.Viewport);
            camera.Location = spawn;
            camera.Zoom = 2;
            for (int i = 0; i < 100; i++)
                stars.Add(new Point(rand.Next(camera.Width/2), rand.Next(camera.Height/2)));
        }

        ////////////////////////////////////////////////////////////////////////////////////
        //Begin Texture Logic
        ////////////////////////////////////////////////////////////////////////////////////

        public static Camera camera;
        public static Texture2D[] textures;
        public enum TextureNames {
            SHIP,
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            String[] textureNames = new String[] {
                "Ship",
            };
            textures = new Texture2D[textureNames.Length];
            for (int i = 0; i < textureNames.Length; i++)
                textures[i] = Content.Load<Texture2D>(textureNames[i]);
        }

        protected override void UnloadContent() {
            spriteBatch.Dispose();
            //The following line should unload everything
            Content.Unload();
        }

        ////////////////////////////////////////////////////////////////////////////////////
        //End Texture Logic
        ////////////////////////////////////////////////////////////////////////////////////

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
            gamepad = GamePad.GetState(PlayerIndex.One);
            keys = Keyboard.GetState();
            if (gamepad.Buttons.Back == ButtonState.Pressed) Exit();
            if (keys.IsKeyDown(Keys.Escape)) Exit();
            playerFleet.Update();
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            //SamplerState.PointClamp is used so scaled images aren't blurry.
            //The other null values act as defaults. Note the camera is used here.
            camera.Location = playerFleet.cameraPosition;
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.TransformMatrix);
            playerFleet.Draw(spriteBatch);

            Texture2D SimpleTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Int32[] pixel = {0xFFFFFF};
            SimpleTexture.SetData<Int32>(pixel, 0, SimpleTexture.Width * SimpleTexture.Height);
            for (int i = 0; i < stars.Count; i++) //turn stars into point later
                spriteBatch.Draw(SimpleTexture, new Rectangle(stars[i].X, stars[i].Y, 1, 1), Color.Silver);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

/*
//How to draw a horizontal line:
Texture2D SimpleTexture = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
Int32[] pixel = {0xFFFFFF};
SimpleTexture.SetData<Int32>(pixel, 0, SimpleTexture.Width * SimpleTexture.Height);
spriteBatch.Draw(SimpleTexture, new Rectangle(0, 0, 64, 1), Color.Blue);
*/
