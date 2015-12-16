using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    public class FlightShip : BasicModel
    {
        public bool hasFired;
        public bool isAlive;
        public float health;
        public float shield;
        public string owner;

        static float gameSpeed = 5.0f;

        float timeBetweenShots;
        bool isStationary;

        public FlightShip(Model model, Vector3 position, float scale, bool isStationary, string ownerName)
            : base(model, position, scale)
        {
            // Constructor
            this.isStationary = isStationary;
            owner = ownerName;

            timeBetweenShots = 0;
            hasFired = false;
            isAlive = true;
            health = 1000f;
            shield = 1000f;
        }

        public override void Update(GameTime gameTime)
        {
            if (!isStationary)
            {
                Update_ProcessInput(gameTime);
                float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 50.0f * gameSpeed;

                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    MoveForward(1f);
                }
            }

            base.Update(gameTime);
        }

        private void Update_ProcessInput(GameTime gameTime)
        {
            float leftRightRot = 0;

            float turningSpeed = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 10000.0f;
            turningSpeed *= 1.6f * gameSpeed;
            KeyboardState keys = Keyboard.GetState();
            if (keys.IsKeyDown(Keys.D))
                leftRightRot += turningSpeed;
            if (keys.IsKeyDown(Keys.A))
                leftRightRot -= turningSpeed;

            float upDownRot = 0;
            if (keys.IsKeyDown(Keys.S))
                upDownRot += turningSpeed;
            if (keys.IsKeyDown(Keys.W))
                upDownRot -= turningSpeed;

            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
            modelRotation *= additionalRot;

            timeBetweenShots += (float)gameTime.TotalGameTime.Seconds;
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                if (timeBetweenShots > 100f)
                {
                    hasFired = true;
                    timeBetweenShots = 0;
                }
            }
        }

        private void MoveForward(float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), modelRotation);
            modelPosition += addVector * speed;
        }

        // Virtual: can be overriden by classes that derive from BasicModel
        // Allows subclasses to apply different scales, rotations, and translations
        public override Matrix GetWorld()
        {
            return world = Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.TwoPi) * Matrix.CreateRotationY(MathHelper.TwoPi / 2) * Matrix.CreateFromQuaternion(modelRotation) * Matrix.CreateTranslation(modelPosition);
        }
    }
}
