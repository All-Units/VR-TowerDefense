using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapTile)), CanEditMultipleObjects]
public class MapCreatorEditor : Editor
{
    private MeshRenderer mr;
    private MeshFilter mf;
    private static Dictionary<GameObject, Texture2D> _texture2Ds = new Dictionary<GameObject, Texture2D>();

    
    public override void OnInspectorGUI()
    {
        
        MapTile t = (MapTile)target;
        if (GUILayout.Button("EnMeshify..."))
            EnMeshify();
        base.OnInspectorGUI();
        if (t.tile_list == null)
            return;
        if (_texture2Ds.Count == 0)
            fillTextureDict(t.tile_list.tiles);
        foreach (var tile in _texture2Ds)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(tile.Value);
            if (GUILayout.Button(tile.Key.name))
            {
                if (mf == null)
                    mf = t.GetComponent<MeshFilter>();
                if (mr == null)
                    mr = t.GetComponent<MeshRenderer>();
                Undo.RecordObject(mf, "Changing model");
                mf.mesh = tile.Key.GetComponent<MeshFilter>().sharedMesh;
                Undo.RecordObject(mr, "Changing model");
                mr.materials = tile.Key.GetComponent<MeshRenderer>().sharedMaterials;
            }
            EditorGUILayout.EndHorizontal();
        }
        
       
    }

    void EnMeshify()
    {
        Transform parent = ((MapTile)target).transform.parent;
        int unmeshed = 0;
        foreach (Transform child in parent)
        {
            if (child.GetComponent<MeshCollider>() == null)
            {
                unmeshed++;
                MeshCollider c = child.gameObject.AddComponent<MeshCollider>();
                c.convex = true;
            }
        }
        Debug.Log($"Meshed {unmeshed} objs");
    }

    static void fillTextureDict(List<GameObject> tiles)
    {
        foreach (GameObject tile in tiles)
        {
            var editor = UnityEditor.Editor.CreateEditor(tile);
            string path = AssetDatabase.GetAssetPath(tile);
            Texture2D tex = editor.RenderStaticPreview(path, null, 150, 150);
            EditorWindow.DestroyImmediate(editor);
            _texture2Ds.Add(tile, tex);
        }
    }
    Vector3[] dirs = new[] { new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, -1f), new Vector3(-1f, 0f, 0f), 
        new Vector3(0f, 1f, 0f), new Vector3(0f, -1f, 0f),};
    protected virtual void OnSceneGUI()
    {
        MapTile t = (MapTile)target;
        
        Vector3 pos = t.transform.position;
        //Set our name to our position
        t.gameObject.name = ((Vector3)Vector3Int.RoundToInt(pos)).preciseVector3String();
        
        float y = 0f;
        Quaternion q;
        //Creates 4 spawn arrows, 1 in each direction
        foreach (Vector3 dir in dirs)
        {
            Vector3 mod = pos + (dir * 2);
            bool valid = !(t.transform.parent.Find(mod.preciseVector3IntString()) != null);
            Handles.color = valid ? Color.green : Color.red;
            q = Quaternion.Euler(0f, y, 0f);
            if (dir.y == 1)
                q = Quaternion.Euler(-90f, 0f, 0f);
            if (dir.y == -1)
                q = Quaternion.Euler(90f, 0f, 0f);
            y += 90f;
            if (Handles.Button(pos + dir, q, .5f, .5f, Handles.ConeHandleCap) && valid)
            {
                var selected = Selection.objects;
                HashSet<Object> newtiles = new HashSet<Object>();
                newtiles.Add(SpawnTileAt(t.gameObject, mod));
                if (selected.Length != 1)
                {
                    foreach (GameObject go in selected)
                    {
                        if (go == t.gameObject)
                            continue;
                        MapTile mt = go.GetComponent<MapTile>();
                        if (mt != null)
                        {
                            Vector3 mod2 = mt.transform.position + (dir * 2);
                            bool v = !(t.transform.parent.Find(mod2.preciseVector3IntString()) != null);
                            if (v)
                                newtiles.Add(SpawnTileAt(mt.gameObject, mod2));
                        }
                    }
                    Selection.objects = newtiles.ToArray();
                }
                
            }
        }
        CreateMoveArrows(pos);
        
        //Creates the rotate 90 snap buttons
        var dirs2 = new[]
        {
            new Vector3(.5f, 0f, -1f),
            new Vector3(-.5f, 0f, -1f),
        };
        y = 90f;
        q = Quaternion.Euler(0f, y, 0f);
        Handles.color = Color.cyan;
        if (Handles.Button(pos + dirs2[0], q, .5f, .5f, Handles.ConeHandleCap))
        {
            Undo.RecordObject(t.transform, "Rotated box");
            Vector3 rot = t.transform.eulerAngles;
            rot.y -= 90f;
            t.transform.eulerAngles = rot;
        }
        Handles.color = Color.magenta;
        y += 180f;
        q = Quaternion.Euler(0f, y, 0f);
        if (Handles.Button(pos + dirs2[1], q, .5f, .5f, Handles.ConeHandleCap))
        {
            Undo.RecordObject(t.transform, "Rotated box");
            Vector3 rot = t.transform.eulerAngles;
            rot.y += 90f;
            t.transform.eulerAngles = rot;
        }
    }
    Vector3[] moveDirs = new[] { new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, -1f), 
        new Vector3(-1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, -1f, 0f), };
    void CreateMoveArrows(Vector3 center)
    {
        float y = 0f;
        foreach (Vector3 dir in moveDirs)
        {
            Vector3 pos = center + (dir * 2);
            Quaternion q = Quaternion.Euler(0f, y, 0f);
            y += 90f;
            if (dir.y == 1)
                q = Quaternion.Euler(-90f, 0f, 0f);
            if (dir.y == -1)
                q = Quaternion.Euler(90f, 0f, 0f);
            Handles.color = Color.blue;
            if (Handles.Button(pos, q, .5f, .5f, Handles.ArrowHandleCap))
            {
                foreach (GameObject go in Selection.objects)
                {
                    
                    MapTile mt = go.GetComponent<MapTile>();
                    if (mt != null)
                    {
                        Undo.RecordObject(mt.transform, "Moved tile");
                        mt.transform.Translate(dir * 2);
                    }

                    
                }
                
            }

        }
    }
    
    /// <summary>
    /// Creates a new tile at the position. Also sets the new tile as currently selected
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pos"></param>
    static GameObject SpawnTileAt(GameObject source, Vector3 pos)
    {
        //Spawns the tile
        GameObject newTile = Instantiate(source, source.transform.parent);
        //Register created obj to undo
        Undo.RegisterCreatedObjectUndo(newTile, "Created tile");
        newTile.transform.position = pos;
        newTile.name = $"{pos.preciseVector3IntString()}";
        Selection.objects = new[] { newTile };
        return newTile;
    }
}
