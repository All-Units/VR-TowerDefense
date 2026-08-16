using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR


[CustomEditor(typeof(MakeAllConvex))]
class MakeAllConvexEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Make All Convex"))
        {
           var self = (MakeAllConvex)target;
           self.makeAllConvex();
        }
       
        base.OnInspectorGUI();
    }
}
#endif
public class MakeAllConvex : MonoBehaviour
{
    #if UNITY_EDITOR

    public void makeAllConvex()
    {
        foreach (Transform c in GetComponent<Transform>().GetAllDescendants()){
            var mc = c.GetComponent<MeshCollider>();
            if (mc != null)
            {
                if (mc.convex == false){
                    EditorUtility.SetDirty(mc);
                    Undo.RecordObject(mc, $"Set {mc.name} to convex");
                }
                mc.convex = true;
            }
        }

    }

    #endif
   
}