using System;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ProceduralTerrain
{
    /// <summary>
    /// Generates the heightmap and color map
    /// </summary>
    [BurstCompile]
    public struct HeightMapGenerator : IJobParallelFor
    {

        [NativeDisableParallelForRestriction] private NativeArray<float> _heightMap;
        [NativeDisableParallelForRestriction] private NativeArray<Color> _colMap;
        private readonly TerrainMeshVariables _meshVariables;
        private readonly TerrainHeightmapVariables _heightmapVariables;
        [NativeDisableParallelForRestriction] private NativeArray<Color> _gradient;


        public HeightMapGenerator(TerrainMeshVariables mv, TerrainHeightmapVariables hv, NativeArray<Color> grad)
        {
            _meshVariables = mv;
            _heightmapVariables = hv;
            _colMap = new NativeArray<Color>(_meshVariables.TotalVerts, Allocator.TempJob);
            _heightMap = new NativeArray<float>(_meshVariables.TotalVerts, Allocator.TempJob);
            _gradient = grad;
        }


        /// <summary>
        /// Executes the job multithreaded
        /// </summary>
        /// <param name="threadIndex"></param>
        public void Execute(int threadIndex)
        {
            float x = threadIndex / /*(float)*/(_meshVariables.TerrainMeshDetail + 1);
            float y = threadIndex % (_meshVariables.TerrainMeshDetail + 1);
            float2 pos = new float2(x, y);

            float h = Mathf.Clamp((OctavedSimplexNoise(pos) + OctavedRidgeNoise(pos)) / 2f * FalloffMap(pos) * _meshVariables.Height, 0, 1000);
            float normalizedHeight = Mathf.Clamp01(h / Mathf.Max(_meshVariables.Height, 0.0001f));
            int gradientIndex = Mathf.Clamp(Mathf.RoundToInt(normalizedHeight * (_gradient.Length - 1)), 0, _gradient.Length - 1);

            _heightMap[threadIndex] = h / _meshVariables.TileEdgeLength;
            _colMap[threadIndex] = _gradient[gradientIndex];
        }

        public Maps ReturnAndDispose() => new Maps(_heightMap, _colMap);

        float OctavedRidgeNoise(float2 pos)
        {
            float noiseVal = 0, amplitude = 1, freq = _heightmapVariables.NoiseScale, weight = 1;

            for (int o = 0; o < _heightmapVariables.Octaves; o++)
            {
                float v = 1 - Mathf.Abs(noise.snoise(pos / freq / _meshVariables.TerrainMeshDetail));
                v *= v;
                v *= weight;
                weight = Mathf.Clamp01(v * _heightmapVariables.Weight);
                noiseVal += v * amplitude;

                freq /= _heightmapVariables.Frequency;
                amplitude /= _heightmapVariables.Lacunarity;
            }

            return noiseVal;
        }
        float OctavedSimplexNoise(float2 pos)
        {
            float noiseVal = 0, amplitude = 1, freq = _heightmapVariables.NoiseScale;

            for (int o = 0; o < _heightmapVariables.Octaves; o++)
            {
                float v = (noise.snoise(pos / freq / _meshVariables.TerrainMeshDetail) + 1) / 2f;
                noiseVal += v * amplitude;

                freq /= _heightmapVariables.Frequency;
                amplitude /= _heightmapVariables.Lacunarity;
            }

            return noiseVal;
        }
        float FalloffMap(float2 pos)
        {
            float x = (pos.x / (_meshVariables.TerrainMeshDetail + 1)) * 2 - 1;
            float y = (pos.y / (_meshVariables.TerrainMeshDetail + 1)) * 2 - 1;

            float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

            float a = _heightmapVariables.FalloffSteepness;
            float b = _heightmapVariables.FalloffOffset;

            return 1 - (Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow((b - b * value), a)));
        }
    }
}