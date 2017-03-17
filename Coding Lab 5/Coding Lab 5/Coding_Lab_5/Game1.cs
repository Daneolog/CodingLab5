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
        public bool collide(Projectile projectile, Ship ship)
        {
            Vector2 pSize, sSize;

            switch (projectile.type)
            {
                case "bullet": pSize = new Vector2(8, 20); break;
                case "rocket": pSize = new Vector2(8, 20); break;
                case "laser": pSize = new Vector2(20, windowSize.Y / 2); break;
                default: pSize = Vector2.Zero; break;
            }

            switch (ship.type)
            {
                case "enemy": sSize = new Vector2(50, 50); break;
                default: sSize = Vector2.Zero; break;
            }

            return projectile.position.X <= ship.position.X + sSize.X &&
                projectile.position.X + pSize.X >= ship.position.X &&
                projectile.position.Y <= ship.position.Y + sSize.Y &&
                projectile.position.Y + pSize.Y >= ship.position.Y;

        }

        public bool collide(Ship carrier, Ship enemy)
        {
            return carrier.position.X <= enemy.position.X + 50 &&
                carrier.position.X + 50 >= enemy.position.X &&
                carrier.position.Y <= enemy.position.Y + 50 &&
                carrier.position.Y + 50 >= enemy.position.Y;

        }

        public double distance(Projectile projectile, Ship ship)
        {
            return Math.Sqrt(Math.Pow(projectile.position.X - ship.position.X, 2) +
                Math.Pow(projectile.position.Y - ship.position.Y, 2));
        }

        public double angle(Projectile projectile, Ship ship)
        {
            double x = ship.position.X + 25 - projectile.position.X - 16;
            double y = -(ship.position.Y + 25 - projectile.position.Y - 10);

            double angle = Math.Atan2(y, x) * 180 / Math.PI;
            if (angle < 0) angle += 360;

            return angle;
        }

        // gameplay mechanics
        Vector2 fullWindow = new Vector2(10000f, 10000f);
        Vector2 window = new Vector2(0f, 0f);
        Vector2 windowSize = new Vector2(800f, 600f);
        int homingPower = 2;
        int chargeTime = 5;
        static Random rnd = new Random();
        int NStarLimit = 6000;
        int FStarLimit = 7000;

        // temporary variables (don't change)
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D backgroundTexture, carrierTexture;
        Ship closestEnemy;
        Ship carrier;
        List<Ship> enemies;
        List<Projectile> bullets;
        int state = 1; // 1: alive in main map, 2: dead in main map, 3: alive in minimap, 4: dead in minimap
        int gunState = 1; // 1: regular gun, 2: rocket, 3: assault rifle, 4: laser.  planning to add 5: tractor beam?
        double[] cooldowns;
        double laserCharge;
        List<Vector2> Nstars;
        List<Vector2> Fstars;

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
            carrier = new Ship("carrier", new Vector2(9000, 5000), 90);
            enemies = new List<Ship>();
            bullets = new List<Projectile>();
            cooldowns = new double[4];
            Nstars = new List<Vector2>();
            Fstars = new List<Vector2>();

            carrierTexture = Content.Load<Texture2D>("carrier");
            
            for (int c = 0; c < NStarLimit; c++)
            {
                Nstars.Add(new Vector2(rnd.Next(0, (int)fullWindow.X), rnd.Next(0, (int)fullWindow.Y)));
            }
            for (int c = 0; c < FStarLimit; c++)
            {
                Fstars.Add(new Vector2(rnd.Next(0, (int)fullWindow.X), rnd.Next(0, (int)fullWindow.Y)));
            }

            // TESTING CODE (UNCOMMENT AFTER USE)
            Ship enemy1 = new Ship("enemy", new Vector2(9300, 4000), 0);
            Ship enemy2 = new Ship("enemy", new Vector2(9300, 4100), 0);
            enemies.Add(enemy1);
            enemies.Add(enemy2);
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
            if (state == 1)
            {
                #region panning
                if (carrier.position.X - windowSize.X / 2 >= 0 && carrier.position.X + windowSize.X / 2 <= fullWindow.X)
                    window.X = carrier.position.X - windowSize.X / 2;
                if (carrier.position.Y - windowSize.Y / 2 >= 0 && carrier.position.Y + windowSize.Y / 2 <= fullWindow.Y)
                    window.Y = carrier.position.Y - windowSize.Y / 2;
                #endregion

                #region moving
                carrier.speed = 5f;

                if (Keyboard.GetState().IsKeyDown(Keys.Up)) { carrier.angle = 90; carrier.speed = 7f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) { carrier.angle = 270; carrier.speed = 7f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) { carrier.angle = 0; carrier.speed = 7f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) { carrier.angle = 180; carrier.speed = 7f; }
                #endregion

                #region shooting
                // shoot bullet
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    if (gunState == 1 && cooldowns[gunState - 1] <= 0)
                    {
                        Projectile bullet = new Projectile("bullet",
                            carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(4, 10),
                            carrier.angle);
                        bullets.Add(bullet);

                        cooldowns[gunState - 1] = 0.5;
                    }
                    else if (gunState == 2 && cooldowns[gunState - 1] <= 0)
                    {
                        Projectile rocket = new Projectile("rocket",
                            carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(16, 10),
                            carrier.angle);
                        bullets.Add(rocket);

                        cooldowns[gunState - 1] = 5;
                    }
                    else if (gunState == 3 && cooldowns[gunState - 1] <= 0)
                    {
                        Vector2 position = carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(4, 10);
                        double angle = carrier.angle;

                        Projectile bullet1 = new Projectile("bullet", position, angle - 10);
                        Projectile bullet2 = new Projectile("bullet", position, angle);
                        Projectile bullet3 = new Projectile("bullet", position, angle + 10);

                        bullets.Add(bullet1);
                        bullets.Add(bullet2);
                        bullets.Add(bullet3);

                        cooldowns[gunState - 1] = 1;
                    }
                    else if (gunState == 4 && cooldowns[gunState - 1] <= 0)
                    {
                        Vector2 position = window + new Vector2(windowSize.X / 2 + 15, 0);
                        double angle = carrier.angle;

                        Projectile laser = new Projectile("laser", position, angle);

                        laserCharge += 0.1;

                        if (laserCharge >= chargeTime)
                        {
                            bullets.Add(laser);
                            laserCharge = 0;

                            cooldowns[gunState - 1] = 10;
                        }
                    }
                }
                else laserCharge = 0;
                #endregion

                #region moving
                // move bullets
                for (int i = 0; i < bullets.Count; i++)
                {
                    Vector2 position = bullets[i].position - window;

                    //if (bullets[i].type == "rocket") // home rockets
                    //{
                    //    for (int j = 0; j < enemies.Count; j++)
                    //    {
                    //        if (closestEnemy == null) closestEnemy = enemies[j];
                    //        else if (distance(bullets[i], enemies[j]) < distance(bullets[i], closestEnemy))
                    //            closestEnemy = enemies[j];
                    //    }

                    //    if (enemies.Count > 0)
                    //    {
                    //        if (bullets[i].angle < angle(bullets[i], closestEnemy)) bullets[i].angle += homingPower;
                    //        else if (bullets[i].angle > angle(bullets[i], closestEnemy)) bullets[i].angle -= homingPower;

                    //        Console.WriteLine(bullets[i].angle + " going to " + angle(bullets[i], closestEnemy));
                    //    }
                    //}

                    bullets[i].Move();
                    if (bullets[i].type == "bullet" && (position.X + 8 <= 0 || position.X >= windowSize.X ||
                        position.Y + 20 <= 0 || position.Y >= windowSize.Y)) bullets.RemoveAt(i);
                    else if (bullets[i].type == "rocket" && (position.X + 32 <= 0 || position.X >= windowSize.X ||
                        position.Y + 20 <= 0 || position.Y >= windowSize.Y)) bullets.RemoveAt(i);
                    else if (bullets[i].type == "laser" && (position.X + 20 <= 0 || position.X >= windowSize.X ||
                        position.Y + 20 <= 0 || position.Y + 300 >= windowSize.Y)) bullets.RemoveAt(i);
                }
                #endregion

                #region collisions
                for (int i = 0; i < enemies.Count; i++)
                {
                    // carrier vs enemy
                    if (collide(carrier, enemies[i])) state = 2;

                    // enemy vs bullet
                    for (int j = 0; j < bullets.Count; j++)
                    {
                        if (i < enemies.Count && j < bullets.Count && collide(bullets[j], enemies[i]))
                        {
                            if (bullets[j].type != "laser") bullets.RemoveAt(j);
                            enemies.RemoveAt(i);
                        }
                    }

                    // enemy vs asteroid
                }
                #endregion

                if (Keyboard.GetState().IsKeyDown(Keys.D1)) gunState = 1;
                else if (Keyboard.GetState().IsKeyDown(Keys.D2)) gunState = 2;
                else if (Keyboard.GetState().IsKeyDown(Keys.D3)) gunState = 3;
                else if (Keyboard.GetState().IsKeyDown(Keys.D4)) gunState = 4;

                carrier.Move();

                for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] -= 0.03;
            }
            else if (state == 2)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Z) || Keyboard.GetState().IsKeyDown(Keys.X)) state = 1;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Rectangle backgroundRect = new Rectangle((int)-window.X, (int)-window.Y,
                (int)(backgroundTexture.Width * Math.Ceiling(fullWindow.X / backgroundTexture.Width)),
                (int)(backgroundTexture.Height * Math.Ceiling(fullWindow.Y / backgroundTexture.Height)));

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            //draw stars
            foreach (Vector2 position in Nstars) spriteBatch.Draw(Content.Load<Texture2D>("NearStar"), position - window, Color.White);
            foreach (Vector2 position in Fstars) spriteBatch.Draw(Content.Load<Texture2D>("FarStar"), position - window, Color.White);

            spriteBatch.Draw(Content.Load<Texture2D>("background"), backgroundRect, Color.White);

            // draw formation ships
            if (state == 1)
            {
                spriteBatch.Draw(Content.Load<Texture2D>("carrier"), carrier.position - window, Color.White);
            }

            // draw enemy ships
            for (int i = 0; i < enemies.Count; i++)
                spriteBatch.Draw(Content.Load<Texture2D>("carrier"), enemies[i].position - window, Color.White);

            // draw bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                Texture2D texture;

                if (bullets[i].type == "bullet") texture = Content.Load<Texture2D>("bullet");
                else if (bullets[i].type == "rocket") texture = Content.Load<Texture2D>("rocket");
                else if (bullets[i].type == "laser") texture = Content.Load<Texture2D>("laser");
                else texture = Content.Load<Texture2D>("bullet");

                spriteBatch.Draw(texture, bullets[i].position - window, Color.White);
            }

            spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "" + gunState, new Vector2(windowSize.X / 2, windowSize.Y - 30), Color.White);
            spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "" + laserCharge, new Vector2(windowSize.X / 2 + 50, windowSize.Y - 30), Color.White);

            if (state == 2)
            {
                spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "enemy x u", new Vector2(100, 100), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class Ship
    {
        // gameplay mechanics
        public const int enemySpeed = 4;

        public string type;
        public float speed;
        public Vector2 position;
        public Vector2 velocity;
        public double angle;

        public Ship(string newType, Vector2 newPosition, int newAngle)
        {
            type = newType;
            position = newPosition;
            angle = newAngle;

            if (type == "enemy") speed = enemySpeed;
        }

        public void Move()
        {
            double radians = angle * Math.PI / 180;

            velocity = speed * new Vector2((float)Math.Cos(radians), (float)-Math.Sin(radians));
            position += velocity;
        }
    }

    public class Projectile
    {
        // projectile mechanics
        public const int bulletSpeed = 10;
        public const int rocketSpeed = 7;

        public string type;
        public float speed;
        public Vector2 position;
        public Vector2 velocity;
        public double angle;

        public Projectile(string newType, Vector2 newPosition, double newAngle)
        {
            type = newType;
            position = newPosition;
            angle = newAngle;

            if (type == "bullet") speed = bulletSpeed;
            else if (type == "rocket") speed = rocketSpeed;
            else if (type == "laser") speed = 20;
        }

        public void Move()
        {
            if (angle < 0) angle += 360;
            double radians = angle * Math.PI / 180;

            velocity = speed * new Vector2((float)Math.Cos(radians), (float)-Math.Sin(radians));
            position += velocity;
        }
    }
}
