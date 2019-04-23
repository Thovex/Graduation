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

    private Matrix3<Coefficient> wave;
    private Stack<Vector3Int> flag;

    private List<Vector3Int> updated = new List<Vector3Int>();

    [SerializeField] private Dictionary<Vector3Int, Pattern> patternPerCoord = new Dictionary<Vector3Int, Pattern>();

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
        training.Train();

        for (int i = objects.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(objects.transform.GetChild(0).gameObject);
        }

        wave = new Matrix3<Coefficient>(outputSize);
        flag = new Stack<Vector3Int>();

        patternPerCoord = new Dictionary<Vector3Int, Pattern>();

        Dictionary<Pattern, bool> initCoefficientDictionary = new Dictionary<Pattern, bool>();

        foreach (Pattern pattern in training.Patterns)
        {
            initCoefficientDictionary.Add(pattern, true);
        }

        For3(wave, (x, y, z) =>
        {
            Coefficient initCoefficient = new Coefficient(new Dictionary<Pattern, bool>(initCoefficientDictionary));
            mostCoefficients = initCoefficient.AllowedCount();
            wave.SetDataAt(x, y, z, initCoefficient);

        });

        //InitialPattern();
    }

    private void InitialPattern()
    {
        Vector3Int coord = Vector3Int.zero;


        Dictionary<Pattern, bool> exampleDict = new Dictionary<Pattern, bool>();
        exampleDict.Add(training.Patterns[patternIndex], true);

        wave.SetDataAt(coord, new Coefficient(exampleDict));

        Pattern selected = wave.GetDataAt(coord).GetLastAllowedPattern();

        SpawnMod(coord, selected);

        patternPerCoord.Add(coord, selected);
        flag.Push(coord);

        Propagate();
    }

    private void SpawnMod(Vector3Int coord, Pattern selected)
    {
       // For3(selected, (x, y, z) =>
      //  {
            string bit = selected.GetDataAt(0,0,0).GenerateBit(training);
            Module module = training.CreateModuleFromBit(bit);

            training.SpawnModule(module, coord + new Vector3Int(0,0,0), objects.transform);
      //  });
    }

    public void Observe(Vector3Int value)
    {
        Vector3Int coord = value != Vector3Int.zero ? value : MinEntropyCoords();

        var allowedPatterns = wave.GetDataAt(coord).allowedPatterns;


        Pattern selected = GetWeightedRandomPattern(allowedPatterns);

        Dictionary<Pattern, bool> newAllowedPatterns = new Dictionary<Pattern, bool>();

        foreach (var allowedPattern in allowedPatterns)
        {
            if (allowedPattern.Key == selected)
            {
                newAllowedPatterns.Add(allowedPattern.Key, true);
            }
            else
            {
                newAllowedPatterns.Add(allowedPattern.Key, false);
            }
        }

        Coefficient coefficient = wave.GetDataAt(coord);
        coefficient.allowedPatterns = newAllowedPatterns;
        wave.SetDataAt(coord, coefficient);

        SpawnMod(coord, selected);



        patternPerCoord.Add(coord, selected);
        flag.Push(coord);

        Propagate();
    }

    public void TestConstrainAll()
    {
        while (!IsFullyCollapsed())
        {
            Observe(MinEntropyCoords());
            Propagate();
        }
    }

    private Pattern GetWeightedRandomPattern(Dictionary<Pattern, bool> inPatterns)
    {
        List<Pattern> weightedPatterns = new List<Pattern>();

        foreach (var allowedPattern in inPatterns)
        {
            if (allowedPattern.Value)
            {
                training.Weights.TryGetValue(allowedPattern.Key, out int weight);

                for (int i = 0; i < weight; i++)
                {
                    weightedPatterns.Add(allowedPattern.Key);
                }
            }
        }

        return weightedPatterns.PickRandom();
    }

    public void Propagate()
    {
        updated.Clear();

        while (flag.Count > 0)
        {
            Vector3Int coord = flag.Pop();
            updated.Add(coord);

            foreach (var direction in Orientations.OrientationUnitVectors)
            {
                if (direction.Key == EOrientations.NULL) continue;

                Vector3Int neighbourCoord = coord + direction.Value;

                if (wave.ValidCoordinate(neighbourCoord))
                {
                    if (wave.GetDataAt(neighbourCoord).AllowedCount() != 1)
                    {

                        Constrain(neighbourCoord);
                    }
                }
            }
        }
    }

    public void Constrain(Vector3Int coord)
    {

        Dictionary<EOrientations, HashSet<Pattern>> newValidData = new Dictionary<EOrientations, HashSet<Pattern>>();

        // Loop through all directions from constrained coord.
        foreach (var direction in Orientations.OrientationUnitVectors)
        {
            if (direction.Key == EOrientations.NULL) continue;

            Vector3Int neighbourCoord = coord + direction.Value;

            // If it's a valid direction (in the matrix)
            if (wave.ValidCoordinate(neighbourCoord))
            {
                // Create a dictionary to hold our values, we will be checking if this count is equal to
                // the amount of patterns we check.
                foreach (var allowedPattern in wave.GetDataAt(neighbourCoord).allowedPatterns)
                {
                    if (allowedPattern.Value)
                    {
                        if (allowedPattern.Key.Propagator.TryGetValue(direction.Value * -1, out List<Pattern> patternsAllowedInSide))
                        {

                            if (newValidData.ContainsKey(direction.Key))
                            {
                                newValidData.TryGetValue(direction.Key, out HashSet<Pattern> validHashSet);

                                foreach (Pattern pattern in patternsAllowedInSide)
                                {
                                    validHashSet.Add(pattern);
                                }

                                newValidData[direction.Key] = validHashSet;
                            }
                            else
                            {
                                HashSet<Pattern> validHashSet = new HashSet<Pattern>();

                                foreach (Pattern pattern in patternsAllowedInSide)
                                {
                                    validHashSet.Add(pattern);
                                }

                                newValidData.Add(direction.Key, validHashSet);
                            }
                        }
                    }
                }
            }
        }

        Dictionary<Pattern, int> patternCounts = new Dictionary<Pattern, int>();

        foreach (var validData in newValidData)
        {
            foreach (Pattern pattern in validData.Value)
            {
                if (patternCounts.ContainsKey(pattern))
                {
                    patternCounts.TryGetValue(pattern, out int count);
                    patternCounts[pattern] = count + 1;
                }
                else
                {
                    patternCounts.Add(pattern, 0);
                }
            }
        }

        Dictionary<Pattern, bool> newAllowedPatterns = wave.GetDataAt(coord).allowedPatterns;


        Dictionary<Pattern, bool> tempNewAllowedDict = new Dictionary<Pattern, bool>(newAllowedPatterns);

        int changedCount = 0;

        foreach (var patternAlloweds in newAllowedPatterns)
        {
            if (patternAlloweds.Value)
            {
                changedCount++;
            }

            tempNewAllowedDict[patternAlloweds.Key] = false;
        }

        newAllowedPatterns = tempNewAllowedDict;

        foreach (var patternCount in patternCounts)
        {
            if (patternCount.Value == newValidData.Count - 1)
            {
                newAllowedPatterns[patternCount.Key] = true;
                changedCount--;
            }
        }


        // Loop through all directions from constrained coord.
        foreach (var direction in Orientations.OrientationUnitVectors)
        {
            if (direction.Key == EOrientations.NULL) continue;

            Vector3Int neighbourCoord = coord + direction.Value;

            // If it's a valid direction (in the matrix)
            if (wave.ValidCoordinate(neighbourCoord))
            {
                if (changedCount > 0 && !updated.Contains(neighbourCoord))
                {
                    flag.Push(neighbourCoord);
                }
            }
        }

        Coefficient coefficient = wave.GetDataAt(coord);
        coefficient.allowedPatterns = newAllowedPatterns;
        wave.SetDataAt(coord, coefficient);

        if (wave.GetDataAt(coord).AllowedCount() == 1)
        {

            Pattern selected = wave.GetDataAt(coord).GetLastAllowedPattern();

            patternPerCoord.Add(coord, selected);

            SpawnMod(coord, selected);

        }


        updated.Add(coord);
    }

    public bool IsFullyCollapsed()
    {
        int allowedCount = 0;

        For3(wave, (x, y, z) =>
        {
            if (wave.GetDataAt(x, y, z).AllowedCount() > allowedCount)
            {
                allowedCount = wave.GetDataAt(x, y, z).AllowedCount();
            }
        });

        return allowedCount > 1 ? false : true;
    }

    private Vector3Int MinEntropyCoords()
    {
        float minEntropy = 0;
        Vector3Int minEntropyCoords = new Vector3Int();

        System.Random random = new System.Random();

        For3(outputSize, (x, y, z) =>
        {
            Vector3Int currentCoordinates = new Vector3Int(x, y, z);

            if (wave.MatrixData[x, y, z].AllowedCount() > 1)
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

        foreach (var pair in wave.GetDataAt(currentCoordinates).allowedPatterns)
        {
            training.Weights.TryGetValue(pair.Key, out int weight);

            sumOfWeights += weight;
            sumOfWeightsLogWeights += weight * (float)Math.Log(weight);
        }
        return (float)Math.Log(sumOfWeights) - (sumOfWeightsLogWeights / sumOfWeights);
    }

    private void OnDrawGizmos()
    {
        if (drawDebug)
        {

            //             Vector3Int minEntropyCoords = MinEntropyCoords();

            For3(wave, (x, y, z) =>
            {
                Vector3Int coord = new Vector3Int(x, y, z);

                int currentAllowedCount = wave.GetDataAt(coord).AllowedCount();

                Gizmos.color =
                wave.GetDataAt(x, y, z).AllowedCount() > 1 ?
                    new Color(0, 1, 0, SetScale(currentAllowedCount, 0, mostCoefficients, 0, 0.5F)) :
                    new Color(1, 0, 0, 0.2F);

                float scale = SetScale(currentAllowedCount, 0, mostCoefficients, 0.05F, .2F);

                Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), scale);
                Handles.Label(transform.position + new Vector3(x, y, z), currentAllowedCount.ToString());


                if (patternPerCoord.ContainsKey(coord))
                {
                    Handles.Label(transform.position + new Vector3(x, y + 0.25F, z), "P" + patternPerCoord[coord].id.ToString());
                }


            });

        }
    }
}
