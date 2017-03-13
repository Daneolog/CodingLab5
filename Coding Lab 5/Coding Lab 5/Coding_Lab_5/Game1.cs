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
        Vector2 fullWindow = new Vector2(10000f, 10000f);
        Vector2 window = new Vector2(0f, 0f);
        Vector2 windowSize = new Vector2(600f, 600f);
        Texture2D backgroundTexture, carrierTexture;

        // temporary variables (don't change)
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Ship carrier = new Ship("carrier", new Vector2(9000, 5000), 90);
        List<Ship> enemies;
        List<Projectile> bullets;
        List<Projectile> rockets;
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
            enemies = new List<Ship>();
            bullets = new List<Projectile>();
            rockets = new List<Projectile>();

            backgroundTexture = Content.Load<Texture2D>("background");
            carrierTexture = Content.Load<Texture2D>("carrier");

            // TESTING CODE (UNCOMMENT AFTER USE)
            Ship enemy = new Ship("enemy", new Vector2(8500, 5000), 0);
            enemies.Add(enemy);
            // END OF TESTING CODE

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
            carrier.speed = 5f;

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            { carrier.angle = 270; carrier.speed = 7f; }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            { carrier.angle = 90; carrier.speed = 7f; }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            { carrier.angle = 0; carrier.speed = 7f; }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            { carrier.angle = 180; carrier.speed = 7f; }
            #endregion

            #region shooting
            // shoot bullet with z key
            if (Keyboard.GetState().IsKeyDown(Keys.Z) && bulletCooldown <= 0)
            {
                Projectile bullet = new Projectile("bullet",
                    carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(10, 10),
                    carrier.angle);
                bullets.Add(bullet);

                bulletCooldown = 0.3;
            }

            // shoot rocket with x key
            if (Keyboard.GetState().IsKeyDown(Keys.X) && rocketCooldown <= 0)
            {
                Projectile rocket = new Projectile("rocket",
                    carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(16, 10),
                    carrier.angle);
                rockets.Add(rocket);

                rocketCooldown = 5;
            }
            #endregion

            #region moving
            // move bullets
            for (int i=0; i<bullets.Count; i++)
            {
                Vector2 position = bullets[i].position - window;

                bullets[i].Move();
                if (position.X + 20 <= 0 || position.X >= windowSize.X ||
                    position.Y + 20 <= 0 || position.Y >= windowSize.Y) bullets.RemoveAt(i);
            }

            // move rockets
            for (int i=0; i<rockets.Count; i++)
            {
                Vector2 position = rockets[i].position - window;

                rockets[i].Move();
                if (position.X + 20 <= 0 || position.X >= windowSize.X ||
                    position.Y + 20 <= 0 || position.Y >= windowSize.Y) rockets.RemoveAt(i);
            }
            #endregion

            carrier.Move();
            if (bulletCooldown > 0) bulletCooldown -= 0.03;
            if (rocketCooldown > 0) rocketCooldown -= 0.03;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Rectangle backgroundRect = new Rectangle((int)-window.X, (int)-window.Y,
                (int)(backgroundTexture.Width * Math.Ceiling(fullWindow.X / backgroundTexture.Width)),
                (int)(backgroundTexture.Height * Math.Ceiling(fullWindow.Y / backgroundTexture.Height)));

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            spriteBatch.Draw(Content.Load<Texture2D>("background"), backgroundRect, Color.White);

            for (int i=0; i<bullets.Count; i++)
                spriteBatch.Draw(Content.Load<Texture2D>("bullet"), bullets[i].position - window, Color.White);
            
            for (int i=0; i<rockets.Count; i++)
                spriteBatch.Draw(Content.Load<Texture2D>("rocket"), rockets[i].position - window, Color.White);

            spriteBatch.Draw(Content.Load<Texture2D>("carrier"), carrier.position - window, Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("carrier"), carrier.position - window + new Vector2(-50, 50), Color.White);
            spriteBatch.Draw(Content.Load<Texture2D>("carrier"), carrier.position - window + new Vector2(50, 50), Color.White);

            for (int i = 0; i < enemies.Count; i++)
                spriteBatch.Draw(Content.Load<Texture2D>("carrier"), enemies[i].position - window, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
    
    public class Ship
    {
        // gameplay mechanics
        public const int enemySpeed = 4;
        
        public float speed;
        public Vector2 position;
        public Vector2 velocity;
        public double angle;

        public Ship(string type, Vector2 newPosition, int newAngle)
        {
            if (type == "enemy") speed = enemySpeed;
            position = newPosition;
            angle = newAngle;
        }

        public void Move()
        {
            double radians = angle * Math.PI / 180;

            velocity = speed * new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            position += velocity;
        }
    }
    
    public class Projectile
    {
        // projectile mechanics
        public const int bulletSpeed = 10;
        public const int rocketSpeed = 7;

        public float speed;
        public Vector2 position;
        public Vector2 velocity;
        public double angle;

        public Projectile(string type, Vector2 newPosition, double newAngle)
        {
            if (type == "bullet") speed = bulletSpeed;
            else if (type == "rocket") speed = rocketSpeed;
            position = newPosition;
            angle = newAngle;
        }

        public void Move()
        {
            double radians = angle * Math.PI / 180;

            velocity = speed * new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
            position += velocity;
        }
    }
}
