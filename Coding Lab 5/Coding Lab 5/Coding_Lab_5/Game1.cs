/***********************************
 * DOCUMENTATION
 * 
 * States list:
 * 1: menu screen
 * 2: alive
 * 3: dead
 * 4: game over
 * 5: credits
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

        public double angle(Ship ship1, Vector2 ship2)
        {
            double x = ship1.position.X - 25 - ship2.X + 25;
            double y = -(ship1.position.Y - 25 - ship2.Y + 25);

            double angle = Math.Atan2(y, x) * 180 / Math.PI;
            if (angle < 0) angle += 360;

            return angle;
        }

        public Vector2 toMinimap(Vector2 original)
        {
            Vector2 minimap;
            minimap.X = original.X / fullWindow.X * minimapTexture.Width;
            minimap.Y = original.Y / fullWindow.Y * minimapTexture.Height;

            return minimap + new Vector2(500, 500);
        }
        #endregion

        // global gameplay variables
        int lives = 5;

        // gameplay mechanics
        Vector2 fullWindow = new Vector2(10000f, 10000f);
        Vector2 windowSize = new Vector2(700f, 700f);
        float gunCooldown = 0.5f, rocketCooldown = 5, arCooldown = 2f, laserCooldown = 10;
        float rocketAmmo = 3, arAmmo = 5, laserAmmo = 2;
        float carrierSpeed = 2.5f;
        int boostSpeed = 7;
        int homingPower = 2;
        int chargeTime = 5;
        int numEnemies = 200;
        int numAsteroids = 200;
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
        List<Vector2> nStars, fStars;
        SpriteFont titleFont, gameFont;
        ButtonState lastDownState, lastUpState;
        int state = 5;
        int gunState = 1;
        double[] cooldowns;
        double[] ammo;
        double laserCharge;
        float vibrationPower;
        int menuChoice = 1;

        // texture files
        Texture2D carrierTexture, bigCarrierTexture, nStarTexture, fStarTexture, livesTexture,
            enemyTexture, minimapTexture, enemyDot, asteroidDot, carrierDot, asteroidTexture1,
            asteroidTexture2, asteroidTexture3, asteroidTexture4;

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
            carrier = new Ship("carrier", new Vector2(fullWindow.X / 2, fullWindow.Y - 2000), 90);
            bullets = new List<Projectile>();
            enemies = new List<Ship>();
            asteroids = new List<Asteroid>();
            nStars = new List<Vector2>();
            fStars = new List<Vector2>();
            cooldowns = new double[4];
            ammo = new double[3];

            ammo[0] = rocketAmmo;
            ammo[1] = arAmmo;
            ammo[2] = laserAmmo;

            for (int i = 0; i < numNearStars; i++) nStars.Add(new Vector2(randomizer.Next(0, (int)fullWindow.X), randomizer.Next(0, (int)fullWindow.Y)));
            for (int i = 0; i < numFarStars; i++) fStars.Add(new Vector2(randomizer.Next(0, (int)fullWindow.X), randomizer.Next(0, (int)fullWindow.Y)));

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

            carrier.speed = carrierSpeed;
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
            bigCarrierTexture = Content.Load<Texture2D>("bigcarrier");
            nStarTexture = Content.Load<Texture2D>("NearStar");
            fStarTexture = Content.Load<Texture2D>("FarStar");
            livesTexture = Content.Load<Texture2D>("lives");
            enemyTexture = Content.Load<Texture2D>("carrier");
            minimapTexture = Content.Load<Texture2D>("minimap");
            enemyDot = Content.Load<Texture2D>("enemyDot");
            asteroidDot = Content.Load<Texture2D>("asteroidDot");
            carrierDot = Content.Load<Texture2D>("carrierDot");
            asteroidTexture1 = Content.Load<Texture2D>("asteroid1");
            asteroidTexture2 = Content.Load<Texture2D>("asteroid2");
            asteroidTexture3 = Content.Load<Texture2D>("asteroid3");
            asteroidTexture4 = Content.Load<Texture2D>("asteroid4");
            titleFont = Content.Load<SpriteFont>("TitleFont");
            gameFont = Content.Load<SpriteFont>("GameFont");

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
                ButtonState downState = GamePad.GetState(PlayerIndex.One).DPad.Down;
                ButtonState upState = GamePad.GetState(PlayerIndex.One).DPad.Down;

                if (menuChoice > 1 && lastDownState == ButtonState.Pressed && downState == ButtonState.Released)
                    menuChoice++;
                else if (menuChoice < 3 && lastUpState == ButtonState.Pressed && upState == ButtonState.Released)
                    menuChoice--;

                if (GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.1f ||
                    GamePad.GetState(PlayerIndex.One).Triggers.Right >= 0.1f)
                {
                    switch (menuChoice)
                    {
                        case 1: state = 2; break;
                        case 2: state = 5; break;
                        case 3: this.Exit(); break;
                    }
                }
            }
            if (state == 2)
            {
                #region panning
                if (carrier.position.X - windowSize.X / 2 >= 0 && carrier.position.X + windowSize.X / 2 <= fullWindow.X)
                    camera.window.X = carrier.position.X - windowSize.X / 2;
                if (carrier.position.Y - windowSize.Y / 2 >= 0 && carrier.position.Y + windowSize.Y / 2 <= fullWindow.Y)
                    camera.window.Y = carrier.position.Y - windowSize.Y / 2;
                #endregion

                #region moving player
                //if (Keyboard.GetState().IsKeyDown(Keys.Up)) { carrier.angle = 90; carrier.speed = boostSpeed; }
                //if (Keyboard.GetState().IsKeyDown(Keys.Down)) { carrier.angle = 270; carrier.speed = boostSpeed; }
                //if (Keyboard.GetState().IsKeyDown(Keys.Right)) { carrier.angle = 0; carrier.speed = boostSpeed; }
                //if (Keyboard.GetState().IsKeyDown(Keys.Left)) { carrier.angle = 180; carrier.speed = boostSpeed; }
                //else { if (carrier.speed >= carrierSpeed) carrier.speed -= 0.1f; }

                if (Math.Abs(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X) > 0 &&
                    Math.Abs(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y) > 0)
                    carrier.angle = (int)(Math.Atan2(GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.Y,
                    GamePad.GetState(PlayerIndex.One).ThumbSticks.Left.X) * 180 / Math.PI);

                if (GamePad.GetState(PlayerIndex.One).Triggers.Left >= 0.01f) carrier.speed = boostSpeed;
                else { if (carrier.speed >= carrierSpeed) carrier.speed -= 0.1f; }
                #endregion

                #region shooting
                //if (Keyboard.GetState().IsKeyDown(Keys.Space))
                if (GamePad.GetState(PlayerIndex.One).Triggers.Right >= 0.5f ||
                    Math.Abs(GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X) >= 0.01f ||
                    Math.Abs(GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y) >= 0.01f)
                {
                    if (gunState == 1 && cooldowns[0] <= 0) // shoot bullet
                    {
                        Vector2 position = carrier.position +
                            new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 -
                            new Vector2(24, 24);
                        int angle = carrier.angle;

                        bullets.Add(new Projectile("bullet", position, angle));
                        cooldowns[0] = gunCooldown;
                    }
                    else if (gunState == 2 && cooldowns[1] <= 0 && ammo[0] >= 1) // shoot rocket
                    {
                        Vector2 position = carrier.position +
                            new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 -
                            new Vector2(24, 24);
                        int angle = carrier.angle;

                        bullets.Add(new Projectile("rocket", position, angle));
                        cooldowns[1] = rocketCooldown;
                        ammo[0]--;
                    }
                    else if (gunState == 3 && cooldowns[2] <= 0 && ammo[1] >= 1)
                    {
                        Vector2 position = carrier.position +
                            new Vector2(carrierTexture.Width, carrierTexture.Height) / 2 -
                            new Vector2(24, 24);
                        int angle = carrier.angle;

                        Projectile bullet1 = new Projectile("bullet", position, angle - 10);
                        Projectile bullet2 = new Projectile("bullet", position, angle);
                        Projectile bullet3 = new Projectile("bullet", position, angle + 10);

                        bullets.Add(new Projectile("bullet", position, angle - 10));
                        bullets.Add(new Projectile("bullet", position, angle));
                        bullets.Add(new Projectile("bullet", position, angle + 10));

                        cooldowns[2] = arCooldown;
                        ammo[1]--;
                    }
                    else if (gunState == 4 && cooldowns[3] <= 0 && ammo[2] >= 1)
                    {
                        laserCharge += 0.1;

                        if (laserCharge >= chargeTime)
                        {
                            Vector2 position = camera.window + new Vector2(windowSize.X / 2 + 15, 0);
                            int angle = carrier.angle;

                            bullets.Add(new Projectile("laser", position, angle));
                            laserCharge = 0;
                            cooldowns[3] = laserCooldown;
                            ammo[2]--;
                        }
                    }
                }
                else laserCharge = 0;
                #endregion

                #region collisions
                for (int i = 0; i < enemies.Count; i++)
                {
                    // enemy vs player
                    if (i < enemies.Count && collide(carrier, enemies[i]))
                    {
                        lives--;
                        carrier.position = new Vector2(fullWindow.X / 2, fullWindow.Y - 2000);
                        if (lives == 0) state = 4;
                        else state = 3;
                        vibrationPower = 1;

                        enemies.RemoveAt(i);
                    }

                    // enemy vs bullet
                    for (int j = 0; j < bullets.Count; j++)
                    {
                        if (i < enemies.Count && j < bullets.Count && collide(bullets[j], enemies[i]))
                        {
                            enemies.RemoveAt(i);
                            if (bullets[j].type != "laser") bullets.RemoveAt(j);
                        }
                    }

                    // enemy vs asteroid
                    for (int j = 0; j < asteroids.Count; j++)
                    {
                        if (i < enemies.Count && j < asteroids.Count && collide(asteroids[j], enemies[i]))
                        {
                            enemies.RemoveAt(i);
                            asteroids.RemoveAt(j);
                        }
                    }
                }

                for (int i = 0; i < asteroids.Count; i++)
                {
                    // asteroid vs player
                    if (i < asteroids.Count && collide(asteroids[i], carrier))
                    {
                        lives--;
                        carrier.position = new Vector2(fullWindow.X / 2, fullWindow.Y - 2000);
                        if (lives == 0) state = 4;
                        else state = 3;
                        vibrationPower = 1;

                        asteroids.RemoveAt(i);
                    }

                    // asteroid vs bullet
                    for (int j = 0; j < bullets.Count; j++)
                    {
                        if (i < asteroids.Count && j < bullets.Count && collide(bullets[j], asteroids[i]))
                        {
                            asteroids.RemoveAt(i);
                            bullets.RemoveAt(j);
                        }
                    }
                }
                #endregion

                carrier.Move();
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (i < enemies.Count)
                    {
                        enemies[i].Move();
                        if (enemies[i].position.X < 0 || enemies[i].position.X > fullWindow.X ||
                            enemies[i].position.Y < 0 || enemies[i].position.Y > fullWindow.Y)
                            enemies.Remove(enemies[i]);
                    }
                }
            }
            else if (state == 3)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Start == ButtonState.Pressed) state = 2;
            }
            else if (state == 4)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                {
                    state = 1;
                    lives = 5;
                }
            }
            else if (state == 5)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed) state = 1;
            }

            #region moving projectiles
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed) gunState = 1;
            else if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed) gunState = 2;
            else if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed) gunState = 3;
            else if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed) gunState = 4;

            for (int i = 0; i < cooldowns.Length; i++) cooldowns[i] -= 0.03;
            if (ammo[0] < rocketAmmo) ammo[0] += 0.0015;
            if (ammo[1] < arAmmo) ammo[1] += 0.0015;
            if (ammo[2] < laserAmmo) ammo[2] += 0.0015;
            if (vibrationPower > 0) vibrationPower -= 0.005f;
            if (vibrationPower < 0) vibrationPower = 0;

            if (enemies.Count() < numEnemies)
            {
                Vector2 position = new Vector2(randomizer.Next(0, (int)fullWindow.X),
                    randomizer.Next(0, (int)fullWindow.Y));

                enemies.Add(new Ship("enemy", position, (int)angle(carrier, position)));
            }

            lastDownState = GamePad.GetState(PlayerIndex.One).DPad.Down;
            lastUpState = GamePad.GetState(PlayerIndex.One).DPad.Up;

            GamePad.SetVibration(PlayerIndex.One, vibrationPower, vibrationPower);
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
            foreach (Vector2 position in nStars) camera.Draw(nStarTexture, position, 90, spriteBatch);
            foreach (Vector2 position in fStars) camera.Draw(fStarTexture, position, 90, spriteBatch);

            if (state == 1)
            {
                spriteBatch.Draw(bigCarrierTexture, new Vector2(windowSize.X / 2 - 24, 50), Color.White);
                spriteBatch.DrawString(titleFont, "+ conians", new Vector2(windowSize.X / 2 - 50, 175), Color.Red);
                spriteBatch.DrawString(titleFont, "Play", new Vector2(windowSize.X / 2 - 20, 300),
                    menuChoice == 1 ? Color.Blue : Color.White);
                spriteBatch.DrawString(titleFont, "Credits", new Vector2(windowSize.X / 2 - 47, 375),
                    menuChoice == 2 ? Color.Blue : Color.White);
                spriteBatch.DrawString(titleFont, "Exit", new Vector2(windowSize.X / 2 - 17, 450),
                    menuChoice == 3 ? Color.Blue : Color.White);
            }
            else if (state == 5)
            {
                spriteBatch.DrawString(titleFont, "Enoch Kumala", new Vector2(windowSize.X / 2 - 125, 200), Color.Green);
                spriteBatch.DrawString(titleFont, "Julian Ochoa", new Vector2(windowSize.X / 2 - 115, 300), Color.Blue);
                spriteBatch.DrawString(titleFont, "Matthew Moltzau", new Vector2(windowSize.X / 2 - 175, 400), Color.Red);
                spriteBatch.DrawString(titleFont, "Michael Para", new Vector2(windowSize.X / 2 - 125, 500), Color.White);
            }
            else
            {
                // draw bullets
                foreach (Projectile bullet in bullets)
                    camera.Draw(Content.Load<Texture2D>(bullet.type), bullet.position, bullet.angle, spriteBatch);

                if (state == 2)
                {
                    camera.Draw(carrierTexture, carrier.position, carrier.angle, spriteBatch);

                    // draw enemy ships
                    foreach (Ship enemy in enemies) camera.Draw(enemyTexture, enemy.position, enemy.angle, spriteBatch);

                    // draw asteroids
                    foreach (Asteroid asteroid in asteroids)
                    {
                        switch (asteroid.type)
                        {
                            case 1: camera.Draw(asteroidTexture1, asteroid.position, 90, spriteBatch); break;
                            case 2: camera.Draw(asteroidTexture2, asteroid.position, 90, spriteBatch); break;
                            case 3: camera.Draw(asteroidTexture3, asteroid.position, 90, spriteBatch); break;
                            case 4: camera.Draw(asteroidTexture4, asteroid.position, 90, spriteBatch); break;
                        }
                    }
                }
                else if (state == 3)
                {
                    spriteBatch.DrawString(gameFont, "ur ded x-|", new Vector2(100, 100), Color.White);
                }
                else if (state == 4)
                {
                    spriteBatch.DrawString(gameFont, "game over x-(", new Vector2(100, 100), Color.White);
                }

                // draw lives
                for (int i = 0; i < lives; i++)
                {
                    Vector2 position = new Vector2(i * 40, windowSize.Y / 2);
                    spriteBatch.Draw(livesTexture, new Vector2(i * 35 + 20, windowSize.Y - 20), Color.White);
                }

                // draw minimap
                spriteBatch.Draw(minimapTexture, new Vector2(500, 500), Color.White);
                foreach (Asteroid asteroid in asteroids) spriteBatch.Draw(asteroidDot, toMinimap(asteroid.position), Color.White);
                foreach (Ship enemy in enemies) spriteBatch.Draw(enemyDot, toMinimap(enemy.position), Color.White);
                spriteBatch.Draw(carrierDot, toMinimap(carrier.position), Color.White);

                if (gunState > 1) spriteBatch.DrawString(gameFont, "" + Math.Round(ammo[gunState - 2], 0), new Vector2(windowSize.X / 2 - 50, windowSize.Y - 30), Color.White);
                spriteBatch.DrawString(gameFont, "" + gunState, new Vector2(windowSize.X / 2, windowSize.Y - 30), Color.White);
                spriteBatch.DrawString(gameFont, "" + laserCharge, new Vector2(windowSize.X / 2 + 50, windowSize.Y - 30), Color.White);
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

        public void Draw(Texture2D texture, Vector2 position, int angle, SpriteBatch spriteBatch)
        {
            Vector2 drawPosition = position - window;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            double radians = (450 - angle) * Math.PI / 180; // 450 - angle because xna flips y coords

            if (drawPosition.X + texture.Width >= 0 && drawPosition.X <= windowSize.X &&
                drawPosition.Y + texture.Height >= 0 && drawPosition.Y <= windowSize.Y)
                spriteBatch.Draw(texture, drawPosition, null, Color.White, (float)radians,
                    origin, 1f, SpriteEffects.None, 0f);
        }
    }

    public class Ship
    {
        // gameplay mechanics
        public const int enemySpeed = 6;

        public string type;
        public float speed;
        public Vector2 position;
        public Vector2 velocity;
        public int angle;

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
        public int angle;

        public Projectile(string newType, Vector2 newPosition, int newAngle)
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
