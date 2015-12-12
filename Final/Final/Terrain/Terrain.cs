using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Final
{
    public class Terrain : Microsoft.Xna.Framework.DrawableGameComponent
    {
        int vertexCountX;
        int vertexCountZ;
        float blockScale;
        float heightScale;
        //byte[] heightMap;
        Color[] heightMap;
        int numVertices;
        int numTriangles;
        bool isInitialized;
        VertexBuffer vb;
        IndexBuffer ib;
        BasicEffect effect;
        Texture2D heightMapTexture;
        Texture2D terrainTexture;
        VertexDeclaration vertexDeclaration;

        ushort[] indices;

        ushort index2;

        public Vector2 StartPosition
        {
            get
            {
                float terrainHalfWidth = (vertexCountX - 1) * blockScale * 0.5f;
                float terrainHalfDepth = (vertexCountZ - 1) * blockScale * 0.5f;

                return new Vector2(-terrainHalfWidth, -terrainHalfDepth);
            }
        }

        public Terrain(Game game) : base(game)
        {
            isInitialized = false;
        }

        public override void Initialize()
        {
            isInitialized = true;
            base.Initialize();
        }

        public void load(string heightmapFileName, int vertexCountX, int vertexCountZ, float blockScale, float heightScale)
        {
            if (!isInitialized)
                Initialize();

            effect = new BasicEffect(GraphicsDevice);

            heightMapTexture = Game.Content.Load<Texture2D>(heightmapFileName);
            int heightMapSize = heightMapTexture.Width * heightMapTexture.Height;
            heightMap = new Color[heightMapSize];
            heightMapTexture.GetData<Color>(heightMap);

            this.vertexCountX = heightMapTexture.Width;
            this.vertexCountZ = heightMapTexture.Height;
            this.blockScale = blockScale;
            this.heightScale = heightScale;

            // Generate terrain mesh
            GenerateTerrainMesh();

            // Load effect
            //effect = new TerrainEffect 
            terrainTexture = Game.Content.Load<Texture2D>(@"map\Bitmap1");
        }

        private void GenerateTerrainMesh()
        {
            numVertices = vertexCountX * vertexCountZ;
            numTriangles = (vertexCountX - 1) * (vertexCountZ - 1) * 2;

            ushort[] indices = GenerateTerrainIndices();
            this.indices = indices;

            VertexPositionNormalTexture[] vertices = GenerateTerrainVertices();

            CalculateNormals(vertices);

            vb = new VertexBuffer(GraphicsDevice,
                typeof(VertexPositionNormalTexture), numVertices,
                BufferUsage.WriteOnly);

            vb.SetData<VertexPositionNormalTexture>(vertices);

            ib = new IndexBuffer(GraphicsDevice, typeof(ushort), numTriangles * 3,
                BufferUsage.WriteOnly);

            ib.SetData<ushort>(indices);
            
        }

        private ushort[] GenerateTerrainIndices()
        {
            int numIndices = numTriangles * 3;
            ushort[] indices = new ushort[numIndices];

            int indicesCount = 0;
            for (int i = 0; i < (vertexCountZ - 1); i++)
            {
                for (int j = 0; j < (vertexCountX - 1); j++)
                {
                    ushort index = (ushort)(j + i * vertexCountZ);
                    index2 = index;

                    // First Triangle
                    indices[indicesCount++] = index;
                    indices[indicesCount++] = (ushort)(index + 1);
                    indices[indicesCount++] = (ushort)(index + vertexCountX + 1);

                    // Second Triangle
                    indices[indicesCount++] = (ushort)(index + vertexCountX + 1);
                    indices[indicesCount++] = (ushort)(index + vertexCountX);
                    indices[indicesCount++] = index;
                }
            }

            return indices;
        }

        private VertexPositionNormalTexture[] GenerateTerrainVertices()
        {
            float halfTerrainWidth = (vertexCountX - 1) * blockScale * .5f;
            float halfTerrainDepth = (vertexCountZ - 1) * blockScale * .5f;

            float tuDerivative = 1.0f / (vertexCountX - 1);
            float tvDerivative = 1.0f / (vertexCountZ - 1);

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexCountX * vertexCountZ];

            int vertexCount = 0;
            float tu = 0;
            float tv = 0;

            for (float i = -halfTerrainDepth; i <= halfTerrainDepth; i += blockScale)
            {
                tu = 0.0f;
                for (float j = -halfTerrainWidth; j <= halfTerrainWidth; j += blockScale)
                {
                    vertices[vertexCount].Position = new Vector3(j, heightMap[vertexCount].R * heightScale, i);
                    vertices[vertexCount].TextureCoordinate = new Vector2(tu, tv);

                    tu += tuDerivative;
                    vertexCount++;
                }

                tv += tvDerivative;
            }

            return vertices;
        }

        private void CalculateNormals(VertexPositionNormalTexture[] vertices)
        {

            ((Game1)Game).testText = vertices.Length.ToString() + "\n";
            ((Game1)Game).testText += indices.Length.ToString() + "\n";

            for (int i = 0; i < indices.Length; i+= 3)
            {
                Vector3 v1 = vertices[indices[i]].Position;
                Vector3 v2 = vertices[indices[i + 1]].Position;
                Vector3 v3 = vertices[indices[i + 2]].Position;

                Vector3 vu = v3 - v1;
                Vector3 vt = v2 - v1;
                Vector3 normal = Vector3.Cross(vu, vt);
                normal.Normalize();

                vertices[indices[i]].Normal += normal;
                vertices[indices[i + 1]].Normal += normal;
                vertices[indices[i + 2]].Normal += normal;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal.Normalize();
            }
        }

        public override void Draw(GameTime gameTime)
        {

            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            rs.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rs;

            effect.World = Matrix.Identity; // No Transformation of the terrain
            effect.View = ((Game1)Game).fpCamera.view;
            effect.Projection = ((Game1)Game).fpCamera.projection;
            effect.Texture = terrainTexture;
            effect.TextureEnabled = true;

            effect.LightingEnabled = true;
            effect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            effect.PreferPerPixelLighting = true;

            effect.DirectionalLight0.Direction = new Vector3(1, -1, 0);
            effect.DirectionalLight0.DiffuseColor = Color.White.ToVector3();
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight1.Enabled = false;
            effect.DirectionalLight2.Enabled = false;

            GraphicsDevice.SetVertexBuffer(vb); // Set vertices
            GraphicsDevice.Indices = ib; // Set indices

            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            foreach (EffectPass CurrentPass in effect.CurrentTechnique.Passes)
            {
                CurrentPass.Apply();

                int nTriangles = (int)MathHelper.Min(numTriangles, 65535);
                int index = 0;
                int iteration = 1;
                int indexNum = 0;
                //while (nTriangles > 0)
                //{

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, index, 0, 65535 * 3, 0, nTriangles);

                    iteration++;
                    index += 65536 / 2;
                    indexNum += nTriangles - 1;
                    nTriangles = (int)MathHelper.Min(numTriangles - iteration * 65535, 65535);
                //}
            }

            base.Draw(gameTime);
        }

        public float GetHeight(Vector2 position)
        {
            return GetHeight(position.X, position.Y);
        }

        public float GetHeight(Vector3 position)
        {
            return GetHeight(position.X, position.Z);
        }

        private float GetHeight(float positionX, float positionZ)
        {
            float height = -999999.0f;
            if (heightMap == null) return height;

            // Get the position relative to the terrain grid
            Vector2 positionInGrid = new Vector2(
                positionX - StartPosition.X,
                positionZ - StartPosition.Y);

            // Calculate the grid position
            Vector2 blockPosition = new Vector2(
                (int)(positionInGrid.X / blockScale),
                (int)(positionInGrid.Y / blockScale));

            // Check if the object is inside the grid
            if (blockPosition.X >= 0 && blockPosition.X < (vertexCountX - 1) &&
                blockPosition.Y >= 0 && blockPosition.Y < (vertexCountZ - 1))
            {
                Vector2 blockOffset = new Vector2(
                    blockPosition.X - (int)blockPosition.X,
                    blockPosition.Y - (int)blockPosition.Y);

                // Get the height of the four vertices of the grid block
                int vertexIndex = (int)blockPosition.X +
                    (int)blockPosition.Y * vertexCountX;
                float height1 = heightMap[vertexIndex + 1].R;
                float height2 = heightMap[vertexIndex].R;
                float height3 = heightMap[vertexIndex + vertexCountX + 1].R;
                float height4 = heightMap[vertexIndex + vertexCountX].R;

                // Top Triangle
                float heightIncX, heightIncY;
                if (blockOffset.X > blockOffset.Y)
                {
                    heightIncX = height1 - height2;
                    heightIncY = height3 - height1;
                }
                // Bottom Triangle
                else
                {
                    heightIncX = height3 - height4;
                    heightIncY = height4 - height2;
                }

                // Linear interpolation to find the height inside the triangle
                float lerpHeight = height2 + heightIncX * blockOffset.X +
                    heightIncY * blockOffset.Y;
                height = lerpHeight * heightScale;
            }

            return height;
        }

        public float? Intersects(Ray ray)
        {
            // This wont be changed if the Ray doesn't collide with terrain
            float? collisionDistance = null;

            // Size of step is half of blockScale
            Vector3 rayStep = ray.Direction * blockScale * 0.5f;

            // Need to save start position to find total distance once collision point is found
            Vector3 rayStartPosition = ray.Position;

            // Linear search - Loop until you find a point inside and outside the terrain
            Vector3 lastRayPosition = ray.Position;
            ray.Position += rayStep;
            float height = GetHeight(ray.Position);
            while (ray.Position.Y > height && height >= 0)
            {
                lastRayPosition = ray.Position;
                ray.Position += rayStep;
                height = GetHeight(ray.Position);
            }

            // If the ray collides with the terrain
            if (height >= 0) // Lowest possible point of terrain
            {
                Vector3 startPosition = lastRayPosition;
                Vector3 endPosition = ray.Position;

                // Binary search. Find the exact collision point
                for (int i = 0; i < 32; i++)
                {
                    // Binary search pass
                    Vector3 middlePoint = (startPosition + endPosition) * 0.5f;
                    if (middlePoint.Y < height)
                        endPosition = middlePoint;
                    else
                        startPosition = middlePoint;
                }
                Vector3 collisionPoint = (startPosition + endPosition) * 0.5f;
                collisionDistance = Vector3.Distance(rayStartPosition, collisionPoint);
            }

            return collisionDistance;
        }
    }
}
