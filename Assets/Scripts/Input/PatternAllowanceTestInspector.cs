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