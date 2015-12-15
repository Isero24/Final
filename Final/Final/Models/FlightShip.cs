using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    class FlightShip : BasicModel
    {
        float gameSpeed = 5.0f;

        public FlightShip(Model model, Vector3 position, GraphicsDevice graphics, float scale)
            : base(model, position, graphics, scale)
        {
            // Constructor
        }

        public override void Update(GameTime gameTime)
        {
            ProcessKeyboard(gameTime);
            float moveSpeed = gameTime.ElapsedGameTime.Milliseconds / 50.0f * gameSpeed;
            MoveForward(moveSpeed);

            base.Update(gameTime);
        }

        private void ProcessKeyboard(GameTime gameTime)
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
