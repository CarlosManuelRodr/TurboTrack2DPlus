using UnityEngine;

namespace Rendering
{
    /// <summary>
    /// Represents a collection of quad graphic primitives that can be converted
    /// into a Mesh.
    /// </summary>
    class QuadCollection
    {
        // Geometry.
        private readonly Vector3[] _shape;
        private readonly Vector3[] _normals;
        private readonly Vector2[] _uv;
        private readonly Vector2[] _uv2;
        private readonly Color[] _color;
        private readonly int[] _tris;
        
        /// <summary>
        /// Keeps track of the index of the current quad to draw.
        /// </summary>
        private int _currentQuad;
        
        /// <summary>
        /// Instantiate a new Quad collection.
        /// </summary>
        /// <param name="capacity">The amount of quads that the collection will contain.</param>
        public QuadCollection(int capacity = 1000)
        {
            int numberVertexes = 4 * capacity;
            _shape = new Vector3[numberVertexes];
            _normals = new Vector3[numberVertexes];
            _tris = new int[4 * numberVertexes];
            _uv = new Vector2[numberVertexes];
            _uv2 = new Vector2[numberVertexes];
            _color = new Color[numberVertexes];

            // Set-up normals and UVs.
            for (int i = 0; i < numberVertexes; i += 4)
            {
                _normals[i] = -Vector3.forward;
                _normals[i + 1] = -Vector3.forward;
                _normals[i + 2] = -Vector3.forward;
                _normals[i + 3] = -Vector3.forward;

                _color[i] = Color.white;
                _color[i + 1] = Color.white;
                _color[i + 2] = Color.white;
                _color[i + 3] = Color.white;

            }

            // Set-up two triangles for each Quad.
            for (int i = 0, j = 0; i < numberVertexes; i += 6, j += 4)
            {
                // First triangle.
                _tris[i] = 0 + j;
                _tris[i + 1] = 1 + j;
                _tris[i + 2] = 2 + j;
                
                // Second triangle.
                _tris[i + 3] = 0 + j;
                _tris[i + 4] = 2 + j;
                _tris[i + 5] = 3 + j;
            }
        }

        /// <summary>
        /// Fill mesh from the current quad collection.
        /// </summary>
        /// <returns>The generated mesh.</returns>
        public void UpdateMesh(Mesh mesh)
        {
            int drawLength = 4 * _currentQuad;
            int trianglesLength = 6 * _currentQuad;
            
            mesh.Clear();
            mesh.SetVertices(_shape, 0, drawLength);
            mesh.SetTriangles(_tris, 0, trianglesLength, 0);
            mesh.SetNormals(_normals, 0, drawLength);
            mesh.SetUVs(0, _uv, 0, drawLength);
            mesh.SetUVs(1, _uv2, 0, drawLength);
            mesh.SetColors(_color, 0, drawLength);
        }

        /// <summary>
        /// Reset the current quad index.
        /// </summary>
        public void Reset()
        {
            _currentQuad = 0;
        }

        /// <summary>
        /// Update the quad coordinates on the current index position.
        /// </summary>
        /// <param name="p1">The bottom left coordinate.</param>
        /// <param name="p2">The top left coordinate.</param>
        /// <param name="p3">The top right coordinate.</param>
        /// <param name="p4">The bottom right coordinate.</param>
        public void AddQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            int i = 4 * _currentQuad;

            // Bottom left.
            _shape[i].x = p1.x;
            _shape[i].y = p1.y;
            
            // Top left.
            _shape[i + 1].x = p2.x;
            _shape[i + 1].y = p2.y;
            
            // Top right.
            _shape[i + 2].x = (p3.x);
            _shape[i + 2].y = p3.y;
            
            // Bottom right.
            _shape[i + 3].x = p4.x;
            _shape[i + 3].y = p4.y;
            
            // Calculate trapezoid perspective for UVs.
            
            // Zero out the left and bottom edges, 
            // leaving a right trapezoid with two sides on the axes and a vertex at the origin.
            // Source: https://forum.unity.com/threads/correcting-affine-texture-mapping-for-trapezoids.151283/#post-2580356
            Vector2[] shiftedPositions = {
                new(0, 0),
                new(0, _shape[i + 1].y - _shape[i].y),
                new(_shape[i + 2].x - _shape[i + 1].x, _shape[i + 2].y - _shape[i + 3].y),
                new(_shape[i + 3].x - _shape[i].x, 0)
            };

            Vector2[] widthsHeights = new Vector2[4];
            widthsHeights[0].x = widthsHeights[3].x = shiftedPositions[3].x;
            widthsHeights[1].x = widthsHeights[2].x = shiftedPositions[2].x;
            widthsHeights[0].y = widthsHeights[1].y = shiftedPositions[1].y;
            widthsHeights[2].y = widthsHeights[3].y = shiftedPositions[2].y;

            for (int j = 0; j < 4; j++)
            {
                _uv[i + j] = shiftedPositions[j];
                _uv2[i + j] = widthsHeights[j];
            }
            
            _currentQuad += 1;
            
        }
        
        /// <summary>
        /// Update the trapezoid quad coordinates on the current index position.
        /// </summary>
        /// <param name="p1">The lower coordinate.</param>
        /// <param name="w1">The lower width</param>
        /// <param name="p2">The upper coordinate.</param>
        /// <param name="w2">The upper width.</param>
        public void AddTrapezoidQuad(Vector2 p1, float w1, Vector2 p2, float w2)
        {
            Vector2 bottomLeft = new Vector2(p1.x - w1, p1.y);
            Vector2 topLeft = new Vector2(p2.x - w2, p2.y);
            Vector2 topRight = new Vector2(p2.x + w2, p2.y);
            Vector2 bottomRight = new Vector2(p1.x + w1, p1.y);

            AddQuad(bottomLeft, topLeft, topRight, bottomRight);
        }
    }
}