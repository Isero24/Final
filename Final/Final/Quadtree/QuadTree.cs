using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Final
{
    public class QuadTree
    {
        private QuadNode _rootNode;
        private TreeVertexCollection _vertices;
        private BufferManager _buffers;
        private Vector3 _position;
        private int _topNodeSize;

        private Vector3 _cameraPosition;
        private Vector3 _lastCameraPosition;
        public List<int> ind;
        public int[] Indices;

        public Matrix View;
        public Matrix Projection;

        public GraphicsDevice Device;

        public BasicEffect Effect;

        public int MinimumDepth = 6;

        public int TopNodeSize { get { return _topNodeSize; } }
        public QuadNode RootNode { get { return _rootNode; } }
        public TreeVertexCollection Vertices { get { return _vertices; } }
        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set { _cameraPosition = value; }
        }
        internal BoundingFrustum ViewFrustrum { get; set; }
        internal int IndexCount { get; set; }

        public QuadTree(Vector3 position, Texture2D heightMap, Matrix viewMatrix, Matrix projectionMatrix, GraphicsDevice device, int scale)
        {
            Device = device;
            _position = position;
            _topNodeSize = heightMap.Width - 1;

            _vertices = new TreeVertexCollection(position, heightMap, scale);
            _buffers = new BufferManager(_vertices.Vertices, device);
            _rootNode = new QuadNode(NodeType.FullNode, _topNodeSize, 1, null, this, 0);

            View = viewMatrix;
            Projection = projectionMatrix;

            //Initialize the bounding frustrum to be used for culling later.
            ViewFrustrum = new BoundingFrustum(viewMatrix * projectionMatrix);

            //Construct an array large enough to hold all of the indices we'll need
            Indices = new int[((heightMap.Width + 1) * (heightMap.Height + 1)) * 3];

            Effect = new BasicEffect(Device);
            Effect.EnableDefaultLighting();
            Effect.FogEnabled = true;
            Effect.FogStart = 300f;
            Effect.FogEnd = 1000f;
            Effect.FogColor = Color.Black.ToVector3();
            Effect.TextureEnabled = true;
            Effect.Texture = new Texture2D(device, 100, 100);
            Effect.Projection = projectionMatrix;
            Effect.View = viewMatrix;
            Effect.World = Matrix.Identity;
        }

        public void Update(GameTime gameTime)
        {
            //Only update if the camera position has changed
            if (_cameraPosition == _lastCameraPosition)
                return;

            Effect.View = View;
            Effect.Projection = Projection;

            _lastCameraPosition = _cameraPosition;
            IndexCount = 0;

            _rootNode.EnforceMinimumDepth();
            ind = new List<int>();
            _rootNode.SetActiveVertices();

            _buffers.UpdateIndexBuffer(Indices, IndexCount);
            _buffers.SwapBuffer();
        }

        public void Draw(GameTime gameTime)
        {
            Device.SetVertexBuffer(_buffers.VertexBuffer);
            Device.Indices = _buffers.IndexBuffer;

            Device.SamplerStates[0] = SamplerState.LinearClamp;

            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _vertices.Vertices.Length, 0, IndexCount / 3);
            }

        }

        internal void UpdateBuffer(int vIndex)
        {
            Indices[IndexCount] = vIndex;
            ind.Add(vIndex);
            IndexCount++;
        }
    }
}
