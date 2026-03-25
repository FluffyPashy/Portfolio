using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ProceduralTerrain
{
    /// <summary>
    /// Generates the mesh from the heightmap
    /// </summary>
    [BurstCompile]
    public struct MeshGenerator : IJobParallelFor
    {

        [NativeDisableParallelForRestriction] private NativeArray<Vector3> _verticies;
        [NativeDisableParallelForRestriction] private NativeArray<int> _triangleIndicies;
        private TerrainMeshVariables _meshVariables;
        private Maps _maps;
        private Color _steepTint;

        public MeshGenerator(TerrainMeshVariables mv, Maps m, Color steepTint)
        {
            _meshVariables = mv;

            _verticies = new NativeArray<Vector3>(_meshVariables.TotalVerts, Allocator.TempJob);
            _triangleIndicies = new NativeArray<int>(_meshVariables.TotalTriangles, Allocator.TempJob);
            _maps = m;
            _steepTint = steepTint;
        }

        public void Execute(int threadIndex)
        {
            int x = threadIndex / _meshVariables.TerrainMeshDetail;
            int y = threadIndex % _meshVariables.TerrainMeshDetail;

            // c - - - - d
            // |         |
            // |         |
            // |         |
            // a - - - - b
            // a is bottom left and the rest of the points are calculated using the index of a
            // we are only looping through each square to calculate the triangle and other bs

            int a = threadIndex + Mathf.FloorToInt(threadIndex / (float)_meshVariables.TerrainMeshDetail);
            int b = a + 1;
            int c = b + _meshVariables.TerrainMeshDetail;
            int d = c + 1;

            _verticies[a] = new Vector3(x + 0, _maps.heightMap[a], y + 0) * _meshVariables.TileEdgeLength;
            _verticies[b] = new Vector3(x + 0, _maps.heightMap[b], y + 1) * _meshVariables.TileEdgeLength;
            _verticies[c] = new Vector3(x + 1, _maps.heightMap[c], y + 0) * _meshVariables.TileEdgeLength;
            _verticies[d] = new Vector3(x + 1, _maps.heightMap[d], y + 1) * _meshVariables.TileEdgeLength;

            _triangleIndicies[threadIndex * 6 + 0] = a;
            _triangleIndicies[threadIndex * 6 + 1] = b;
            _triangleIndicies[threadIndex * 6 + 2] = c;
            _triangleIndicies[threadIndex * 6 + 3] = b;
            _triangleIndicies[threadIndex * 6 + 4] = d;
            _triangleIndicies[threadIndex * 6 + 5] = c;
        }

        public Mesh DisposeAndGetMesh()
        {
            // create and assign values to mesh
            var m = new Mesh();

            m.SetVertices(_verticies);
            m.SetColors(_maps.colorMap);
            m.triangles = _triangleIndicies.ToArray();

            m.RecalculateNormals();

            Vector3[] normals = m.normals;
            Color[] colors = _maps.colorMap.ToArray();

            for (int i = 0; i < colors.Length && i < normals.Length && i < _verticies.Length; i++)
            {
                float slope = Vector3.Angle(normals[i], Vector3.up);
                float slopeBlend = Mathf.InverseLerp(25f, 65f, slope);
                float heightBlend = Mathf.InverseLerp(_meshVariables.Height * 0.25f, _meshVariables.Height, _verticies[i].y);
                float rockyBlend = Mathf.Clamp01(slopeBlend * Mathf.Lerp(0.65f, 1f, heightBlend));

                colors[i] = Color.Lerp(colors[i], _steepTint, rockyBlend);
            }

            m.colors = colors;

            // Away with the memory hoarding!! (dispose the native arrays from memory)
            _verticies.Dispose();
            _triangleIndicies.Dispose();
            _maps.Dispose();

            return m;
        }
    }
}
