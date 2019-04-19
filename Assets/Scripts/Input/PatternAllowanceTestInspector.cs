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
    Vector3Int obj1;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PatternAllowanceTest patternTest = (PatternAllowanceTest)target;


        if (GUILayout.Button("Init", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.Initialize();
        }

        obj1 = EditorGUILayout.Vector3IntField("GameObject 1:", obj1);

        if (GUILayout.Button("Observe", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.Constrain(obj1);
        }

        if (GUILayout.Button("Propagate", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.Propagate();
        }

        if (GUILayout.Button("Check if is collapsed", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.IsFullyCollapsed();
        }


    }
}