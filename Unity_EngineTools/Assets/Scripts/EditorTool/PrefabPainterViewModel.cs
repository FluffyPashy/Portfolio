using UnityEditor;
using UnityEngine;

namespace PrefabPainter
{
    public class PrefabPainterViewModel
    {
        public PrefabPainterModel model = new PrefabPainterModel();

        public PrefabPainterModel PrefabPainterModel
        {
            get => default;
            set
            {
            }
        }

        //Makes sure the painter is ready to paint
        private bool ValidatePainter()
        {
            //Check if the prefab list is empty or no prefab assigned
            if (model.prefabList.Count == 0)
            {
                Debug.LogError("Prefab Painter: No Prefabs assigned to the Prefab List");
                return false;
            }
            else if (model.prefabList.Contains(null))
            {
                for (int i = 0; i < model.prefabList.Count; i++)
                {
                    if (model.prefabList[i] == null)
                    {
                        //remove the null prefab from the list
                        model.prefabList.RemoveAt(i);
                    }
                }

                if (model.prefabList.Count == 0)
                {
                    Debug.LogError("Prefab Painter: No Prefabs assigned to the Prefab List");
                    return false;
                }
                else return true;
            }
            else if (model.parentTransform == null)
            {
                Debug.LogError("Prefab Painter: No Parent Transform assigned");

                //remove all last placed prefab
                for (int i = 0; i < model.paintedPrefabs.Count; i++)
                {
                    if (model.paintedPrefabs[i] != null)
                    {
                        Undo.DestroyObjectImmediate(model.paintedPrefabs[i]);
                    }
                }

                return false;
            }
            else return true;
        }


        //Create a Parent for the Prefabs to be painted in
        public void CreateParentPrefab()
        {
            GameObject parent = new GameObject(model.parentName);
            model.parentTransform = parent.transform;
        }


        //Paint Prefabs
        public void PaintPrefabs(Vector3 cursorPosition)
        {
            {
                if (!ValidatePainter())
                    return;

                int paintedCount = 0;

                //loop through the amount of prefabs to be painted
                for (int i = 0; i < model.amount; i++)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(-model.brushSize, model.brushSize), 0, Random.Range(-model.brushSize, model.brushSize));
                    Vector3 spawnPosition = cursorPosition + randomOffset;

                    //Raycast down to find the surface
                    if (Physics.Raycast(spawnPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, model.layerMask))
                    {
                        float distanceToNearest = GetDistanceToNearestPrefab(hit.point);
                        if (distanceToNearest > model.minDistance)
                        {
                            Quaternion randomRotation = Quaternion.Euler(Random.Range(-model.randomRotationRange.x, model.randomRotationRange.x),
                                                                        Random.Range(-model.randomRotationRange.y, model.randomRotationRange.y),
                                                                        Random.Range(-model.randomRotationRange.z, model.randomRotationRange.z));
                            Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                            Quaternion finalRotation = randomRotation * normalRotation;

                            // Randomize the scale between the min and max values
                            Vector3 randomScale = new Vector3(Random.Range(model.randomScalemin.x, model.randomScalemax.x),
                                                                Random.Range(model.randomScalemin.y, model.randomScalemax.y),
                                                                Random.Range(model.randomScalemin.z, model.randomScalemax.z));

                            GameObject selectedPrefab = model.prefabList[Random.Range(0, model.prefabList.Count)];
                            GameObject spawnedPrefab = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;

                            // Set the position, rotation and scale of the prefab
                            spawnedPrefab.transform.position = hit.point;
                            spawnedPrefab.transform.rotation = finalRotation;
                            spawnedPrefab.transform.localScale = randomScale;

                            // Set the parent transform if specified
                            if (model.parentTransform != null)
                                spawnedPrefab.transform.parent = model.parentTransform;

                            model.paintedPrefabs.Add(spawnedPrefab);
                            paintedCount++;

                            Undo.RegisterCreatedObjectUndo(spawnedPrefab, "Paint Prefab");
                        }
                    }
                }
            }
        }

        //Get the distance to the nearest prefab
        private float GetDistanceToNearestPrefab(Vector3 position)
        {
            float minDistance = float.MaxValue;
            foreach (GameObject paintedPrefab in model.paintedPrefabs)
            {
                float distance = Vector3.Distance(position, paintedPrefab.transform.position);
                if (distance < minDistance)
                    minDistance = distance;
            }
            return minDistance;
        }

        //Remove missing prefabs from the list
        public void RemoveMissingPrefabs()
        {
            for (int i = 0; i < model.paintedPrefabs.Count; i++)
            {
                if (model.paintedPrefabs[i] == null)
                {
                    model.paintedPrefabs.RemoveAt(i);
                }
            }
        }
    }
}