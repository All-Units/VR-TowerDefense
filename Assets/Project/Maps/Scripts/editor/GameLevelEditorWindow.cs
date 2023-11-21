using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

public class GameLevelEditorWindow : EditorWindow
{
    private string customFolderPath = "Assets/Project/Maps/SO"; // Specify your custom folder path here

    [MenuItem("Castle Tools/Game Level Editor %m")]
    public static void ShowWindow()
    {
        GetWindow<GameLevelEditorWindow>("Game Level Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("List of Game Levels:", EditorStyles.boldLabel);

        // Find all GameLevel_SO assets in the specified folder
        string[] guids = AssetDatabase.FindAssets("t:GameLevel_SO", new[] { customFolderPath });

        // Create a dictionary to group the assets by their folder names
        Dictionary<string, List<GameLevel_SO>> folderGroups = new Dictionary<string, List<GameLevel_SO>>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameLevel_SO gameLevel = AssetDatabase.LoadAssetAtPath<GameLevel_SO>(assetPath);

            if (gameLevel != null)
            {
                // Get the folder containing the asset
                string folderName = Path.GetDirectoryName(assetPath).Remove(0, customFolderPath.Length);

                // Create a new list for the folder if it doesn't exist in the dictionary
                if (!folderGroups.ContainsKey(folderName))
                {
                    folderGroups[folderName] = new List<GameLevel_SO>();
                }

                // Add the GameLevel_SO to the folder's list
                folderGroups[folderName].Add(gameLevel);
            }
        }

        // Iterate through the folder groups and display the items with headers
        foreach (var folderGroup in folderGroups)
        {
            GUILayout.Label(folderGroup.Key, EditorStyles.boldLabel);

            foreach (GameLevel_SO gameLevel in folderGroup.Value)
            {
                if (GUILayout.Button(gameLevel.levelTitle))
                {
                    // Check if the SceneAsset is assigned
                    if (gameLevel.scene != null)
                    {
                        string scenePath = AssetDatabase.GetAssetPath(gameLevel.scene);
                        if (!string.IsNullOrEmpty(scenePath))
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                // Load the selected scene
                                EditorSceneManager.OpenScene(scenePath);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("The selected Scene Asset is not a valid scene.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("No Scene Asset assigned for this Game Level.");
                    }
                }
            }
        }
    }
}
#endif
