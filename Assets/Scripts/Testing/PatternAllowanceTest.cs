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

    [SerializeField] private Transform objects;
    [SerializeField] private Transform displayTarget;

    [SerializeField] private List<Pattern> allowedPatterns = new List<Pattern>();

    List<Tuple<string, EOrientations>> lookingFor = new List<Tuple<string, EOrientations>>();
    Vector3Int lastpropagated = Vector3Int.zero;

    private Pattern moduleMatrix;
    private Matrix3<bool> wave;
    private Matrix3<Coefficient> coefficients;
    private Matrix3<Pattern> patterns;

    private Matrix3<Vector3Int> userCoordinateMatrix = new Matrix3<Vector3Int>();

    private int mostCoefficients = 0;

    [SerializeField] bool drawDebug = true;

    private void Update()
    {
        if (training)
        {
            // Initialize();
        }
    }
    public void Initialize()
    {
        moduleMatrix = new Pattern(outputSize);
        wave = new Matrix3<bool>(outputSize);
        coefficients = new Matrix3<Coefficient>(outputSize);
        //patterns = new Matrix3<Pattern>(outputSize);


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


        InitializePattern(userCoordinateMatrix, training.Patterns[patternIndex]);
    }


    public void InitSelectionMatrix()
    {
        Vector3Int nSize = new Vector3Int(training.N, training.N, training.N);
        userCoordinateMatrix = new Matrix3<Vector3Int>(nSize);

        For3(userCoordinateMatrix, (x, y, z) =>
        {
            userCoordinateMatrix.MatrixData[x, y, z] = new Vector3Int(x + 3, y, z + 3);
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

    public void InitializePattern()
    {
        try
        {
            while (!IsFullyCollapsed())
            {
                Vector3Int minEntropyCoords = MinEntropyCoords();
                Collapse(minEntropyCoords, coefficients.GetDataAt(minEntropyCoords).AllowedBits.PickRandom());
            }
        } catch (Exception)
        {
            drawDebug = false;
        }
    }

    private void InitializePattern(Matrix3<Vector3Int> patternSpawnCoordinate, Pattern pattern)
    {


        For3(patternSpawnCoordinate, (x, y, z) =>
        {
            Vector3Int patternCoord = patternSpawnCoordinate.GetDataAt(x, y, z);

            Collapse(
                patternCoord,
                pattern.GetDataAt(x, y, z).GenerateBit(training)
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
        lastpropagated = coord;
        lookingFor = new List<Tuple<string, EOrientations>>();

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

        // if (coefficients.GetDataAt(coord).AllowedBits.Count == 1)
        //{
        //    Collapse(coord, coefficients.GetDataAt(coord).AllowedBits.ToList().PickRandom());
        // }
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
        if (drawDebug)
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

                foreach (var b in lookingFor)
                {
                    Gizmos.DrawLine(transform.position + lastpropagated, transform.position + lastpropagated + Orientations.ToUnitVector(b.Item2));
                }

                Gizmos.color = new Color(1, 0, 1, 0.2F);

                For3(userCoordinateMatrix, (x, y, z) =>
                {
                    Gizmos.DrawSphere(transform.position + userCoordinateMatrix.MatrixData[x, y, z], 0.25F);
                });
            }
            catch (Exception) { }
        }
    }

    public void Move(EOrientations orientation)
    {

        For3(userCoordinateMatrix, (x, y, z) =>
        {
            userCoordinateMatrix.MatrixData[x, y, z] += Orientations.ToUnitVector(orientation) * 2;
        });

        allowedPatterns.Clear();

        Dictionary<EOrientations, Pattern> orientationWithPattern = new Dictionary<EOrientations, Pattern>();

        For3(userCoordinateMatrix, (x, y, z) =>
        {
            Vector3Int coord = userCoordinateMatrix.GetDataAt(x, y, z);

            foreach (var direction in Orientations.OrientationUnitVectorsDefaultAngles)
            {
                Vector3Int neighbourCoord = coord + direction.Value;

                if (wave.ValidCoordinate(neighbourCoord))
                {
                    if (!wave.GetDataAt(neighbourCoord))
                    {
                        if (!orientationWithPattern.ContainsKey(direction.Key))
                        {
                            orientationWithPattern.Add(direction.Key, patterns.GetDataAt(neighbourCoord));
                        }
                    }
                }
            }
        });

        Dictionary<Pattern, int> patternAndCount = new Dictionary<Pattern, int>();

        foreach (var pair in orientationWithPattern)
        {
            pair.Value.Propagator.TryGetValue(Orientations.ToUnitVector(pair.Key), out List<int> value);

            foreach (int i in value)
            {
                if (patternAndCount.ContainsKey(training.Patterns[i]))
                {
                    patternAndCount.TryGetValue(training.Patterns[i], out int currentValue);
                    currentValue++;
                    patternAndCount[training.Patterns[i]] = currentValue;
                }
                else
                {
                    patternAndCount.Add(training.Patterns[i], 1);
                }
            }
        }

        foreach (var pair in patternAndCount)
        {
            if (pair.Value == orientationWithPattern.Count)
            {
                allowedPatterns.Add(pair.Key);
            }
        }

        //         Matrix3<string> bitMatrix = new Matrix3<string>(training.N);
        // 
        //         For3(bitMatrix, (x, y, z) => { bitMatrix.MatrixData[x, y, z] = "null"; });
        // 
        //         For3(userCoordinateMatrix, (x, y, z) =>
        //         {
        //             Vector3Int coord = userCoordinateMatrix.GetDataAt(x, y, z);
        // 
        //             if (!wave.GetDataAt(coord))
        //             {
        //                 bitMatrix.MatrixData[x, y, z] = moduleMatrix.GetDataAt(coord).GenerateBit(training);
        //             }
        //         });
        //         
        //         // TO DO USE PROPAGATOR?
        // 
        // 
        //         foreach (Pattern pattern in training.Patterns)
        //         {
        //             if (pattern.CompareBitPatterns(training, bitMatrix))
        //             {
        //                 allowedPatterns.Add(pattern);
        //             }
        //         }
    }
}
