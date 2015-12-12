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
        public virtual void Update() { }

        public void Draw(Camera camera)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.EnableDefaultLighting();
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.World = GetWorld() * mesh.ParentBone.Transform;
                }

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
