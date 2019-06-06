using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using World;

[CustomEditor(typeof(MapGenerator))]    
public class MapGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        MapGenerator generator = (MapGenerator)target;

        if (DrawDefaultInspector() && generator.autoUpdate)
        {
            generator.DrawMapInEditor();
        }

        if (GUILayout.Button("New Seed"))
        {
            generator.NewSeed();
            if (generator.autoUpdate)
            {
                generator.DrawMapInEditor();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            generator.DrawMapInEditor();
        }        
            
    }
    
}

