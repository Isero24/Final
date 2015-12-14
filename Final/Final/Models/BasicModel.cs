using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    class BasicModel
    {

        public Model model { get; protected set; }

        protected GraphicsDevice graphicsDevice;
        protected Matrix world = Matrix.Identity;
        protected Vector3 modelPosition;
        protected Vector3 velocity;
        protected Quaternion modelRotation;
        protected MouseState prevMouseState;

        private float mainTurretAngle = 0;

        public BasicModel(Model model, Vector3 position, GraphicsDevice graphics)
        {
            this.model = model;

            // Initialize to defaults
            velocity = Vector3.Zero;
            modelRotation = Quaternion.Identity;
            prevMouseState = Mouse.GetState();
            graphicsDevice = graphics;
            modelPosition = position;

            // Applys a vector3 starting position to the world matrix
            world *= Matrix.CreateTranslation(position);
        }

        // Can be overriden by classes that derive from BasicModel
        public virtual void Update()
        {
            //mainTurretAngle -= 0.015f;
        }

        public void Draw(Camera camera)
        {
            Matrix[] meshWorldMatrices = new Matrix[model.Meshes.Count];
            meshWorldMatrices[0] = Matrix.CreateRotationY(mainTurretAngle);
            meshWorldMatrices[1] = Matrix.CreateRotationY(mainTurretAngle);
            meshWorldMatrices[2] = Matrix.CreateRotationY(mainTurretAngle);
            meshWorldMatrices[3] = Matrix.CreateTranslation(new Vector3(0, 0, 0));

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            int meshIndex = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.World = GetWorld() * transforms[mesh.ParentBone.Index] * meshWorldMatrices[meshIndex];
                }
                meshIndex++;
                mesh.Draw();
            }
        }

        // Virtual: can be overriden by classes that derive from BasicModel
        // Allows subclasses to apply different scales, rotations, and translations
        public virtual Matrix GetWorld()
        {
            return world;
        }
    }
}
