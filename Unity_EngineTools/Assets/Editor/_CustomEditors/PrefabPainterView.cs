using UnityEditor;
using UnityEngine;

namespace PrefabPainter
{
    [ExecuteInEditMode]
    public class PrefabPainterView : EditorWindow
    {
        private PrefabPainterViewModel viewModel = new PrefabPainterViewModel();

        [MenuItem("Tools/Prefab Painter")]
        private static void Init()
        {
            PrefabPainterView window = (PrefabPainterView)GetWindow(typeof(PrefabPainterView));
            window.titleContent = new GUIContent("Prefab Painter");
            window.Show();
            window.Focus();
            window.Repaint();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += SceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= SceneGUI;
        }

        private void OnGUI()
        {
            //Show the prefablist
            EditorGUILayout.LabelField("Prefab List", EditorStyles.boldLabel);
            ShowPrefabList();

            //Parent Name
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Parent Settings", EditorStyles.boldLabel);
            viewModel.model.parentName = EditorGUILayout.TextField("Parent Name", viewModel.model.parentName);

            //Parent Transform
            viewModel.model.parentTransform = EditorGUILayout.ObjectField("Parent Transform", viewModel.model.parentTransform, typeof(Transform), true) as Transform;
            if (viewModel.model.parentTransform == null)
            {
                if (GUILayout.Button("Create Parent"))
                {
                    viewModel.CreateParentPrefab();
                }
            }

            //Brush settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Brush Settings", EditorStyles.boldLabel);
            viewModel.model.brushSize = EditorGUILayout.FloatField("Brush Size", viewModel.model.brushSize);
            viewModel.model.layerMask = EditorGUILayout.MaskField("Layer Mask", viewModel.model.layerMask, UnityEditorInternal.InternalEditorUtility.layers);
            viewModel.model.amount = EditorGUILayout.IntField("Amount", viewModel.model.amount);
            viewModel.model.minDistance = EditorGUILayout.Slider("Min Distance", viewModel.model.minDistance, 0f, 250f);
            viewModel.model.randomScalemin = EditorGUILayout.Vector3Field("Random Scale Min", viewModel.model.randomScalemin);
            viewModel.model.randomScalemax = EditorGUILayout.Vector3Field("Random Scale Max", viewModel.model.randomScalemax);
            viewModel.model.randomRotationRange = EditorGUILayout.Vector3Field("Random Rotation Range", viewModel.model.randomRotationRange);

            //Paint toggle button
            EditorGUILayout.Space(5);
            if (viewModel.model.paint)
            {
                GUI.color = Color.green;
                if (GUILayout.Button("Painting Enabled\nLeftCtrl + RightClick to Paint", GUILayout.Height(30)))
                {
                    viewModel.model.paint = !viewModel.model.paint;
                }
            }
            else
            {
                GUI.color = Color.red;
                if (GUILayout.Button("Painting Disabled", GUILayout.Height(30)))
                {
                    viewModel.model.paint = !viewModel.model.paint;
                }
            }

        }

        /// <summary>
        /// Handles the scene GUI events to be able to paint the prefabs
        /// </summary>
        /// <param name="sceneView"></param> <summary>
        private void SceneGUI(SceneView sceneView)
        {
            Event current = Event.current;

            //if paint is enabled, paint the prefab
            if (viewModel.model.paint)
            {
                if (current.type == EventType.MouseDown && current.control || current.type == EventType.MouseDrag && current.control)
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        viewModel.PaintPrefabs(hit.point);
                    }
                }
            }


            //if a object gets removed from the scene, remove it from the list
            if (current.type == EventType.Layout)
            {
                viewModel.RemoveMissingPrefabs();
            }

            //draw the brush
            if (viewModel.model.paint) DrawGizmoBrush(current.mousePosition);
        }

        /// <summary>
        /// Draws the brush in the scene view using the Event.current.mousePosition
        /// from the SceneGUI method
        /// </summary>
        /// <param name="mousePosition"></param>
        private void DrawGizmoBrush(Vector3 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                //Translucent brush
                Handles.color = new Color(0f, 1f, 0f, 0.1f);
                Handles.DrawSolidDisc(hit.point, hit.normal, viewModel.model.brushSize);
            }
        }

        //Show the prefablist
        void ShowPrefabList()
        {
            //Show an "add button" if no prefabs are     in the list
            if (viewModel.model.prefabList.Count == 0)
            {
                //Show warning if no prefabs are in the list
                EditorGUILayout.HelpBox("No Prefabs in List", MessageType.Warning);

                //Add Prefab Button
                if (GUILayout.Button("Add Prefabs"))
                {
                    viewModel.model.prefabList.Add(null);
                }
            }
            else //Show the prefabs in the list
            {
                for (int i = 0; i < viewModel.model.prefabList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    viewModel.model.prefabList[i] = EditorGUILayout.ObjectField(viewModel.model.prefabList[i], typeof(GameObject), false) as GameObject;
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        viewModel.model.prefabList.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                //Add Prefab Button
                if (GUILayout.Button("Add Prefabs"))
                {
                    viewModel.model.prefabList.Add(null);
                }
            }
        }
    }
}