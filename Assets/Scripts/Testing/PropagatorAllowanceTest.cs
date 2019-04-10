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
    }
}

[ExecuteAlways]
public class PropagatorAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript training;
    [SerializeField] private int patternToDisplay;

    [SerializeField] private EOrientations neighbourOrientationToDisplay;
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


        Vector3Int direction = Orientations.ToUnitVector(neighbourOrientationToDisplay);

        selectedPattern.Propagator.TryGetValue(direction, out List<int> value);

        neighbourIndexToDisplay = value.PickRandom();

        Pattern neighbourPattern = training.Patterns[neighbourIndexToDisplay];

        GameObject newPattern2 = new GameObject("Pattern");

        newPattern2.transform.parent = patternNSpawnTransform.transform;
        newPattern2.transform.localPosition = Vector3.zero;

        For3(neighbourPattern, (x, y, z) =>
        {
            if (neighbourPattern.MatrixData[x, y, z].Prefab != null)
            {
                GameObject patternData = Instantiate(neighbourPattern.MatrixData[x, y, z].Prefab, newPattern2.transform);
                patternData.transform.localPosition = new Vector3(x, y, z) + direction;
                patternData.transform.localEulerAngles = neighbourPattern.MatrixData[x, y, z].RotationEuler;
            }
        });




    }

    private void OnDrawGizmos()
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

            var selectedPatternBits = selectedPattern.GenerateBits(training);

            For3(selectedPatternBits, (x, y, z) =>
            {
                Handles.Label(transform.position + new Vector3Int(x, y, z) + Vector3.up / 8, selectedPatternBits.GetDataAt(x, y, z));

            });

            For3(selected.Item1, (x, y, z) =>
            {
                Handles.Label(transform.position + selected.Item2 + new Vector3Int(x, y, z), selected.Item1.GetDataAt(x, y, z));
            });


        }
    }
}
