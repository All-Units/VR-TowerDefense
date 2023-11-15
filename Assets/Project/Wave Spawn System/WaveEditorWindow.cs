#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class WaveEditorWindow : EditorWindow
{
    [MenuItem("Castle Tools/Wave Editor %#w")]
    public static void OpenWindow()
    {
        Debug.Log("Opening window");
        var window = GetWindow<WaveEditorWindow>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label($"Wave Editor Window");
    }
}
#endif