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

namespace Coding_Lab_5
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        // gameplay mechanics
        Vector2 fullWindow = new Vector2(2000f, 2000f);
        Vector2 window = new Vector2(0f, 0f);
        Vector2 windowSize = new Vector2(600f, 600f);
        Texture2D backgroundTexture, carrierTexture;

        // temporary variables (don't change)
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Carrier carrier = new Carrier();
        List<Projectile> bullets;
        Projectile rocket;
        double bulletCooldown, rocketCooldown;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = (int)windowSize.X;
            graphics.PreferredBackBufferHeight = (int)windowSize.Y;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            carrier.position = new Vector2(100, 100);

            bullets = new List<Projectile>();
            backgroundTexture = Content.Load<Texture2D>("background");
            carrierTexture = Content.Load<Texture2D>("carrier");

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here
            #region panning
            if (carrier.position.X - windowSize.X / 2 >= 0 && carrier.position.X + windowSize.X / 2 <= fullWindow.X)
                window.X = carrier.position.X - windowSize.X / 2;
            if (carrier.position.Y - windowSize.Y / 2 >= 0 && carrier.position.Y + windowSize.Y / 2 <= fullWindow.Y)
                window.Y = carrier.position.Y - windowSize.Y / 2;
            #endregion

            #region moving
            carrier.speed = 1.5f;
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) { carrier.angle = 270; carrier.speed = 2f; }
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) { carrier.angle = 90; carrier.speed = 2f; }
            if (Keyboard.GetState().IsKeyDown(Keys.Right)) { carrier.angle = 0; carrier.speed = 2f; }
            if (Keyboard.GetState().IsKeyDown(Keys.Left)) { carrier.angle = 180; carrier.speed = 2f; }
            #endregion

            #region shooting
            // shoot bullet with z key
            if (Keyboard.GetState().IsKeyDown(Keys.Z) && bulletCooldown <= 0)
            {
                Projectile bullet = new Projectile("bullet",
                    carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(10, 10),
                    carrier.angle);
                bullets.Add(bullet);

                bulletCooldown = 2;
            }

            // shoot rocket with x key
            if (Keyboard.GetState().IsKeyDown(Keys.X) && rocketCooldown <= 0)
            {
                rocket = new Projectile("rocket",
                    carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(16, 10),
                    carrier.angle);

                rocketCooldown = 10;
            }

            // move bullets
            for (int i=0; i<bullets.Count; i++)
            {
                bullets[i].Update();
                if (bullets[i].distanceTraveled >= 300) bullets.RemoveAt(i);
            }

            // move rocket
            if (rocket != null)
            {
                rocket.Update();
                if (rocket.distanceTraveled >= 300) rocket = null;
            }
            #endregion

            carrier.Move();
            if (bulletCooldown > 0) bulletCooldown -= 0.1;
            if (rocketCooldown > 0) rocketCooldown -= 0.1;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            float angle = (float)(carrier.angle * Math.PI / 180);
            Rectangle backgroundRect = new Rectangle((int)-window.X, (int)-window.Y,
                (int)(backgroundTexture.Width * Math.Ceiling(fullWindow.X / backgroundTexture.Width)),
                (int)(backgroundTexture.Height * Math.Ceiling(fullWindow.Y / backgroundTexture.Height)));

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(Content.Load<Texture2D>("background"), backgroundRect, Color.White);

            for (int i=0; i<bullets.Count; i++)
                spriteBatch.Draw(Content.Load<Texture2D>("bullet"), bullets[i].position - window, Color.White);
            if (rocket != null) spriteBatch.Draw(Content.Load<Texture2D>("rocket"), rocket.position, Color.White);

            spriteBatch.Draw(Content.Load<Texture2D>("carrier"), carrier.position - window, Color.White);
            spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "" + (int)rocketCooldown,
                new Vector2(windowSize.X / 2, windowSize.Y - 30), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class Carrier
    {
        public float speed;
        public Vector2 position;
        public Vector2 velocity;
        public int angle;

        public Carrier()
        {
            velocity = Vector2.Zero;
        }

        public void Move()
        {
            double angleInRadians = angle * Math.PI / 180;

            velocity = speed * new Vector2((float)Math.Cos(angleInRadians), (float)Math.Sin(angleInRadians));
            position += velocity;
        }
    }

    public class Projectile
    {
        // projectile mechanics
        public static int bulletSpeed = 5;
        public static int rocketSpeed = 3;

        public string type;
        public Vector2 position;
        public Vector2 velocity;
        public double angle;
        public int distanceTraveled; // to know when to despawn

        public Projectile(string newType, Vector2 newPosition, int newAngle)
        {
            type = newType;
            position = newPosition;
            angle = newAngle * Math.PI / 180;
            distanceTraveled = 0;

            switch (type)
            {
                case "bullet":
                    velocity = bulletSpeed * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    break;
                case "rocket":
                    velocity = rocketSpeed * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    break;
            }
        }

        public void Update()
        {
            position += velocity;
            distanceTraveled += (int)Math.Sqrt(Math.Pow(velocity.X, 2) + Math.Pow(velocity.Y, 2));
        }
    }
}
