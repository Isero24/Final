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

namespace Final
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ModelManager : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public List<BasicModel> models = new List<BasicModel>();
        public List<FlightShip> playerVehicles = new List<FlightShip>();
        public List<Bullet> bulletList = new List<Bullet>();

        GraphicsDevice graphics;
        Camera camera;
        Model bulletTexture;

        BasicEffect effect;

        public ModelManager(Game game, Camera c)
            : base(game)
        {
            graphics = ((Game1)Game).GraphicsDevice;
            camera = c;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            bulletTexture = Game.Content.Load<Model>("bullet");
            effect = new BasicEffect(graphics);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {           
            // Loop through all models and call Update
            for (int i = 0; i < models.Count; ++i)
            {
                models[i].Update(gameTime);
            }

            foreach (Bullet bullet in bulletList)
            {
                bullet.Update(gameTime);
            }
            bulletList.RemoveAll(b => !b.isAlive);

            foreach (FlightShip flightShip in playerVehicles)
            {
                flightShip.Update(gameTime);
                if (flightShip.hasFired)
                {
                    bulletList.Add(new Bullet(bulletTexture, playerVehicles[0].modelPosition, 15f, playerVehicles[0].modelRotation, flightShip.owner));
                    flightShip.hasFired = false;
                }
            }

            // Check for bullet vs flightship collisions
            foreach (Bullet b in bulletList)
            {
                foreach (FlightShip fs in playerVehicles)
                {
                    if (fs.getBoundingSphere().Intersects(b.getBoundingSphere()) && !(fs.owner.Equals(b.owner)))
                    {
                        b.isAlive = false;
                        
                        if (fs.shield > 0)
                        {
                            fs.shield -= b.damage;
                        }
                        else
                        {
                            fs.health -= b.damage;

                            if (fs.health <= 0)
                            {
                                fs.isAlive = false;
                            }
                        }

                        ((Game1)Game).Window.Title = String.Format("Is enemy defeated?: {0}", fs.isAlive);

                    }
                }
            }
            bulletList.RemoveAll(b => !b.isAlive);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Loop through and draw each model
            foreach (BasicModel bm in models)
            {
                bm.Draw(camera, graphics);
            }

            foreach (FlightShip fs in playerVehicles)
            {
                fs.Draw(camera, graphics);
            }

            foreach (Bullet b in bulletList)
            {
                b.Draw(camera, graphics);
            }

            base.Draw(gameTime);
        }
    }
}
