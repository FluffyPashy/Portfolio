using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace ProceduralTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainMeshGenerator : MonoBehaviour
    {
        public static TerrainMeshGenerator Instance;
        private const int GradientSampleCount = 100;

        private MeshFilter MeshFilter { get { return GetComponent<MeshFilter>(); } }
        public TerrainMeshVariables meshVariables;
        public TerrainHeightmapVariables heightmapVariables;
        [SerializeField] public Gradient HeightmapGradient;
        [SerializeField] private Color steepTint = Color.black;
        public PrefabPlacementVariables prefabPlacementVariables;
        /* public TerrainType[] terrainTypes; */

        private void Reset()
        {
            meshVariables = TerrainMeshVariables.Default();
            heightmapVariables = TerrainHeightmapVariables.Default();
            prefabPlacementVariables = PrefabPlacementVariables.Default();
            steepTint = Color.black;
            HeightmapGradient = CreateDefaultHeightGradient();
        }

        private static Gradient CreateDefaultHeightGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.25f, 0.12f, 0.02f), 0f),
                    new GradientColorKey(new Color(0.64f, 0.47f, 0.16f), 0.22f),
                    new GradientColorKey(new Color(0.91f, 0.85f, 0.61f), 0.58f),
                    new GradientColorKey(new Color(0.20f, 0.52f, 0.14f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                });
            return gradient;
        }


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void OnValidate()
        {
            GenerateMap();
        }

        private void Start()
        {
            PlacePrefabs();
        }


        /// <summary>
        /// Generates the mesh and applies it to the mesh filter
        /// </summary>
        public void GenerateMap()
        {
            NativeArray<Color> _gradientColorArray = new NativeArray<Color>(GradientSampleCount, Allocator.Persistent);

            for (int i = 0; i < GradientSampleCount; i++)
            {
                float t = i / (float)(GradientSampleCount - 1);

                // Fallback palette ensures color output even when no gradient is configured.
                _gradientColorArray[i] = HeightmapGradient != null ? HeightmapGradient.Evaluate(t) : Color.Lerp(new Color(0.12f, 0.45f, 0.16f), new Color(0.9f, 0.9f, 0.9f), t);
            }

            HeightMapGenerator heightmapGenerator = new HeightMapGenerator(meshVariables, heightmapVariables, _gradientColorArray);
            heightmapGenerator.Schedule(meshVariables.TotalVerts, 10000).Complete();
            Maps _maps = heightmapGenerator.ReturnAndDispose();

            MeshGenerator meshGenerator = new MeshGenerator(meshVariables, _maps, steepTint);
            meshGenerator.Schedule(meshVariables.TerrainMeshDetail * meshVariables.TerrainMeshDetail, 10000).Complete();

            MeshFilter.mesh = meshGenerator.DisposeAndGetMesh();

            _gradientColorArray.Dispose();
        }

        public int PlacePrefabs()
        {
            Mesh mesh = MeshFilter.mesh;
            if (mesh == null || prefabPlacementVariables.Rules == null || prefabPlacementVariables.Rules.Length == 0)
            {
                return 0;
            }

            if (prefabPlacementVariables.ClearExistingBeforePlacement)
            {
                ClearPreviouslyPlacedPrefabs();
            }

            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            int triangleCount = triangles.Length / 3;
            if (triangleCount == 0 || vertices.Length == 0)
            {
                return 0;
            }

            float spawnMultiplier = prefabPlacementVariables.GlobalSpawnMultiplier <= 0f ? 1f : prefabPlacementVariables.GlobalSpawnMultiplier;
            int maxPlacements = Mathf.Max(1, prefabPlacementVariables.MaxPrefabInstances);
            UnityEngine.Random.InitState(prefabPlacementVariables.RandomSeed);
            Transform parent = transform;
            int placedCount = 0;
            bool capReached = false;
            List<Vector3>[] placedPositionsPerRule = new List<Vector3>[prefabPlacementVariables.Rules.Length];

            for (int i = 0; i < placedPositionsPerRule.Length; i++)
            {
                placedPositionsPerRule[i] = new List<Vector3>();
            }

            for (int triangle = 0; triangle < triangleCount && !capReached; triangle++)
            {
                int i0 = triangles[triangle * 3 + 0];
                int i1 = triangles[triangle * 3 + 1];
                int i2 = triangles[triangle * 3 + 2];

                Vector3 worldV0 = transform.TransformPoint(vertices[i0]);
                Vector3 worldV1 = transform.TransformPoint(vertices[i1]);
                Vector3 worldV2 = transform.TransformPoint(vertices[i2]);

                Vector3 centroid = (worldV0 + worldV1 + worldV2) / 3f;
                Vector3 worldNormal = ((transform.TransformDirection(normals[i0]) + transform.TransformDirection(normals[i1]) + transform.TransformDirection(normals[i2])) / 3f).normalized;
                float slope = Vector3.Angle(worldNormal, Vector3.up);

                for (int r = 0; r < prefabPlacementVariables.Rules.Length; r++)
                {
                    PrefabPlacementRule rule = prefabPlacementVariables.Rules[r];
                    if (rule.Prefab == null)
                    {
                        continue;
                    }

                    if (centroid.y < rule.MinHeight || centroid.y > rule.MaxHeight)
                    {
                        continue;
                    }

                    if (slope < rule.MinSlope || slope > rule.MaxSlope)
                    {
                        continue;
                    }

                    if (UnityEngine.Random.value > EvaluateSpawnProbability(rule.SpawnChance, spawnMultiplier))
                    {
                        continue;
                    }

                    if (rule.MinDistance > 0f)
                    {
                        float minDistanceSqr = rule.MinDistance * rule.MinDistance;
                        List<Vector3> rulePositions = placedPositionsPerRule[r];
                        bool tooClose = false;

                        for (int p = 0; p < rulePositions.Count; p++)
                        {
                            if ((rulePositions[p] - centroid).sqrMagnitude < minDistanceSqr)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (tooClose)
                        {
                            continue;
                        }
                    }

                    float randomYaw = UnityEngine.Random.Range(-rule.RandomYaw, rule.RandomYaw);
                    Quaternion rotation = rule.AlignToTerrain
                        ? Quaternion.FromToRotation(Vector3.up, worldNormal) * Quaternion.Euler(0f, randomYaw, 0f)
                        : Quaternion.Euler(0f, randomYaw, 0f);
                    GameObject instance = Instantiate(rule.Prefab, centroid, rotation, parent);
                    instance.transform.localScale = new Vector3(
                        UnityEngine.Random.Range(rule.MinScale.x, rule.MaxScale.x),
                        UnityEngine.Random.Range(rule.MinScale.y, rule.MaxScale.y),
                        UnityEngine.Random.Range(rule.MinScale.z, rule.MaxScale.z)
                    );
                    instance.name = $"{rule.Prefab.name}_Placed";
                    placedPositionsPerRule[r].Add(centroid);

                    placedCount++;
                    if (placedCount >= maxPlacements)
                    {
                        capReached = true;
                        break;
                    }
                }
            }

            if (capReached)
            {
                Debug.LogWarning($"Prefab placement stopped at Max Prefab Instances limit ({maxPlacements}). Increase the limit in Prefab Placement Variables if needed.", this);
            }

            return placedCount;
        }

        private static float EvaluateSpawnProbability(float spawnChance, float spawnMultiplier)
        {
            // Keep old 0..1 values working as direct probabilities, but allow a friendlier
            // 0..100 density scale with finer low-end control for new values.
            if (spawnChance <= 1f)
            {
                return Mathf.Clamp01(spawnChance * spawnMultiplier);
            }

            float normalizedDensity = Mathf.Clamp01(spawnChance / 100f);
            return Mathf.Clamp01(normalizedDensity * normalizedDensity * spawnMultiplier);
        }

        [ContextMenu("Rebuild Terrain + Prefabs")]
        public void RebuildTerrainAndPrefabs()
        {
            GenerateMap();
            PlacePrefabs();
        }

        [ContextMenu("Clear Placed Prefabs")]
        public void ClearPreviouslyPlacedPrefabs()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (!child.name.EndsWith("_Placed", StringComparison.Ordinal))
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        /*         
                TerrainType GetTerrainType(float height)
                {
                    foreach (TerrainType terrainType in terrainTypes)
                    {
                        if (height < terrainType.Height) return terrainType;
                    }

                    return terrainTypes[0];
                } */
    }

    public struct Maps
    {
        [NativeDisableParallelForRestriction] public NativeArray<float> heightMap;
        [NativeDisableParallelForRestriction] public NativeArray<Color> colorMap;

        public Maps(NativeArray<float> h, NativeArray<Color> c)
        {
            heightMap = h;
            colorMap = c;
        }

        public void Dispose()
        {
            colorMap.Dispose();
            heightMap.Dispose();
        }
    }

    [Serializable]
    public struct TerrainMeshVariables
    {
        [field: SerializeField, Range(16, 255)] public int TerrainMeshDetail { get; private set; }
        [field: SerializeField, Range(50f, 2000f)] public float TerrainWidth { get; private set; }
        [field: SerializeField, Range(1f, 100f)] public float Height { get; private set; }

        public int TotalVerts { get { return totalVerts; } private set { TotalVerts = value; } }
        public int TotalTriangles { get { return totalTriangles; } private set { TotalTriangles = value; } }
        public float TileEdgeLength { get { return tileEdgeLength; } private set { TileEdgeLength = value; } }

        public TerrainMeshVariables(int terrainMeshDetail, float terrainWidth, float height)
        {
            TerrainMeshDetail = terrainMeshDetail;
            TerrainWidth = terrainWidth;
            Height = height;
        }

        public static TerrainMeshVariables Default() => new TerrainMeshVariables(255, 1000f, 12f);

        private int totalVerts => (TerrainMeshDetail + 1) * (TerrainMeshDetail + 1);
        private int totalTriangles => TerrainMeshDetail * TerrainMeshDetail * 6;
        private float tileEdgeLength => TerrainWidth / TerrainMeshDetail;
    }

    [Serializable]
    public struct TerrainHeightmapVariables
    {
        [field: SerializeField, Header("Noise"), Range(0.1f, 5f)] public float NoiseScale { get; private set; }
        [field: SerializeField, Range(0.5f, 3f)] public float Frequency { get; private set; }
        [field: SerializeField, Range(0.25f, 2f)] public float Lacunarity { get; private set; }
        [field: SerializeField, Range(1, 8)] public int Octaves { get; private set; }
        [field: SerializeField, Range(0f, 5f)] public float Weight { get; private set; }

        [field: SerializeField, Range(0.25f, 4f)] public float FalloffSteepness { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float FalloffOffset { get; private set; }

        public TerrainHeightmapVariables(float noiseScale, float frequency, float lacunarity, int octaves, float weight, float falloffSteepness, float falloffOffset)
        {
            NoiseScale = noiseScale;
            Frequency = frequency;
            Lacunarity = lacunarity;
            Octaves = octaves;
            Weight = weight;
            FalloffSteepness = falloffSteepness;
            FalloffOffset = falloffOffset;
        }

        public static TerrainHeightmapVariables Default() => new TerrainHeightmapVariables(0.5f, 1.5f, 0.5f, 4, 3.29f, 1.57f, 0.5f);

        /*         [field: SerializeField, Header("Extra")] public float waterLevel; */
    }

    [Serializable]
    public struct PrefabPlacementVariables
    {
        [field: SerializeField] public bool ClearExistingBeforePlacement { get; private set; }
        [field: SerializeField, Range(0f, 2f)] public float GlobalSpawnMultiplier { get; private set; }
        [field: SerializeField, Range(100, 50000)] public int MaxPrefabInstances { get; private set; }
        [field: SerializeField] public int RandomSeed { get; private set; }
        [field: SerializeField] public PrefabPlacementRule[] Rules { get; private set; }

        public static PrefabPlacementVariables Default() => new PrefabPlacementVariables
        {
            ClearExistingBeforePlacement = true,
            GlobalSpawnMultiplier = 1f,
            MaxPrefabInstances = 3000,
            RandomSeed = 1337,
            Rules = new[] { PrefabPlacementRule.GrassExample() }
        };
    }

    [Serializable]
    public struct PrefabPlacementRule
    {
        [field: SerializeField] public string Label { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField, Range(0f, 100f), Tooltip("Values above 1 use an easier percent-style density scale. Old 0..1 values still work as legacy direct probabilities.")] public float SpawnChance { get; private set; }
        [field: SerializeField, Range(0f, 90f)] public float MinSlope { get; private set; }
        [field: SerializeField, Range(0f, 90f)] public float MaxSlope { get; private set; }
        [field: SerializeField, Range(-50f, 1000f)] public float MinHeight { get; private set; }
        [field: SerializeField, Range(-50f, 1000f)] public float MaxHeight { get; private set; }
        [field: SerializeField] public Vector3 MinScale { get; private set; }
        [field: SerializeField] public Vector3 MaxScale { get; private set; }
        [field: SerializeField, Range(0f, 180f)] public float RandomYaw { get; private set; }
        [field: SerializeField, Range(0f, 25f)] public float MinDistance { get; private set; }
        [field: SerializeField] public bool AlignToTerrain { get; private set; }

        public static PrefabPlacementRule GrassExample() => new PrefabPlacementRule
        {
            Label = "Grass Example",
            Prefab = null,
            SpawnChance = 8f,
            MinSlope = 0f,
            MaxSlope = 35f,
            MinHeight = -5f,
            MaxHeight = 200f,
            MinScale = new Vector3(0.85f, 0.85f, 0.85f),
            MaxScale = new Vector3(1.2f, 1.2f, 1.2f),
            RandomYaw = 180f,
            MinDistance = 1.5f,
            AlignToTerrain = true
        };
    }

    [System.Serializable]
    public struct TerrainType
    {
        [field: SerializeField] public string BiomeName { get; private set; }
        [field: SerializeField] public float Height { get; private set; }
        [field: SerializeField] public Color BiomeTint { get; private set; }
        [field: SerializeField] public Texture2D Albedo { get; private set; }
    }
}