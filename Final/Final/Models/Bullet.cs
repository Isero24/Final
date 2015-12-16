using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Final
{
    public class Bullet : BasicModel
    {
        public bool isAlive = true;
        public float lifeTime;
        public float damage;
        public string owner;

        static public float timeToLive;
        static public float moveSpeed;

        public Bullet (Model model, Vector3 position, float scale, Quaternion rotation, string ownerName)
            : base (model, position, scale)
        {
            modelRotation = rotation;
            owner = ownerName;

            moveSpeed = 50.0f;
            lifeTime = 0.0f;
            timeToLive = 3.0f;
            damage = 50.0f;

            modelPosition += Vector3.Transform(new Vector3(0, 0, -1), modelRotation) * 55.0f;
        }

        public override void Update(GameTime gameTime)
        {
            Update_BulletTime(gameTime);
            Update_BulletPosition(gameTime);
        }

        protected void Update_BulletPosition(GameTime gameTime)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), modelRotation);
            modelPosition += addVector * moveSpeed;
        }

        protected void Update_BulletTime(GameTime gameTime)
        {
            lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (lifeTime >= timeToLive)
            {
                isAlive = false;
            }
        }

        public override void Draw(Camera camera, GraphicsDevice graphics)
        {
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            graphics.RasterizerState = rs;

            graphics.BlendState = BlendState.AlphaBlend;

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            int meshIndex = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect be in mesh.Effects)
                {
                    be.Projection = camera.projection;
                    be.View = camera.view;
                    be.DiffuseColor = Color.Orange.ToVector3();
                    be.World = transforms[mesh.ParentBone.Index] * GetWorld();

                }
                meshIndex++;
                mesh.Draw();
            }

            base.Draw(camera, graphics);
        }

        public override Matrix GetWorld()
        {
            Random rand = new Random();
            Vector3 alteredPosition = modelPosition;

            float xValue = MathHelper.Lerp(-10, 10, (float)rand.NextDouble());
            float yValue = MathHelper.Lerp(-10, 10, (float)rand.NextDouble());

            alteredPosition.X += xValue;
            alteredPosition.Y += yValue;

            return world = Matrix.CreateScale(scale) * Matrix.CreateRotationY(MathHelper.PiOver2) * Matrix.CreateFromQuaternion(modelRotation) * Matrix.CreateTranslation(alteredPosition);
        }
    }
}
