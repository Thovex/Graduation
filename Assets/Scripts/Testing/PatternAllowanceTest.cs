using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;



[ExecuteAlways]
public class PatternAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript training;
    [SerializeField] private Vector3Int outputSize;

    [SerializeField] private int patternIndex = 0;
    [SerializeField] private Vector3Int patternSpawnCoordinate = Vector3Int.zero;

    [SerializeField] private Transform objects;
    [SerializeField] private Transform displayTarget;

    [SerializeField] private List<Pattern> allowedPatterns = new List<Pattern>();


    private Pattern moduleMatrix;
    private Matrix3<bool> wave;
    private Matrix3<Coefficient> coefficients;

    private Matrix3<Vector3Int> userCoordinateMatrix = new Matrix3<Vector3Int>();

    private int mostCoefficients = 0;

    private void Update()
    {
        if (training)
        {
            Initialize();
        }
    }
    public void Initialize()
    {
        moduleMatrix = new Pattern(outputSize);
        wave = new Matrix3<bool>(outputSize);
        coefficients = new Matrix3<Coefficient>(outputSize);

        for (int i = objects.childCount; i > 0; --i)
        {
            DestroyImmediate(objects.GetChild(0).gameObject);
        }

        InitSelectionMatrix();

        HashSet<string> allowedData = ReturnAllowedData();

        For3(outputSize, (x, y, z) =>
        {
            wave.MatrixData[x, y, z] = true;
            coefficients.MatrixData[x, y, z] = new Coefficient(allowedData);
        });

        InitializeInitialPattern();
    }


    public void InitSelectionMatrix()
    {
        Vector3Int nSize = new Vector3Int(training.N, training.N, training.N);
        userCoordinateMatrix = new Matrix3<Vector3Int>(nSize);

        For3(userCoordinateMatrix, (x, y, z) =>
        {
            userCoordinateMatrix.MatrixData[x, y, z] = new Vector3Int(x, y, z);
        });
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
            moduleMatrix.SetDataAt(coord, training.CreateModuleFromBit(bit));
            training.SpawnModule(moduleMatrix.GetDataAt(coord), coord, objects);
            coefficients.SetDataAt(coord, new Coefficient(new HashSet<string> { }));
            wave.SetDataAt(coord, false);

            foreach (var direction in Orientations.OrientationUnitVectors)
            {
                Vector3Int neighbourCoord = coord + direction.Value;

                if (wave.ValidCoordinate(neighbourCoord))
                {
                    if (wave.GetDataAt(neighbourCoord))
                    {
                        Propagate(neighbourCoord);
                    }
                }
            }
        }


    }

    private void Propagate(Vector3Int coord)
    {
        List<Tuple<string, EOrientations>> lookingFor = new List<Tuple<string, EOrientations>>();

        foreach (var direction in Orientations.OrientationUnitVectors)
        {
            Vector3Int neighbourCoord = coord + direction.Value;

            if (wave.ValidCoordinate(neighbourCoord))
            {
                if (!wave.GetDataAt(neighbourCoord))
                {
                    string bit = moduleMatrix.GetDataAt(neighbourCoord).GenerateBit(training);
                    lookingFor.Add(new Tuple<string, EOrientations>(bit, direction.Key));
                }
            }
        }

        Coefficient coefficient = coefficients.GetDataAt(coord);
        coefficient.AllowedBits = training.RetrieveAllowedBits(lookingFor);
        coefficients.SetDataAt(coord, coefficient);

        //         if (coefficients.GetDataAt(coord).AllowedBits.Count == 1)
        //         {
        //             Collapse(coord, coefficients.GetDataAt(coord).AllowedBits.ToList()[0]);
        //         }
    }

    public void UpdateAll()
    {
        Vector3Int coord = MinEntropyCoords() + Orientations.OrientationUnitVectors.Values.PickRandom();
    }

    private bool IsFullyCollapsed()
    {
        return !wave.Contains(true);
    }

    private Vector3Int MinEntropyCoords()
    {
        float minEntropy = 0;
        Vector3Int minEntropyCoords = new Vector3Int();

        System.Random random = new System.Random();

        For3(outputSize, (x, y, z) =>
        {
            Vector3Int currentCoordinates = new Vector3Int(x, y, z);
            if (coefficients.MatrixData[x, y, z].AllowedBits.Any())
            {
                float entropy = ShannonEntropy(currentCoordinates);
                float entropyPlusNoise = entropy - (float)random.NextDouble() / 1000;

                if (minEntropy == 0 || entropyPlusNoise < minEntropy)
                {
                    minEntropy = entropyPlusNoise;
                    minEntropyCoords = new Vector3Int(x, y, z);
                }
            }
        });

        return minEntropyCoords;
    }

    public float ShannonEntropy(Vector3Int currentCoordinates)
    {
        int sumOfWeights = 0;
        float sumOfWeightsLogWeights = 0;

        foreach (string pair in coefficients.GetDataAt(currentCoordinates).AllowedBits)
        {
            training.Weights.TryGetValue(pair, out int weight);

            sumOfWeights += weight;
            sumOfWeightsLogWeights += weight * (float)Math.Log(weight);
        }
        return (float)Math.Log(sumOfWeights) - (sumOfWeightsLogWeights / sumOfWeights);
    }

    private void OnDrawGizmos()
    {
        try
        {
            //Vector3Int minEntropyCoords = MinEntropyCoords();

            For3(wave, (x, y, z) =>
            {
                Vector3Int coord = new Vector3Int(x, y, z);

                int currentAllowedCount = coefficients.GetDataAt(x, y, z).AllowedBits.Count;

                Gizmos.color =
                wave.GetDataAt(x, y, z) ?
                    new Color(0, 1, 0, SetScale(currentAllowedCount, 0, mostCoefficients, 0, 0.5F)) :
                    new Color(1, 0, 0, 0.2F);

                float scale = SetScale(currentAllowedCount, 0, mostCoefficients, 1, 0.1F);

                Gizmos.DrawCube(transform.position + new Vector3(x, y, z), new Vector3(scale, scale, scale));
                Handles.Label(transform.position + new Vector3(x, y, z), coefficients.GetDataAt(x, y, z).AllowedBits.Count.ToString());

                //if (minEntropyCoords == coord)
                //{
                //    Gizmos.color = new Color(1, 0, 1, 0.2F);
                //    Gizmos.DrawSphere(transform.position + coord, 0.25F);
                //}
            });

            Gizmos.color = new Color(1, 0, 1, 0.2F);

            For3(userCoordinateMatrix, (x, y, z) =>
            {
                Gizmos.DrawSphere(transform.position + userCoordinateMatrix.MatrixData[x, y, z], 0.25F);
            });
        }
        catch (Exception) { }
    }

    public void Move(EOrientations orientation)
    {

        For3(userCoordinateMatrix, (x, y, z) =>
        {
            userCoordinateMatrix.MatrixData[x, y, z] += Orientations.ToUnitVector(orientation);
        });

        allowedPatterns.Clear();

        Matrix3<string> bitMatrix = new Matrix3<string>(training.N);

        For3(bitMatrix, (x, y, z) => { bitMatrix.MatrixData[x, y, z] = "null"; });

        For3(userCoordinateMatrix, (x, y, z) =>
        {
            Vector3Int coord = userCoordinateMatrix.GetDataAt(x, y, z);

            if (!wave.GetDataAt(coord))
            {
                bitMatrix.MatrixData[x, y, z] = moduleMatrix.GetDataAt(coord).GenerateBit(training);
            }
        });

        foreach (Pattern pattern in training.Patterns)
        {
            if (pattern.CompareBitPatterns(training, bitMatrix))
            {
                allowedPatterns.Add(pattern);
            }
        }
    }
}
