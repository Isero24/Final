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
        public FlightShip(Model model, Vector3 position, GraphicsDevice graphics)
            : base(model, position, graphics)
        {
            // Constructor
        }

        public override void Update()
        {
            Vector2 rot = Vector2.Zero;

            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Mouse.GetState().X <= graphicsDevice.Viewport.Width / 3)
            {
                // Modifying rotation by a turning speed to the left.
                rot.Y = 1f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Mouse.GetState().X >= 2 * graphicsDevice.Viewport.Width / 3)
            {
                // Modifying rotation by a turning speed to the right.
                rot.Y = -1f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up) || Mouse.GetState().Y <= graphicsDevice.Viewport.Height / 3)
            {
                // Modifying rotation by a turning speed upwards.
                rot.X = 1f;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) || Mouse.GetState().Y >= 2 * graphicsDevice.Viewport.Height / 3)
            {
                // Modifying rotation by a turning speed downwards.
                rot.X = -1f;
            }

            //Quaternion qRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), xRot);
            Quaternion qRot = 
                Quaternion.CreateFromAxisAngle(new Vector3(-1, 0, 0), rot.X * 0.01f) *
                Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), rot.Y * 0.01f);

            modelRotation *= qRot;

            // Obtain the models direction
            Vector3 modelDirection = Vector3.Transform(Vector3.Forward, modelRotation);

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                // Moves model forward
                // Modify models position by a direction applied by a speed modifier.
                modelPosition += modelDirection * 0.5f;
            }

            prevMouseState = Mouse.GetState();

            base.Update();
        }

        // Virtual: can be overriden by classes that derive from BasicModel
        // Allows subclasses to apply different scales, rotations, and translations
        public override Matrix GetWorld()
        {
            return world = Matrix.CreateFromQuaternion(modelRotation) * Matrix.CreateTranslation(modelPosition);
        }
    }
}
