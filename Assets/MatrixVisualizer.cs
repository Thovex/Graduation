using Sirenix.OdinInspector.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;

[CustomEditor(typeof(MatrixVisualizer))]
public class MatrixVisualizerInspector : OdinEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MatrixVisualizer visualizer = (MatrixVisualizer)target;

        if (GUILayout.Button("Clear", GUILayout.Height(75), GUILayout.Width(150)))
        {
            visualizer.InMatrix.Clear();
        }
    }
}

[ExecuteAlways]
public class MatrixVisualizer : MonoBehaviour
{
    public List<Tuple<Matrix3<string>, Color, int, HashSet<string>>> InMatrix = new List<Tuple<Matrix3<string>, Color, int, HashSet<string>>>();
    public Dictionary<Vector3, Pattern> Display = new Dictionary<Vector3, Pattern>();

    public TrainingScript training;
    public GameObject patternsHere;

    [SerializeField] private int spacing = 1;

    private void OnDrawGizmos()
    {

        if (!training) return;
        Display.Clear();

        for (int i = 0; i < InMatrix.Count; i++)
        {

            Matrix3<string> bits = InMatrix[i].Item1;

            For3(InMatrix[i].Item1, (x, y, z) =>
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = InMatrix[i].Item2;


                Handles.Label(
                    transform.position + (new Vector3Int(x, y, z) * spacing) + (InMatrix[i].Item1.SizeX * Vector3.right * spacing * i),
                    bits.GetDataAt(x, y, z), style
                );
            });


            Vector3 cubeSize = (Vector3)InMatrix[i].Item1.Size / 1.5F * spacing;
            cubeSize.x = 0;


            if (InMatrix[i].Item2 == Color.green)
            {
                //Display.Add(transform.position + Vector3.right * i + (new Vector3(InMatrix[i].Item1.SizeX + 1, 1, 1) / 2) + (Vector3.right * spacing * i), InMatrix[i].Item1);
                List<string> hashSetToList = new List<string>();


                if (InMatrix[i].Item4 != null)
                {
                    hashSetToList = InMatrix[i].Item4.ToList();

                }

                for (int j = 0; j < hashSetToList.Count; j++)
                {
                    Handles.Label(transform.position + ((Vector3.right / 2) * spacing) + (InMatrix[i].Item1.SizeX * Vector3.right * spacing * i) + Vector3.back * (j + 1), hashSetToList[j]);
                }


            }

        }


    }

    private void Update()
    {
        //         if (patternsHere && training)
        //         {
        //             for (int i = patternsHere.transform.childCount; i > 0; --i)
        //             {
        //                 DestroyImmediate(patternsHere.transform.GetChild(0).gameObject);
        //             }
        // 
        //             foreach (var display in Display)
        //             {
        //                 For3(display.Value, (x, y, z) =>
        //                 {
        //                     if (display.Value.MatrixData[x, y, z].Prefab != null)
        //                     {
        //                         GameObject patternData = Instantiate(display.Value.MatrixData[x, y, z].Prefab, patternsHere.transform);
        //                         patternData.transform.localPosition = display.Key + new Vector3(x, y, z);
        //                         patternData.transform.localEulerAngles = display.Value.MatrixData[x, y, z].RotationEuler;
        //                     }
        //                 });
        //             }
        //         }
        //     }
    }
}
