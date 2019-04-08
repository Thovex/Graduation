using System;
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
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
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

    [SerializeField] public Dictionary<int, GameObject> PrefabAndId { get; set; } = new Dictionary<int, GameObject>();
    [SerializeField] public HashSet<Pattern> Patterns { get; set; } = new HashSet<Pattern>();
    [SerializeField] private Dictionary<string, Dictionary<EOrientations, Coefficient>> AllowedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();

    [SerializeField] private Dictionary<Vector3Int, Module> TESTONLY_ModuleMatrix = new Dictionary<Vector3Int, Module>();
    public Matrix<Module> ModuleMatrix { get; set; }

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
        Train();
    }

    public void Train()
    {
        InitializeData();
        DefinePatterns();
        DisplayPatterns();

    }

    private void UpdateInputComponents()
    {
        // Loop through all the children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            // Create int XYZ coord based on child value in input grid.
            Vector3Int coord = V3ToV3I(childTransform.localPosition);

            ModulePrototype modulePrototype = childTransform.GetComponent<ModulePrototype>();
            modulePrototype.CoefficientDict = ModuleMatrix.MatrixData[coord.x, coord.y, coord.z].ModuleNeighbours;

            modulePrototype.CalculateDisplay();
        }
    }

    // Clear old data and initialize new data.
    private void InitializeData()
    {
        _input = GetComponent<InputGriddify>();

        // Initialize matrix of InputSize.
        ModuleMatrix = new Matrix<Module>(_input.inputSize);
        AllowedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();
        Patterns = new HashSet<Pattern>();

        TESTONLY_ModuleMatrix = new Dictionary<Vector3Int, Module>();

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

        AssignCoordinateToChildren();
    }
    private void AssignCoordinateToChildren()
    {
        // Loop through all the children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            // Create int XYZ coord based on child value in input grid.
            Vector3Int coord = V3ToV3I(childTransform.localPosition);

            // Create new Module from it's Prefab object and it's local rotation.
            ModuleMatrix.MatrixData[coord.x, coord.y, coord.z] = new Module(PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject), V3ToV3I(childTransform.localEulerAngles));

            // Change child name to (X, Y, Z) + Prefab.Name
            childTransform.name = V3ToV3I(childTransform.localPosition).ToString() + " " + (PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject)).name;
        }

        // Find invalid indices
        For3(ModuleMatrix, (x, y, z) =>
        {
            if (!ModuleMatrix.Valid(x, y, z))
            {
                // Get the 0 prefab which is "Empty" and fill the matrix with "Empty" values for unassigned blocks.
                PrefabAndId.TryGetValue(0, out GameObject emptyPrefab);
                ModuleMatrix.MatrixData[x, y, z] = new Module(emptyPrefab, Vector3Int.zero);
            }
        });

        CalculateModuleNeighbours();
    }

    private void CalculateModuleNeighbours()
    {
        For3(ModuleMatrix, (x, y, z) =>
        {
            // Get the module we want to create neighbours for and create a current coordinate
            Vector3Int currentMatrixCoordinate = new Vector3Int(x, y, z);
            Module designatedModule = ModuleMatrix.GetDataAt(currentMatrixCoordinate);

            // Loop through each orientation in the 3D matrix.
            foreach (Vector3Int orientationDir in Orientations.OrientationUnitVectors.Values)
            {
                // Calculate orientation based on Vector
                EOrientations orientation = Orientations.DirToOrientation(orientationDir);

                // Get neighbour coordinate 
                Vector3Int neighbourCoordinate = currentMatrixCoordinate + orientationDir;

                // If this coordinate exists, meaning we're still inside our matrix.
                if (ModuleMatrix.ValidCoordinate(neighbourCoordinate))
                {
                    Module neighbourModule = ModuleMatrix.GetDataAt(neighbourCoordinate);

                    // Retrieve designated module's neighbour list
                    Dictionary<EOrientations, Coefficient> designatedModuleNeighbours = designatedModule.ModuleNeighbours;

                    // Find orientation and coefficient HashSet
                    designatedModuleNeighbours.TryGetValue(orientation, out Coefficient coefficient);

                    if (coefficient.AllowedBits == null)
                    {
                        coefficient.Initialize();
                    }

                    // Add this neighbour to it.
                    coefficient.AllowedBits.Add(neighbourModule.GenerateBit(this));

                    // Update dictionary
                    designatedModuleNeighbours[orientation] = coefficient;

                    // Overwrite old dictionary with new updated dictionary
                    designatedModule.ModuleNeighbours = designatedModuleNeighbours;

                }
            }

            // TEST ONLY - purposes.
            if (ModuleMatrix.GetDataAt(currentMatrixCoordinate).Prefab != PrefabAndId[0])
            {
                TESTONLY_ModuleMatrix.Add(currentMatrixCoordinate, ModuleMatrix.GetDataAt(currentMatrixCoordinate));
            }
        });

        FetchSimilarModuleData();

        //UpdateInputComponents();
    }

    private void FetchSimilarModuleData()
    {
        // Define dictionary which can hold a bit (based on Module) and save in a List
        // those specific modules. So only the same modules get collected together.
        Dictionary<string, List<Module>> similarModulesByBit = new Dictionary<string, List<Module>>();

        // We want to do this function 4 times in total. Because we will rotate our data
        // to get all results (similar to rotating the patterns).

        for (int i = 0; i < 4; i++)
        {
            For3(ModuleMatrix, (x, y, z) =>
            {
                // Generate the module's bit.
                string bit = ModuleMatrix.GetDataAt(x, y, z).GenerateBit(this);

                // Check in the dictionary if this specific bit is already added.
                if (similarModulesByBit.ContainsKey(bit))
                {
                    // Get current lists thats available if it already was in dictionary.
                    similarModulesByBit.TryGetValue(bit, out List<Module> similarModules);

                    // Add this module to similar ones.
                    similarModules.Add(ModuleMatrix.GetDataAt(x, y, z));
                    similarModulesByBit[bit] = similarModules;
                }
                else
                {
                    // Looks like this bit is not in the dictionary yet so we will create a new
                    // list for this specific bit.
                    List<Module> newSimilarModules = new List<Module>();

                    // Append itself to the list and add to dictionary.
                    newSimilarModules.Add(ModuleMatrix.GetDataAt(x, y, z));
                    similarModulesByBit.Add(bit, newSimilarModules);
                }
            });

            // Rotate input matrix.
            ModuleMatrix.RotatePatternCounterClockwise(1);
        }
        // UpdateInputComponents();
    }

    private void CombineSimilarData(Dictionary<string, List<Module>> similarModulesByBit)
    {
        Dictionary<string, Dictionary<EOrientations, Coefficient>> newCombinedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();

        foreach (var similarModulesPair in similarModulesByBit)
        {
            // TODO combine
           // similarModulesPair.Value;
        }

    }

    private void DefinePatterns()
    {
        N = _input.NValue;
        if (N > 0)
        {
            For3(_input.inputSize, N, (x, y, z) =>
            {
                Module[,,] newTrainingData = new Module[N, N, N];
                bool bIsNull = true;
                For3(new Vector3Int(N, N, N), (nx, ny, nz) =>
                {
                    Vector3Int coordinate = new Vector3Int(x + nx, y + ny, z + nz);

                    if (ModuleMatrix.ValidCoordinate(coordinate))
                    {
                        newTrainingData[nx, ny, nz] = ModuleMatrix.GetDataAt(coordinate);
                        bIsNull = false;
                    }
                });
                if (!bIsNull)
                {
                    Pattern newPattern = new Pattern(N, newTrainingData);
                    bool isEqual = false;
                    foreach (Pattern pattern in Patterns)
                    {
                        if (newPattern.IsEqualToMatrix(pattern))
                        {
                            isEqual = true;
                        }
                    }
                    if (!isEqual)
                    {
                        Patterns.Add(newPattern);
                        for (int i = 1; i < 4; i++)
                        {
                            Pattern rotatedPattern = new Pattern(N, newTrainingData);
                            rotatedPattern.RotatePatternCounterClockwise(i);
                            Patterns.Add(rotatedPattern);
                        }
                    }
                }
            });
        }
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
                Bitsplay newBitsplay = newPattern.AddComponent<Bitsplay>();

                newBitsplay.Training = this;
                newBitsplay.Pattern = pattern;
                newPattern.transform.localPosition = Vector3.zero + (index * (_input.NValue + _input.NValue)) * Vector3.left;
                newPattern.transform.parent = displayPatternObject.transform;

                For3(pattern, (x, y, z) =>
                {
                    if (pattern.MatrixData[x, y, z].Prefab != null)
                    {
                        GameObject patternData = Instantiate(pattern.MatrixData[x, y, z].Prefab, newPattern.transform);
                        patternData.transform.localPosition = new Vector3(x, y, z);
                        patternData.transform.localEulerAngles = pattern.MatrixData[x, y, z].RotationEuler;
                    }
                });
                index++;
            }
        }
    }
}
