using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    public class FirstPersonCamera : Camera
    {
        float speed;
        float velocity;
        bool isMouseActive = false;

        public FirstPersonCamera(Game game, Vector3 cameraPosition, Vector3 target, Vector3 cameraUp) 
            : base(game, cameraPosition, target, cameraUp)
        {
            velocity = 1;
            speed = 3;
            prevKeyboardState = Keyboard.GetState();
        }

        public override void Update(GameTime gameTime)
        {
            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                    isMouseActive = true;
            }
            else
            {
                isMouseActive = false;
            }

            // Process user input for camera movement
            ProcessInput();

            // Process simple physics for the camera
            //ProcessPhysics();

            prevKeyboardState = Keyboard.GetState();

            CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);

            base.Update(gameTime);
        }

        private void ProcessInput()
        {
           
            if (isMouseActive == true)
            {
                // Yaw rotation
                cameraDirection = Vector3.Transform(cameraDirection,
                    Matrix.CreateFromAxisAngle(cameraUp, (-MathHelper.PiOver4 / 150) *
                    (Mouse.GetState().X - prevMouseState.X)));

                // Pitch rotation
                cameraDirection = Vector3.Transform(cameraDirection,
                    Matrix.CreateFromAxisAngle(Vector3.Cross(cameraUp, cameraDirection),
                    (MathHelper.PiOver4 / 100) *
                    (Mouse.GetState().Y - prevMouseState.Y)));

            }

            // Reset mouseState
            prevMouseState = Mouse.GetState();


            float? futureHeight;

            // Moving forward and backward
            // The height for the next move is calculated before applying it.
            // If the height difference is too much, then the cameras move is not applied.
            // Checks for null values for out of bounds checking
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                futureHeight = ((Game1)Game).terrain.Intersects(new Ray(cameraPosition + cameraDirection * speed, Vector3.Down));

                //if (futureHeight - 25 > 0 && futureHeight != null)
                    cameraPosition += cameraDirection * speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                futureHeight = ((Game1)Game).terrain.Intersects(new Ray(cameraPosition - cameraDirection * speed, Vector3.Down));

                //if (futureHeight - 25 > 0 && futureHeight != null)
                    cameraPosition -= cameraDirection * speed;
            }

            // Speed boost when shift is held down.
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                speed = 6.0f;
            }
            else
            {
                speed = 3.0f;
            }

            // Moving side to side
            // The height for the next move is calculated before applying it.
            // If the height difference is too much, then the cameras move is not applied.
            // Checks for null values for out of bounds checking
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                futureHeight = ((Game1)Game).terrain.Intersects(new Ray(cameraPosition + Vector3.Cross(cameraUp, cameraDirection) * speed, Vector3.Down));

                //if (futureHeight - 25 > 0 && futureHeight != null)
                    cameraPosition += Vector3.Cross(cameraUp, cameraDirection) * speed;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                futureHeight = ((Game1)Game).terrain.Intersects(new Ray(cameraPosition - Vector3.Cross(cameraUp, cameraDirection) * speed, Vector3.Down));

                //if (futureHeight - 25 > 0 && futureHeight != null)
                    cameraPosition -= Vector3.Cross(cameraUp, cameraDirection) * speed;
            }

        }

        private void ProcessPhysics()
        {
            // Gets the current height between the cameras current position and the terrain beneath it
            float? currentHeight = ((Game1)Game).terrain.Intersects(new Ray(cameraPosition, Vector3.Down));

            ((Game1)Game).testText = currentHeight.ToString(); // Output testing for height

            // Gravity Physics
            if (currentHeight - 50.0f > 0.0f)
            {
                cameraPosition += Vector3.Down * velocity;
                velocity += 0.1f;
            }
            
            // Physics when camera falls within ground height
            // Velocity is set to 0
            // When camera changes position to different heights, the difference between its current
            // height and its forced height is added to the cameras y value.
            if (currentHeight - 50.0f < -1.0f)
            {
                velocity = 0;

                Vector3 testV3 = new Vector3(0, 50 - (float)currentHeight, 0);

                cameraPosition += testV3;   
            }

            // Space is for jumping
            // Sets the velocity to -5 so as to simulate a rising and falling mechanic
            // Velocity must be 0, this means that the camera needs to be on the ground with 
            // no falling or rising velocity.
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && velocity == 0)
            {
                velocity = -5.0f;
                cameraPosition += Vector3.Down * velocity;
            }
        }
    }
}
