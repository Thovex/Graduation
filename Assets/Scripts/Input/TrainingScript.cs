﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;
using Vector3Int = UnityEngine.Vector3Int;

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : OdinEditor
{
    bool drawOdinEditor = false;

    public override void OnInspectorGUI()
    {
        drawOdinEditor = EditorGUILayout.Toggle("Draw Odin Editor", drawOdinEditor);
        if (drawOdinEditor)
        {
            base.OnInspectorGUI();
        }
        else
        {
            DrawUnityInspector();
        }

        TrainingScript training = (TrainingScript)target;
        if (GUILayout.Button("Update"))
        {
            training.Train();
        }
    }
}

[ExecuteAlways]
public class TrainingScript : SerializedMonoBehaviour
{
    [SerializeField] private GameObject displayPatternObject;
    private InputGriddify _input;

    [Header("EDIT MODE")]
    [SerializeField] private bool editMode = false;

    [HideInInspector] public Dictionary<int, GameObject> PrefabAndId { get; set; } = new Dictionary<int, GameObject>();
    [HideInInspector] public List<Pattern> Patterns { get; set; } = new List<Pattern>();
    [SerializeField] public Dictionary<string, Dictionary<EOrientations, Coefficient>> AllowedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();
    public Dictionary<Pattern, int> Weights = new Dictionary<Pattern, int>();

    [SerializeField] public List<int> PatternsNotToReflect = new List<int>();
    [SerializeField] public List<int> PatternsNotToRotate = new List<int>();

    public Pattern ModuleMatrix { get; set; }

    public int N { get; set; } = 2;
    public int PrefabToId(GameObject prefab)
    {
        if (prefab)
        {
            foreach (KeyValuePair<int, GameObject> pair in PrefabAndId)
            {
                if (prefab == pair.Value) return pair.Key;
            }
        }

        return -1;
    }

    private void Update()
    {
        //Train();
    }

    public void Train()
    {
        InitializeData();
        AssignCoordinateToChildren();
        DefinePatterns();
        DisplayPatterns();
    }

    // Clear old data and initialize new data.
    private void InitializeData()
    {
        _input = GetComponent<InputGriddify>();

        // Initialize matrix of InputSize.
        ModuleMatrix = new Pattern(_input.inputSize);
        AllowedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();
        Patterns = new List<Pattern>();
        Weights = new Dictionary<Pattern, int>();

        GetResources();
    }

    // Assign Prefab & Id from local Unity Resources folder.
    private void GetResources()
    {
        PrefabAndId = new Dictionary<int, GameObject>();

        // Load all "WFC" Resources.
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Wfc");
        for (int i = 0; i < prefabs.Length; i++)
        {
            PrefabAndId.Add(i, prefabs[i]);
        }
    }

    private int GetPrefabIDByPrefab(GameObject prefab)
    {
        foreach (var pair in PrefabAndId)
        {
            if (pair.Value == prefab)
            {
                return pair.Key;
            }
        }

        return -1;
    }

    private void AssignCoordinateToChildren()
    {
        // Loop through all the children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            // Create int XYZ coord based on child value in input grid.
            Vector3Int coord = V3ToV3I(childTransform.localPosition);

            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject);

            // Create new Module from it's Prefab object and it's local rotation.
            ModuleMatrix.MatrixData[coord.x, coord.y, coord.z] = new Module(prefab, V3ToV3I(childTransform.localEulerAngles), V3ToV3I(childTransform.localScale));

