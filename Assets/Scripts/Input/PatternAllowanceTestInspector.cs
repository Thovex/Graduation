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

            patternTest.TestConstrainAll();

        }

        obj1 = EditorGUILayout.Vector3IntField("GameObject 1:", obj1);

        if (GUILayout.Button("Constrain Obj", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.Observe(Vector3Int.zero);
        }

        if (GUILayout.Button("Propagate", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.Propagate();
        }

        if (GUILayout.Button("Test constrain all", GUILayout.Width(150), GUILayout.Height(20)))
        {
            patternTest.TestConstrainAll();
        }


    }
}