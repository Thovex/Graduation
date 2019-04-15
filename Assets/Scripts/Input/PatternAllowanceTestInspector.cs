using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternAllowanceTest patternTest = (PatternAllowanceTest)target;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Forward", GUILayout.Height(75), GUILayout.Width(150)))
        {
            patternTest.Move(EOrientations.FORWARD);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Left", GUILayout.Height(75), GUILayout.Width(150)))
        {
            patternTest.Move(EOrientations.LEFT);
        }

        if (GUILayout.Button("Right", GUILayout.Height(75), GUILayout.Width(150)))
        {
            patternTest.Move(EOrientations.RIGHT);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Back", GUILayout.Height(75), GUILayout.Width(150)))
        {
            patternTest.Move(EOrientations.BACK);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Up", GUILayout.Width(50), GUILayout.Height(20)))
        {
            patternTest.Move(EOrientations.UP);
        }

        if (GUILayout.Button("Down", GUILayout.Width(50), GUILayout.Height(20)))
        {
            patternTest.Move(EOrientations.DOWN);
        }
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Init", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.Initialize();
        }

        if (GUILayout.Button("Gen Pattern @ Coords", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.InitializePattern();
        }


    }
}