using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;


[CustomEditor(typeof(PropagatorAllowanceTest))]
public class PropagatorAllowanceTestInspector : OdinEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PropagatorAllowanceTest propagatorTest = (PropagatorAllowanceTest)target;

        if (GUILayout.Button("Sync", GUILayout.Height(75)))
        {
            propagatorTest.Sync();
        }

        if (GUILayout.Button("SyncNeighbour", GUILayout.Height(75)))
        {
            propagatorTest.SyncNeighbour();
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Forward", GUILayout.Height(75), GUILayout.Width(150)))
        {
            propagatorTest.Move(EOrientations.FORWARD);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Left", GUILayout.Height(75), GUILayout.Width(150)))
        {
            propagatorTest.Move(EOrientations.LEFT);
        }

        if (GUILayout.Button("Right", GUILayout.Height(75), GUILayout.Width(150)))
        {
            propagatorTest.Move(EOrientations.RIGHT);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Back", GUILayout.Height(75), GUILayout.Width(150)))
        {
            propagatorTest.Move(EOrientations.BACK);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Flip Horizontal (RIGHT/LEFT)", GUILayout.Width(150), GUILayout.Height(20)))
        {
            propagatorTest.Flip(EOrientations.LEFT);
        }

        if (GUILayout.Button("Flip Vertical (FORWARD/BACK)", GUILayout.Width(150), GUILayout.Height(20)))
        {
            propagatorTest.Flip(EOrientations.FORWARD);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}

[ExecuteAlways]
public class PropagatorAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript training;
    [SerializeField] private int patternToDisplay;

    [SerializeField] private EOrientations orientationToDisplay;

    [SerializeField] private Transform patternSpawnTransform;
    [SerializeField] private Transform patternNSpawnTransform;

    [SerializeField] private int N = 2;

    [OdinSerialize] private Pattern selectedPattern = new Pattern(2);

    public void Sync()
    {
        training.Train();

        selectedPattern = training.Patterns[patternToDisplay];
        selectedPattern.BuildPropagator(training, orientationToDisplay);

        for (int i = patternSpawnTransform.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(patternSpawnTransform.transform.GetChild(0).gameObject);
        }

        GameObject newPattern = new GameObject("Pattern");

        newPattern.transform.parent = patternSpawnTransform.transform;
        newPattern.transform.localPosition = Vector3.zero;


        For3(selectedPattern, (x, y, z) =>
        {
            if (selectedPattern.MatrixData[x, y, z].Prefab != null)
            {
                GameObject patternData = Instantiate(selectedPattern.MatrixData[x, y, z].Prefab, newPattern.transform);
                patternData.transform.localPosition = new Vector3(x, y, z);
                patternData.transform.localEulerAngles = selectedPattern.MatrixData[x, y, z].RotationEuler;
            }
        });

        SyncNeighbour();
    }

    public void SyncNeighbour()
    {

        for (int i = patternNSpawnTransform.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(patternNSpawnTransform.transform.GetChild(0).gameObject);
        }

        if (orientationToDisplay == EOrientations.NULL)
        {
            foreach (var direction in Orientations.OrientationUnitVectors)
            {
                if (direction.Key == EOrientations.NULL) continue;

                Place(direction);
            }
        }
        else
        {
            foreach (var direction in Orientations.OrientationUnitVectors)
            {
                if (direction.Key != orientationToDisplay) continue;

                Place(direction);
            }
        }
    }

    private void Place(KeyValuePair<EOrientations, Vector3Int> direction)
    {
        if (selectedPattern.Propagator.TryGetValue(direction.Value, out List<Pattern> value))
        {
            if (value.Count == 0)
            {
                Debug.LogError("No values");
            }
            else
            {
                GameObject newPattern2 = new GameObject("Pattern");

                newPattern2.transform.parent = patternNSpawnTransform.transform;
                newPattern2.transform.localPosition = Vector3.zero + direction.Value * 2;

                for (int i = 0; i < value.Count; i++)
                {
                    For3(value[i], (x, y, z) =>
                    {
                        if (value[i].MatrixData[x, y, z].Prefab != null)
                        {
                            GameObject patternData = Instantiate(value[i].MatrixData[x, y, z].Prefab, newPattern2.transform);
                            patternData.transform.localPosition = new Vector3(x, y, z) + direction.Value * 2 * (i+1) * 2;
                            patternData.transform.localEulerAngles = value[i].MatrixData[x, y, z].RotationEuler;
                        }
                    });
                }
            }
        }
    }

    internal void Flip(EOrientations orientation)
    {
        selectedPattern.Flip(orientation);
    }

    internal void Move(EOrientations orientation)
    {
        selectedPattern.PushData(Orientations.ToUnitVector(orientation));
    }

    private void OnDrawGizmos()
    {

        var selectedPatternBits = selectedPattern.GenerateBits(training);

        For3(selectedPatternBits, (x, y, z) =>
        {
            Handles.Label(transform.position + new Vector3Int(x, y, z) + Vector3.up / 8, selectedPatternBits.GetDataAt(x, y, z));

        });

    }
}
