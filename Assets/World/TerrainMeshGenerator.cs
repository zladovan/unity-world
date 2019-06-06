using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public static class TerrainMeshGenerator
    {
        public static MeshBuilder GenerateTerrainMesh(HeightMap heightMap, int levelOfDetail)
        {
            var width = heightMap.Width;
            var height = heightMap.Height;
            var topLeft = new Vector3((width - 1) / -2f, 0, (height - 1) / 2f); // shifting to make position in center
            var step = levelOfDetail <= 0 ? 1 : levelOfDetail * 2;
            var verticesPerLine = (width - 1) / step + 1;
            var meshBuilder = new MeshBuilder(width * height / step, (width - 1) * (height - 1) * 2 / step);

            for (int y = 0; y < height; y += step)
            {
                for (int x = 0; x < width; x += step)
                {
                    //Debug.Log(string.Format("Add vertext [{0}, {1}, {2}]", topLeft.x + x, heightMap[x,y], topLeft.z - y));
                    meshBuilder.AddVertex(topLeft.x + x, heightMap[x,y], topLeft.z - y);
                    meshBuilder.AddUV(x / (float)width, y / (float)height);
                    if (x < width - 1 && y < height - 1)
                    {
                        var index = meshBuilder.VertexIndex - 1;
                        meshBuilder.AddQuad(index, index + 1, index + verticesPerLine, index + verticesPerLine + 1);
                    }
                }
            }

            return meshBuilder;
        }

        public static MeshBuilder GenerateFlatTerrainMesh(HeightMap heightMap, int levelOfDetail)
        {
            var width = heightMap.Width;
            var height = heightMap.Height;
            var topLeft = new Vector3((width - 1) / -2f, 0, (height - 1) / 2f); // shifting to make position in center
            var step = levelOfDetail <= 0 ? 1 : levelOfDetail * 2;
            var trianglesCount = (width - 1) * (height - 1) * 2 / step;
            var verticesCount = trianglesCount * 3;
            var meshBuilder = new MeshBuilder(verticesCount, trianglesCount);

            // To achieve flat effect it is needed to not share vertices between triangles,
            // therefor each triangle has three standalone vertices
            for (int y = 0; y < height - 1; y+=step)
            {
                for (int x = 0; x < width - 1; x+=step)
                {
                    /*
                     * Vertices
                     *
                     *  a---b
                     *  |   |
                     *  c---d
                     */
                    var a = new Vector3(topLeft.x + x, heightMap[x, y], topLeft.z - y);
                    var b = new Vector3(topLeft.x + x + step, heightMap[x + step, y], topLeft.z - y);
                    var c = new Vector3(topLeft.x + x, heightMap[x, y + step], topLeft.z - (y + step));
                    var d = new Vector3(topLeft.x + x + step, heightMap[x + step, y + step], topLeft.z - (y + step));
                    
                    /*
                     * Texture coordinates 
                     *
                     *  ua--ub
                     *  |    |
                     *  uc--ud
                     */
                    var ua = new Vector2(x / (float)width, y / (float)height);
                    var ub = new Vector2((x + step) / (float)width, y / (float)height);
                    var uc = new Vector2(x / (float)width, (y + step) / (float)height);
                    var ud = new Vector2((x + step) / (float)width, (y + step) / (float)height);

                    // Add triangle a, d, c 
                    meshBuilder.AddVertex(a, ua);
                    meshBuilder.AddVertex(d, ud);
                    meshBuilder.AddVertex(c, uc);
                    meshBuilder.AddTriangle(meshBuilder.VertexIndex - 3, meshBuilder.VertexIndex - 2, meshBuilder.VertexIndex - 1);

                    // Add triangle d, a, b
                    meshBuilder.AddVertex(d, ud);
                    meshBuilder.AddVertex(a, ua);
                    meshBuilder.AddVertex(b, ub);
                    meshBuilder.AddTriangle(meshBuilder.VertexIndex - 3, meshBuilder.VertexIndex - 2, meshBuilder.VertexIndex - 1);
                }
            }

            return meshBuilder;
        }
    }

    public class MeshBuilder
    {
        private Vector3[] vertices;
        private int[] triangles;
        public Vector2[] uvs;
        private int vertexIndex = 0;
        private int uvIndex = 0;
        private int triangleIndex = 0;    

        public MeshBuilder(int verticesCount, int trianglesCount) 
        {
            vertices = new Vector3[verticesCount];
            uvs = new Vector2[verticesCount];
            triangles = new int[trianglesCount * 3];
        }   

        public void AddVertex(float x, float y, float z)
        {
            vertices[vertexIndex++] = new Vector3(x, y, z);
        }

        public void AddVertex(Vector3 vertex)
        {
            vertices[vertexIndex++] = vertex;
        }

        public void AddVertex(Vector3 vertex, Vector2 uv)
        {
            AddVertex(vertex);
            AddUV(uv);
        }

        public void DuplicateLastVertex()
        {
            Vector3 lastVertex =  vertices[vertexIndex - 1];
            vertices[vertexIndex++] = new Vector3(lastVertex.x, lastVertex.y, lastVertex.z);
        }

        public void AddTriangle(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c; 
            triangleIndex += 3;
        }

        public void AddQuad(int a, int b, int c, int d)
        {
            AddTriangle(a, d, c);
            AddTriangle(d, a, b);
        }

        public void AddUV(float u, float v)
        {   
            uvs[uvIndex++] = new Vector2(u, v);
        }

        public void AddUV(Vector2 uv)
        {   
            uvs[uvIndex++] = uv;
        }

        public Mesh Build()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            return mesh;
        }

        public int VertexIndex 
        {
            get { return vertexIndex; }
        }

    }

}