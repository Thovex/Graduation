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

    [Header("EDIT MODE")]
    [SerializeField] private bool editMode = false;

    [SerializeField] public Dictionary<int, GameObject> PrefabAndId { get; set; } = new Dictionary<int, GameObject>();
    [SerializeField] public HashSet<Pattern> Patterns { get; set; } = new HashSet<Pattern>();
    [SerializeField] public Dictionary<string, Dictionary<EOrientations, Coefficient>> AllowedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();

    public Matrix3<Module> ModuleMatrix { get; set; }

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

    // Clear old data and initialize new data.
    private void InitializeData()
    {
        _input = GetComponent<InputGriddify>();

        // Initialize matrix of InputSize.
        ModuleMatrix = new Matrix3<Module>(_input.inputSize);
        AllowedData = new Dictionary<string, Dictionary<EOrientations, Coefficient>>();
        Patterns = new HashSet<Pattern>();

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

        if (!editMode)
        {
            CalculateModuleNeighbours();
        }
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
            Pattern ModulePattern = new Pattern(ModuleMatrix.MatrixData);

            For3(ModulePattern, (x, y, z) =>
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

            ModulePattern.RotateCounterClockwise(1);
        }

        // UpdateInputComponents();

        CombineSimilarData(similarModulesByBit);
    }

    private void CombineSimilarData(Dictionary<string, List<Module>> similarModulesByBit)
    {
        // Take our dictionary where string = bit and list<module> is each list of similar modules to this bit.
        // Loop through dictionary.
        for (int i = 0; i < similarModulesByBit.Count; i++)
        {
            // Create our List of List's.
            var ListList = similarModulesByBit.Values.ToList();

            // Get it's List of List<Module> and convert it's values to a List so we'll end up with just a List
            // of Modules.
            for (int j = 0; j < ListList.Count; j++)
            {
                // Create a new dictionary for each bit to append similar values to.
                Dictionary<EOrientations, Coefficient> similarDict = new Dictionary<EOrientations, Coefficient>();

                // Get list of Modules per Bit
                var List = ListList[j].ToList();

                // Loop through this list
                for (int k = 0; k < List.Count; k++)
                {
                    // Get it's module's module neighours. We want to append this to our similarDict.
                    foreach (var neighbourPair in List[k].ModuleNeighbours)
                    {
                        // Loop through the different orientations with coefficients.
                        if (similarDict.TryGetValue(neighbourPair.Key, out Coefficient currentValue))
                        {
                            // Initialize coefficient if it's not initialized yet (odd problem with struct?)
                            if (currentValue.AllowedBits == null)
                            {
                                currentValue.Initialize();
                            }

                            // Check which bits were allowed in this module from the training
                            foreach (string bit in neighbourPair.Value.AllowedBits)
                            {
                                // We don't want a "null" bit to enter this list (outside the training area)
                                if (bit != "null")
                                {
                                    // Append it's new bits to the allowed bits.
                                    currentValue.AllowedBits.Add(bit);
                                }
                            }
                            // Update dictionary's value with new allowed bits.
                            similarDict[neighbourPair.Key] = currentValue;
                        }
                        else
                        {
                            // Looks like this coefficient didn't exist yet. We make a new one.
                            Coefficient newCoefficient = new Coefficient();
                            newCoefficient.Initialize();

                            // Check which bits were allowed in this module from the training
                            foreach (string bit in neighbourPair.Value.AllowedBits)
                            {
                                // We don't want a "null" bit to enter this list (outside the training area)
                                if (bit != "null")
                                {
                                    // Append it's new bits to the allowed bits.
                                    newCoefficient.AllowedBits.Add(bit);
                                }
                            }
                            // Update dictionary's value with new allowed bits.
                            similarDict[neighbourPair.Key] = newCoefficient;
                        }
                    }
                }

                // Now we've finished that we want to make sure we add all finished data to the allowed data dictionary.
                // We will do this by looping through the modules so we have all bit values. This has no "null" values.
                for (int k = 0; k < List.Count; k++)
                {
                    // Create it's bit
                    string bit = List[k].GenerateBit(this);

                    // Let's see if it's already in the data.
                    if (!AllowedData.ContainsKey(bit))
                    {
                        // It's not, so we add it.
                        AllowedData.Add(bit, similarDict);
                    }
                }
            }
        }

        UpdateInputComponents();
    }

    private void UpdateInputComponents()
    {
        // Loop through all the children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);

            // Create int XYZ coord based on child value in input grid.
            Vector3Int coord = V3ToV3I(childTransform.localPosition);

            // Get the module prototype (to display to the developer what each module is)
            ModulePrototype modulePrototype = childTransform.GetComponent<ModulePrototype>();

            // Set it's bit.
            modulePrototype.bit = ModuleMatrix.MatrixData[coord.x, coord.y, coord.z].GenerateBit(this);

            // See if we can extract data from the new Allowed Data Dictionary.
            AllowedData.TryGetValue(modulePrototype.bit, out Dictionary<EOrientations, Coefficient> allowedDictionary);

            // Set our data to the new data.
            ModuleMatrix.MatrixData[coord.x, coord.y, coord.z].ModuleNeighbours = allowedDictionary;
            modulePrototype.CoefficientDict = ModuleMatrix.MatrixData[coord.x, coord.y, coord.z].ModuleNeighbours;

            // Display data to developer.
            modulePrototype.CalculateDisplay();
        }
    }

    public Module CreateModuleFromBit(string bit)
    {
        // Define new Module
        Module newModule = new Module();

        // Extract bit data using the 1st char, create int by using an empty char min operator.
        int id = bit[0] - '0';

        // Get rotation from bit by 2nd char. 
        Vector3Int rot = Orientations.CharToEulerRotation(bit[1]);

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
            newModule.transform.localPosition = localPosition;
            return newModule;
        }

        return null;
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
                    Pattern newPattern = new Pattern(newTrainingData);
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
                            Pattern rotatedPattern = new Pattern(newTrainingData);
                            rotatedPattern.RotateCounterClockwise(i);
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
