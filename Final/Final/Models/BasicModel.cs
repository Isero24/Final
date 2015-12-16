using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    public class BasicModel
    {
        public Model model { get; protected set; }
        public Vector3 modelPosition;
        public Quaternion modelRotation;
        public BoundingSphere bounds;

        protected Matrix world = Matrix.Identity;
        protected Vector3 velocity;
        protected MouseState prevMouseState;
        protected float scale;

        private float propellerAngle = 0;

        public BasicModel(Model model, Vector3 position, float scale)
        {
            this.model = model;

            // Initialize to defaults
            velocity = Vector3.Zero;
            modelRotation = Quaternion.Identity;
            prevMouseState = Mouse.GetState();
            modelPosition = position;
            this.scale = scale;

            bounds = getBoundingSphere();
        }

        // Can be overriden by classes that derive from BasicModel
        public virtual void Update(GameTime gameTime)
        {
            propellerAngle -= 0.25f;
        }

        public virtual void Draw(Camera camera, GraphicsDevice graphics)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            graphics.RasterizerState = rs;

            graphics.BlendState = BlendState.AlphaBlend;

            Matrix[] meshWorldMatrices = new Matrix[2];
            meshWorldMatrices[0] = Matrix.CreateTranslation(new Vector3(0, 0, 0));
            meshWorldMatrices[1] = Matrix.CreateRotationZ(propellerAngle);

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
   
            int meshIndex = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.World = transforms[mesh.ParentBone.Index] * meshWorldMatrices[meshIndex] * GetWorld();
                    
                }
                meshIndex++;
                mesh.Draw();
            }
        }

        // Virtual: can be overriden by classes that derive from BasicModel
        // Allows subclasses to apply different scales, rotations, and translations
        public virtual Matrix GetWorld()
        {
            return world * Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(modelRotation) * Matrix.CreateTranslation(modelPosition);
        }

        public BoundingSphere getBoundingSphere()
        {
            BoundingSphere sphere = new BoundingSphere();

            foreach (ModelMesh mesh in model.Meshes)
            {
                if (sphere.Radius == 0)
                    sphere = mesh.BoundingSphere;
                else
                    sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);
            }

            sphere.Center = modelPosition;
            sphere.Radius *= scale;

            return sphere;
        }
    }
}
