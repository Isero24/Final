﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Final
{
    class TreeVertexCollection
    {
        public VertexPositionNormalTexture[] Vertices;

        Vector3 _position;

        int _topSize;
        int _halfSize;
        int _vertexCount;
        int _scale;

        public VertexPositionNormalTexture this[int index]
        {
            get { return Vertices[index]; }
            set { Vertices[index] = value; }
        }

        public TreeVertexCollection(Vector3 position, Texture2D heightMap, int scale)
        {
            _scale = scale;
            _topSize = heightMap.Width - 1;
            _halfSize = _topSize / 2;
            _position = position;
            _vertexCount = heightMap.Width * heightMap.Width;

            // Initialize our array to hold the vertices
            Vertices = new VertexPositionNormalTexture[_vertexCount];

            // Our method to populate the vertex collection
            BuildVertices(heightMap);

            // Our method to calculate the normals for all vertices
            CalculateAllNormals();
        }

        private void BuildVertices(Texture2D heightMap)
        {
            var heightMapColors = new Color[_vertexCount];
            heightMap.GetData(heightMapColors);

            float x = _position.X;
            float z = _position.Z;
            float y = _position.Y;
            float maxX = x + _topSize;

            for (int i = 0; i < _vertexCount; i++)
            {
                if (x > maxX)
                {
                    x = _position.X;
                    z++;
                }

                y = _position.Y + (heightMapColors[i].R / 5.0f);
                var vert = new VertexPositionNormalTexture(new Vector3(x * _scale, y * _scale, z * _scale), Vector3.Zero, Vector2.Zero);
                vert.TextureCoordinate = new Vector2((vert.Position.X - _position.X) / _topSize, (vert.Position.Z - _position.Z) / _topSize);
                Vertices[i] = vert;
                x++;
            }
        }

        private void CalculateAllNormals()
        {
            if (_vertexCount < 9)
                return;

            int i = _topSize + 2, j = 0, k = i + _topSize;

            for (int n = 0; i <= (_vertexCount - _topSize) - 2; i+= 2, n++, j+= 2, k += 2)
            {
                if (n == _halfSize)
                {
                    n = 0;
                    i += _topSize + 2;
                    j += _topSize + 2;
                    k += _topSize + 2;
                }


            }
        }

    }
}
