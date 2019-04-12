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


    [SerializeField] private bool advancedRot = false;

    [SerializeField] private EOrientations neighbourOrientationToDisplay;
    [SerializeField] private EOrientationsAdvanced neighbourOrientationAdvancedToDisplay;

    [SerializeField] private int neighbourIndexToDisplay;

    [SerializeField] private int debugIndexToDisplay;

    private float switchTimer = 5F;

    [SerializeField] private Transform patternSpawnTransform;
    [SerializeField] private Transform patternNSpawnTransform;

    [SerializeField] private int N = 2;

    [OdinSerialize] private Pattern selectedPattern = new Pattern(2);

    public void Sync()
    {
        selectedPattern = training.Patterns[patternToDisplay];
        selectedPattern.BuildPropagator(training);

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


        //Vector3Int direction = advancedRot ? Orientations.ToUnitVector(neighbourOrientationAdvancedToDisplay) : Orientations.ToUnitVector(neighbourOrientationToDisplay);

        SyncDefault();
        SyncAdvanced();
    }

    private void SyncDefault()
    {
        foreach (var direction in Orientations.OrientationUnitVectors)
        {
            if (direction.Key == EOrientations.NULL || direction.Key == EOrientations.UP || direction.Key == EOrientations.DOWN) continue;

            if (selectedPattern.Propagator.TryGetValue(direction.Value, out List<int> value))
            {


                if (value.Count == 0)
                {

                    Debug.LogError("No values");
                }
                else
                {
                    neighbourIndexToDisplay = value.PickRandom();

                    Pattern neighbourPattern = training.Patterns[neighbourIndexToDisplay];

                    GameObject newPattern2 = new GameObject("Pattern");

                    newPattern2.transform.parent = patternNSpawnTransform.transform;
                    newPattern2.transform.localPosition = Vector3.zero + direction.Value;


                    For3(neighbourPattern, (x, y, z) =>
                    {
                        if (neighbourPattern.MatrixData[x, y, z].Prefab != null)
                        {
                            GameObject patternData = Instantiate(neighbourPattern.MatrixData[x, y, z].Prefab, newPattern2.transform);
                            patternData.transform.localPosition = new Vector3(x, y, z) + direction.Value;
                            patternData.transform.localEulerAngles = neighbourPattern.MatrixData[x, y, z].RotationEuler;
                        }
                    });
                }
            }
        }
    }

    private void SyncAdvanced()
    {
        foreach (var direction in Orientations.OrientationUnitVectorsAdvanced)
        {
            if (direction.Key == EOrientationsAdvanced.NULL) continue;

            if (selectedPattern.Propagator.TryGetValue(direction.Value, out List<int> value))
            {


                if (value.Count == 0)
                {

                    Debug.LogError("No values");
                }
                else
                {
                    neighbourIndexToDisplay = value.PickRandom();

                    Pattern neighbourPattern = training.Patterns[neighbourIndexToDisplay];

                    GameObject newPattern2 = new GameObject("Pattern");

                    newPattern2.transform.parent = patternNSpawnTransform.transform;
                    newPattern2.transform.localPosition = Vector3.zero + direction.Value;


                    For3(neighbourPattern, (x, y, z) =>
                    {
                        if (neighbourPattern.MatrixData[x, y, z].Prefab != null)
                        {
                            GameObject patternData = Instantiate(neighbourPattern.MatrixData[x, y, z].Prefab, newPattern2.transform);
                            patternData.transform.localPosition = new Vector3(x, y, z) + direction.Value;
                            patternData.transform.localEulerAngles = neighbourPattern.MatrixData[x, y, z].RotationEuler;
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
        try
        {
            {
                Gizmos.color = new Color(1, 0, 1, 0.2F);

                Pattern pattern = training.Patterns[patternToDisplay];

                foreach (var pair in pattern.Propagator)
                {
                    For3(new Vector3Int(N, N, N), (x, y, z) =>
                    {
                        Gizmos.DrawWireCube(patternSpawnTransform.position + new Vector3Int(x, y, z) + pair.Key, Vector3.one);
                    });
                }

                var selected = selectedPattern.debugPatterns[debugIndexToDisplay];


                For3(selected.Item1, (x, y, z) =>
                {
                    Handles.Label(transform.position + selected.Item2 + new Vector3Int(x, y, z), selected.Item1.GetDataAt(x, y, z));
                });


            }
        }
        catch (Exception) { }

        var selectedPatternBits = selectedPattern.GenerateBits(training);

        For3(selectedPatternBits, (x, y, z) =>
        {
            Handles.Label(transform.position + new Vector3Int(x, y, z) + Vector3.up / 8, selectedPatternBits.GetDataAt(x, y, z));

        });

    }
}
