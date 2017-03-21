/***********************************
 * DOCUMENTATION
 * 
 * States list:
 * 1: alive in main map
 * 2: dead in main map
 * 3: alive in minimap
 * 4: dead in minimap
 * 5: pausing in any map
 * 
 * Guns list:
 * 1: regular gun
 * 2: rocket
 * 3: assault rifle
 * 4: laser.
 * planning to add 5: tractor beam?
 ***********************************/

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
        #region gameplay functions
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

        public bool collide(Projectile projectile, Asteroid asteroid)
        {
            Vector2 pSize;

            switch (projectile.type)
            {
                case "bullet": pSize = new Vector2(8, 20); break;
                case "rocket": pSize = new Vector2(8, 20); break;
                case "laser": pSize = new Vector2(20, windowSize.Y / 2); break;
                default: pSize = Vector2.Zero; break;
            }

            return projectile.position.X <= asteroid.position.X + 32 &&
                projectile.position.X + pSize.X >= asteroid.position.X &&
                projectile.position.Y <= asteroid.position.Y + 32 &&
                projectile.position.Y + pSize.Y >= asteroid.position.Y;

        }

        public bool collide(Asteroid asteroid, Ship ship)
        {
            return asteroid.position.X <= ship.position.X + 50 &&
                asteroid.position.X + 32 >= ship.position.X &&
                asteroid.position.Y <= ship.position.Y + 50 &&
                asteroid.position.Y + 32 >= ship.position.Y;

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
        #endregion

        // gameplay mechanics
        Vector2 fullWindow = new Vector2(10000f, 10000f);
        Vector2 windowSize = new Vector2(700f, 700f);
        float gunCooldown = 0.5f, rocketCooldown = 5, arCooldown = 2f, laserCooldown = 10;
        int carrierSpeed = 5;
        int homingPower = 2;
        int chargeTime = 5;
        int numEnemies = 500;
        int numAsteroids = 1000;
        int numNearStars = 6000;
        int numFarStars = 7000;

        // temporary variables (don't change)
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Camera camera;
        Random randomizer = new Random();
        Ship closestEnemy;
        Ship carrier;
        List<Projectile> bullets;
        List<Ship> enemies;
        List<Asteroid> asteroids;
        List<Vector2> nStars;
        List<Vector2> fStars;
        KeyboardState last;
        int state = 1;
        int gunState = 1;
        double[] cooldowns;
        double laserCharge;

        // texture files
        Texture2D carrierTexture, nStarTexture, fStarTexture, enemyTexture, asteroidTexture1, asteroidTexture2,
            asteroidTexture3, asteroidTexture4;

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
            camera = new Camera(windowSize);
            carrier = new Ship("carrier", new Vector2(windowSize.X / 2, windowSize.Y - 100), 90);
            bullets = new List<Projectile>();
            enemies = new List<Ship>();
            asteroids = new List<Asteroid>();
            nStars = new List<Vector2>();
            fStars = new List<Vector2>();
            cooldowns = new double[4];

            for (int i=0; i<numNearStars; i++) nStars.Add(new Vector2(randomizer.Next(0, (int)fullWindow.X), randomizer.Next(0, (int)fullWindow.Y)));
            for (int i=0; i<numFarStars; i++) fStars.Add(new Vector2(randomizer.Next(0, (int)fullWindow.X), randomizer.Next(0, (int)fullWindow.Y)));

            for (int i = 0; i < numEnemies; i++)
            {
                Vector2 position = new Vector2(randomizer.Next(0, (int)fullWindow.X), randomizer.Next(0, (int)fullWindow.Y));
                int angle = randomizer.Next(0, 8) * 45;
                
                enemies.Add(new Ship("enemy", position, angle));
            }

            for (int i = 0; i < numAsteroids; i++)
            {
                Vector2 position = new Vector2(randomizer.Next(0, (int)fullWindow.X), randomizer.Next(0, (int)fullWindow.Y));
                int type = randomizer.Next(1, 4);

                asteroids.Add(new Asteroid(position, type));
            }

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
            carrierTexture = Content.Load<Texture2D>("carrier");
            nStarTexture = Content.Load<Texture2D>("NearStar");
            fStarTexture = Content.Load<Texture2D>("FarStar");
            enemyTexture = Content.Load<Texture2D>("carrier");
            asteroidTexture1 = Content.Load<Texture2D>("asteroid1");
            asteroidTexture2 = Content.Load<Texture2D>("asteroid2");
            asteroidTexture3 = Content.Load<Texture2D>("asteroid3");
            asteroidTexture4 = Content.Load<Texture2D>("asteroid4");

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
            #region state 1
            if (state == 1)
            {
                #region panning
                if (carrier.position.X - windowSize.X / 2 >= 0 && carrier.position.X + windowSize.X / 2 <= fullWindow.X)
                    camera.window.X = carrier.position.X - windowSize.X / 2;
                if (carrier.position.Y - windowSize.Y / 2 >= 0 && carrier.position.Y + windowSize.Y / 2 <= fullWindow.Y)
                    camera.window.Y = carrier.position.Y - windowSize.Y / 2;
                #endregion

                #region moving
                carrier.speed = carrierSpeed;

                if (Keyboard.GetState().IsKeyDown(Keys.Up)) { carrier.angle = 90; carrier.speed = 7f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Down)) { carrier.angle = 270; carrier.speed = 7f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Right)) { carrier.angle = 0; carrier.speed = 7f; }
                if (Keyboard.GetState().IsKeyDown(Keys.Left)) { carrier.angle = 180; carrier.speed = 7f; }
                #endregion

                #region collisions
                for (int i = 0; i < enemies.Count; i++)
                {
                    // enemy vs player
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
                    for (int j = 0; j < asteroids.Count; j++)
                    {
                        if (i < enemies.Count && j < asteroids.Count && collide(asteroids[j], enemies[i]))
                        {
                            asteroids.RemoveAt(j);
                            enemies.RemoveAt(i);
                        }
                    }
                }

                for (int i = 0; i < asteroids.Count; i++)
                {
                    // asteroid vs player
                    if (collide(asteroids[i], carrier)) state = 2;

                    // asteroid vs bullet
                    for (int j = 0; j < bullets.Count; j++)
                    {
                        if (i < asteroids.Count && j < bullets.Count && collide(bullets[j], asteroids[i]))
                        {
                            bullets.RemoveAt(j);
                            asteroids.RemoveAt(i);
                        }
                    }
                }
                #endregion

                carrier.Move();
                foreach (Ship enemy in enemies) enemy.Move();
            }
            #endregion
            else if (state == 2)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space)) state = 1;
            }

            #region shooting
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                if (gunState == 1 && cooldowns[gunState - 1] <= 0) // shoot bullet
                {
                    Vector2 position = carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(4, 10);
                    double angle = carrier.angle;

                    bullets.Add(new Projectile("bullet", position, angle));
                    cooldowns[gunState - 1] = gunCooldown;
                }
                else if (gunState == 2 && cooldowns[gunState - 1] <= 0) // shoot rocket
                {
                    Vector2 position = carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(16, 10);
                    double angle = carrier.angle;

                    bullets.Add(new Projectile("rocket", position, angle));
                    cooldowns[gunState - 1] = rocketCooldown;
                }
                else if (gunState == 3 && cooldowns[gunState - 1] <= 0)
                {
                    Vector2 position = carrier.position + new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 - new Vector2(4, 10);
                    double angle = carrier.angle;

                    Projectile bullet1 = new Projectile("bullet", position, angle - 10);
                    Projectile bullet2 = new Projectile("bullet", position, angle);
                    Projectile bullet3 = new Projectile("bullet", position, angle + 10);

                    bullets.Add(new Projectile("bullet", position, angle - 10));
                    bullets.Add(new Projectile("bullet", position, angle));
                    bullets.Add(new Projectile("bullet", position, angle + 10));

                    cooldowns[gunState - 1] = arCooldown;
                }
                else if (gunState == 4 && cooldowns[gunState - 1] <= 0)
                {
                    laserCharge += 0.1;

                    if (laserCharge >= chargeTime)
                    {
                        Vector2 position = camera.window + new Vector2(windowSize.X / 2 + 15, 0);
                        double angle = carrier.angle;

                        bullets.Add(new Projectile("laser", position, angle));
                        laserCharge = 0;
                        cooldowns[gunState - 1] = laserCooldown;
                    }
                }
            }
            else laserCharge = 0;
            #endregion

            #region moving
            // move bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                Vector2 position = bullets[i].position - camera.window;

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

            if (Keyboard.GetState().IsKeyDown(Keys.D1)) gunState = 1;
            else if (Keyboard.GetState().IsKeyDown(Keys.D2)) gunState = 2;
            else if (Keyboard.GetState().IsKeyDown(Keys.D3)) gunState = 3;
            else if (Keyboard.GetState().IsKeyDown(Keys.D4)) gunState = 4;

            for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] -= 0.03;
            last = Keyboard.GetState();
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            // draw stars
            foreach (Vector2 position in nStars) camera.Draw(nStarTexture, position, spriteBatch);
            foreach (Vector2 position in fStars) camera.Draw(fStarTexture, position, spriteBatch);

            // draw carrier
            if (state == 1) camera.Draw(carrierTexture, carrier.position, spriteBatch);

            // draw bullets
            foreach (Projectile bullet in bullets)
                camera.Draw(Content.Load<Texture2D>(bullet.type), bullet.position, spriteBatch);

            spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "" + gunState, new Vector2(windowSize.X / 2, windowSize.Y - 30), Color.White);
            spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "" + laserCharge, new Vector2(windowSize.X / 2 + 50, windowSize.Y - 30), Color.White);
            
            if (state == 1)
            {
                // draw enemy ships
                foreach (Ship enemy in enemies) camera.Draw(enemyTexture, enemy.position, spriteBatch);

                // draw asteroids
                foreach (Asteroid asteroid in asteroids)
                {
                    if (asteroid.type == 1) camera.Draw(asteroidTexture1, asteroid.position, spriteBatch);
                    else if (asteroid.type == 2) camera.Draw(asteroidTexture2, asteroid.position, spriteBatch);
                    else if (asteroid.type == 3) camera.Draw(asteroidTexture3, asteroid.position, spriteBatch);
                    else if (asteroid.type == 4) camera.Draw(asteroidTexture4, asteroid.position, spriteBatch);
                }
            }
            else if (state == 2)
            {
                spriteBatch.DrawString(Content.Load<SpriteFont>("GameFont"), "ur ded x-|", new Vector2(100, 100), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public class Camera
    {
        public Vector2 window;
        public Vector2 windowSize;

        public Camera(Vector2 nWindowSize)
        {
            windowSize = nWindowSize;
        }

        public void Draw(Texture2D texture, Vector2 position, SpriteBatch spriteBatch)
        {
            Vector2 drawPosition = position - window;

            if (drawPosition.X + texture.Width >= 0 && drawPosition.X <= windowSize.X &&
                drawPosition.Y + texture.Height >= 0 && drawPosition.Y <= windowSize.Y)
                spriteBatch.Draw(texture, drawPosition, Color.White);
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
        }

        public void Move()
        {
            if (angle < 0) angle += 360;
            double radians = angle * Math.PI / 180;

            velocity = speed * new Vector2((float)Math.Cos(radians), (float)-Math.Sin(radians));
            position += velocity;
        }
    }

    public class Asteroid
    {
        public Vector2 position;
        public int type;

        public Asteroid(Vector2 newPosition, int newType)
        {
            position = newPosition;
            type = newType;
        }
    }
}
