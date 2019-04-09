using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Thovex.Utility;

[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{

}

[ExecuteAlways]
public class PatternAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript training;
    [SerializeField] private Vector3Int outputSize;

    [SerializeField] private int patternIndex = 0;
    [SerializeField] private Vector3Int patternSpawnCoordinate = Vector3Int.zero;

    [SerializeField] private Transform objects;

    private Matrix3<Module> moduleMatrix;
    private Matrix3<bool> wave;
    private Matrix3<Coefficient> coefficients;

    private int mostCoefficients = 0;

    private void Update()
    {
        if (training)
        {
            Initialize();
        }
    }
    private void Initialize()
    {
        moduleMatrix = new Matrix3<Module>(outputSize);
        wave = new Matrix3<bool>(outputSize);
        coefficients = new Matrix3<Coefficient>(outputSize);

        for (int i = objects.childCount; i > 0; --i)
        {
            DestroyImmediate(objects.GetChild(0).gameObject);
        }

        HashSet<string> allowedData = ReturnAllowedData();

        For3(outputSize, (x, y, z) =>
        {
            wave.MatrixData[x, y, z] = true;
            coefficients.MatrixData[x, y, z] = new Coefficient(allowedData);
        });

        InitializeInitialPattern();
    }

    private HashSet<string> ReturnAllowedData()
    {
        HashSet<string> allowedHashSet = new HashSet<string>();
        var listData = training.AllowedData.Keys.ToList();

        foreach (var data in listData)
        {
            allowedHashSet.Add(data);
        }

        mostCoefficients = allowedHashSet.Count;

        return allowedHashSet;
    }

    private void InitializeInitialPattern()
    {
        if (!(training.Patterns.Count >= patternIndex))
        {
            Debug.LogError("Pattern index not correct");
            return;
        }

        Pattern initialPattern = training.Patterns.ToList()[patternIndex];

        For3(initialPattern, (x, y, z) =>
        {
            Vector3Int patternCoord = new Vector3Int(x, y, z);

            wave.SetDataAt(patternSpawnCoordinate + patternCoord, false);

            Collapse(
                patternSpawnCoordinate + patternCoord,
                initialPattern.GetDataAt(patternCoord).GenerateBit(training)
            );
        });
    }

    private void Collapse(Vector3Int coord, string bit)
    {
        if (coefficients.GetDataAt(coord).AllowedBits.Contains(bit))
        {
            training.SpawnModule(training.CreateModuleFromBit(bit), coord, objects);

            coefficients.SetDataAt(coord, new Coefficient(new HashSet<string> { bit }));
        }
    }

    private void Propagate(Vector3Int coord)
    {

    }

    private void OnDrawGizmos()
    {
        For3(wave, (x, y, z) =>
        {
            Gizmos.color =
            wave.GetDataAt(x, y, z) ?
                new Color(0, 1, 0, SetScale(coefficients.GetDataAt(x, y, z).AllowedBits.Count, 0, mostCoefficients, 0, 0.5F)) :
                new Color(1, 0, 0, 0.2F);

            Gizmos.DrawCube(transform.position + new Vector3(x, y, z), Vector3.one / 4);
            Handles.Label(transform.position + new Vector3(x, y, z), coefficients.GetDataAt(x, y, z).AllowedBits.Count.ToString());

        });
    }
}