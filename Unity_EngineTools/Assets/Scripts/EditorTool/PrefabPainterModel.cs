using System.Collections.Generic;
using UnityEngine;

namespace PrefabPainter
{
    [System.Serializable]
    public class PrefabPainterModel
    {
        //Tool settings
        public List<GameObject> prefabList { get; set;} = new List<GameObject>();
        public string parentName { get; set;} = "Painted Prefabs";
        public Transform parentTransform { get; set;} = null;
        public LayerMask layerMask { get; set;} = 0;
        public float brushSize { get; set;} = 1f;
        public int amount { get; set;} = 10;
        public float minDistance { get; set;} = 0.1f;
        public Vector3 randomRotationRange { get; set;} = new Vector3(0f, 360f, 0f);
        public Vector3 randomScalemin { get; set;} = new Vector3(1f, 1f, 1f);
        public Vector3 randomScalemax { get; set;} = new Vector3(1f, 1f, 1f);
        public bool paint { get; set;} = false;

        //Painting
        public List<GameObject> paintedPrefabs { get; set;} = new List<GameObject>();
    }
}