using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class FolderStrucGen : EditorWindow {

    private int status = 0;
    private int missingf = 0;
    bool showFoldout = false;
    private string missingfs = "Missing folders..";

    private int d_exists;
    private int p_exists;

    [MenuItem("Tools/Custom/FolderStrucGen")]
    private static void ShowWindow() {
        var window = GetWindow<FolderStrucGen>();
        window.titleContent = new GUIContent("FolderStrucGen");
        window.Show();
    }

    //get ProjectName
    public string GetProjectName() {
        string[] s = Application.dataPath.Split('/');
        string projectName = s[s.Length - 2];
        return projectName;
    }

    private void OnGUI() {

        //caching projectName
        string projectName = GetProjectName();

        string[] path = {
            "Assets/3rd-Party",
            "Assets/Materials",
            "Assets/PhysicsMaterials",
            "Assets/Scripts",
            "Assets/_" + projectName + "/_Prototype",
            "Assets/_" + projectName + "/Actors",
            "Assets/_" + projectName + "/Enviroment",
            "Assets/_" + projectName + "/Items",
            "Assets/_" + projectName + "/Scripts",
            "Assets/_" + projectName + "/StreamingAssets",
            "Assets/_" + projectName + "/Systems",
        };


        ////////////////////////////////////////////////////////////////////////
        // VAR CHECKER START
        ////////////////////////////////////////////////////////////////////////

        //check for incomplete folder struc
        for (int i = 0; i < path.Length; i++) {
            if (!Directory.Exists(path[i])) {
                missingf = 1;
                status = 1;
            }
        }
        
        //add int+1 to var, for each existing folder - needed for comparison
        for (int i = 0; i < path.Length; i++) {
            if (Directory.Exists(path[i])) {
                d_exists -= 1;
            }
        }
        //count paths defindend in FolderStrucGen
        p_exists = path.Count(); // int 11

        //compare folder exists return : path count
        if (p_exists + d_exists == 11) {
            status = 0;
        }
        ////////////////////////////////////////////////////////////////////////
        // VAR CHECKER End
        ////////////////////////////////////////////////////////////////////////


        //folder struc exists
        if (status == -1) {
            EditorGUILayout.HelpBox("All set, nothing to do here!", MessageType.Info);
        }


        //first run
        if (status == 0) {
            EditorGUILayout.HelpBox("Create a basic project folder structure with one click", MessageType.Info);
        }


        //visual warning for missing/incomplete 
        if (missingf == 1) {
            EditorGUILayout.HelpBox("Following folders are missing and will be created:", MessageType.Warning);
            
            //display missing folders
            showFoldout = EditorGUILayout.Foldout(showFoldout, missingfs);
            if (showFoldout) {
                for (int i = 0; i < path.Length; i++) {
                    GUI.enabled = false;
                    if(!Directory.Exists(path[i])) {
                        EditorGUILayout.LabelField(path[i]);
                    }
                }
            }
            GUI.enabled = true;
        }

        //create folder struc
        if (status != -1) {
            if (GUILayout.Button("Create")) {
                for (int i = 0; i < path.Length; i++) {
                    if(!Directory.Exists(path[i])) {
                        Directory.CreateDirectory(path[i]);
                    }
                }
                status = -1;
                missingf = 0;
                AssetDatabase.Refresh();
            }
        }
    }
}