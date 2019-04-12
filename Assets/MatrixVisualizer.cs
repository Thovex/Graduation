using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
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
    public List<Matrix3<string>> InMatrix = new List<Matrix3<string>>();

    [SerializeField] private int spacing = 1;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < InMatrix.Count; i++)
        {
            For3(InMatrix[i], (x, y, z) =>
            {
                Handles.Label(
                    transform.position + (new Vector3Int(x, y, z) * spacing) + (InMatrix[i].SizeX * Vector3.right * spacing * i), 
                    InMatrix[i].GetDataAt(x, y, z)
                );
            });

            Vector3 cubeSize = (Vector3)InMatrix[i].Size / 1.5F * spacing;
            cubeSize.x = 0;

            Gizmos.DrawWireCube(transform.position + Vector3.right * i + (new Vector3(InMatrix[i].SizeX + 1, 1, 1) /2 ) + (Vector3.right *spacing * i), cubeSize);

        }
    }
}
