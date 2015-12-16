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
    public class Camera : Microsoft.Xna.Framework.GameComponent
    {
        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }
        public Vector3 cameraPosition { get; set; }

        protected Vector3 cameraUp;
        protected Vector3 cameraDirection;

        protected MouseState prevMouseState;
        protected KeyboardState prevKeyboardState;

        public Camera(Game game, Vector3 cameraPosition, Vector3 target, Vector3 cameraUp)
            : base(game)
        {
            this.cameraPosition = cameraPosition;
            this.cameraUp = cameraUp;

            cameraDirection = target - cameraPosition;
            cameraDirection.Normalize();

            CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);

            projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)Game.Window.ClientBounds.Width /
                (float)Game.Window.ClientBounds.Height,
                1, 3000);

            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2,
                game.Window.ClientBounds.Height / 2);

            prevMouseState = Mouse.GetState();

            // TODO: Construct any child components here
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

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// 
        public virtual void Update()
        {
            
        }

        public void UpdateCamera(BasicModel model)
        {
            Vector3 campos = new Vector3(0, 50, 200);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(model.modelRotation));
            campos += model.modelPosition;

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(model.modelRotation));

            CreateLookAt(campos, model.modelPosition, camup);
        }


        protected void CreateLookAt(Vector3 cameraPosition, Vector3 target, Vector3 up)
        {
            view = Matrix.CreateLookAt(cameraPosition, target, up);
        }
    }
}