            // Change child name to (X, Y, Z) + Prefab.Name + ID
            childTransform.name = V3ToV3I(childTransform.localPosition).ToString() + " (Bit: " + ModuleMatrix.MatrixData[coord.x, coord.y, coord.z].GenerateBit(this) + ") (" + prefab.name + ")";
        }

        // Find invalid indices
        For3(ModuleMatrix, (x, y, z) =>
        {
            if (!ModuleMatrix.Valid(x, y, z))
            {
                // Get the 0 prefab which is "Empty" and fill the matrix with "Empty" values for unassigned blocks.
                PrefabAndId.TryGetValue(0, out GameObject emptyPrefab);
                ModuleMatrix.MatrixData[x, y, z] = new Module(emptyPrefab, Vector3Int.zero, Vector3Int.one);
            }
        });
    }

    public Module CreateModuleFromBit(string bit)
    {
        // Define new Module
        Module newModule = new Module();

        // Extract bit data using the 1st char, create int by using an empty char min operator.
        int id = bit[0] - '0';

        // Get rotation from bit by 2nd char. 
        Vector3Int rot = Orientations.CharToEulerRotation(bit[1]);

        //Get scale from bit by 3rd char.
        string scale = bit[2].ToString();
        Vector3Int sca = Vector3Int.zero;

        if (scale == "N") sca = Vector3Int.one;
        if (scale == "X") sca = new Vector3Int(-1, 1, 1);
        if (scale == "Z") sca = new Vector3Int(1, 1, -1);

        newModule.Scale = sca;


        // Get prefab
        PrefabAndId.TryGetValue(id, out GameObject prefab);
        newModule.Prefab = prefab;

        // The rotationDir will automatically set itself after the Euler is set.
        newModule.RotationEuler = rot;

        // Get it's neighbour data
        AllowedData.TryGetValue(bit, out Dictionary<EOrientations, Coefficient> neighbours);
        newModule.ModuleNeighbours = neighbours;

        return newModule;
    }

    public GameObject SpawnModule(Module module, Vector3 localPosition, Transform parent)
    {
        if (module.Prefab != null)
        {
            GameObject newModule = Instantiate(module.Prefab, Vector3.zero, Quaternion.Euler(module.RotationEuler), parent);
            newModule.transform.localScale = module.Scale;
            newModule.transform.localPosition = localPosition;
            return newModule;
        }

        return null;
    }

    private void DefinePatterns()
    {
        N = _input.NValue;

        int currentPattern = 0;

        if (N > 0)
        {
            For3(_input.inputSize, N, (x, y, z) =>
            {
                if (!PatternsNotToRotate.Contains(currentPattern))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Pattern rotatedPattern = new Pattern(DefineNewPatternData(x, y, z));
                        rotatedPattern.RotateCounterClockwise(i);
                        Patterns.Add(rotatedPattern);
                    }
                }
                else
                {
                    Pattern defaultPattern = new Pattern(DefineNewPatternData(x, y, z));
                    Patterns.Add(defaultPattern);
                }

                if (!PatternsNotToReflect.Contains(currentPattern))
                {

                    for (int i = 0; i < 4; i++)
                    {
                        Pattern reflectedPattern = new Pattern(DefineNewPatternData(x, y, z));
                        reflectedPattern.RotateCounterClockwise(i);

                        if (i % 2 == 0)
                        {
                            reflectedPattern.Reflect(EOrientations.RIGHT);
                        }
                        else
                        {
                            reflectedPattern.Reflect(EOrientations.FORWARD);
                        }

                        Patterns.Add(reflectedPattern);
                    }
                }

                currentPattern++;

            });
        }


        List<Pattern> checkedPatterns = new List<Pattern>();

        int index = 0;

        foreach (Pattern pattern in Patterns)
        {
            bool bIsEqual = false;

            foreach (Pattern checkedPattern in checkedPatterns)
            {
                if (pattern.CompareBitPatterns(this, checkedPattern.GenerateBits(this)))
                {
                    bIsEqual = true;
                    break;
                }
            }

            if (!bIsEqual)
            {
                pattern.id = index;
                checkedPatterns.Add(pattern);
                index++;
            }
        }

        Patterns = checkedPatterns;

        foreach (Pattern pattern in Patterns)
        {
            pattern.BuildPropagator(this, EOrientations.NULL);

            Weights.Add(pattern, 1);
        }
    }

    private Module[,,] DefineNewPatternData(int x, int y, int z)
    {
        Module[,,] newTrainingData = new Module[N, N, N];

        For3(new Vector3Int(N, N, N), (nx, ny, nz) =>
        {
            Vector3Int coordinate = new Vector3Int(x + nx, y + ny, z + nz);

            if (ModuleMatrix.ValidCoordinate(coordinate))
            {
                newTrainingData[nx, ny, nz] = ModuleMatrix.GetDataAt(coordinate);
            }
        });
        return newTrainingData;
    }

    private void DisplayPatterns()
    {
        if (displayPatternObject)
        {
            for (int i = displayPatternObject.transform.childCount; i > 0; --i)
            {
                DestroyImmediate(displayPatternObject.transform.GetChild(0).gameObject);
            }

            int index = 0;

            foreach (Pattern pattern in Patterns)
            {
                GameObject newPattern = new GameObject("Pattern");
                //                 Bitsplay newBitsplay = newPattern.AddComponent<Bitsplay>();
                // 
                //                 newBitsplay.Training = this;
                //                 newBitsplay.Pattern = pattern;

                int val = index % 4;

                newPattern.transform.parent = displayPatternObject.transform;
                newPattern.transform.localPosition = (index * (_input.NValue + _input.NValue)) * (Vector3.left / (4 - 1)) / N + ((Vector3.up * (N + 1)) * val) + val * (Vector3.right / (4 - 1)) * N;

                For3(pattern, (x, y, z) =>
                {
                    if (pattern.MatrixData[x, y, z].Prefab != null)
                    {
                        GameObject patternData = Instantiate(pattern.MatrixData[x, y, z].Prefab, newPattern.transform);
                        patternData.transform.localScale = pattern.MatrixData[x, y, z].Scale;
                        patternData.transform.localPosition = new Vector3(x, y, z);
                        patternData.transform.localEulerAngles = pattern.MatrixData[x, y, z].RotationEuler;
                    }
                });
                index++;
            }
        }
    }
}
