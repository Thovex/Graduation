using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
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
            training.TranslatePrefabsToId();
        }
        if (GUILayout.Button("For2"))
        {
        }
    }
}
[ExecuteInEditMode]
public class TrainingScript : SerializedMonoBehaviour
{
    [SerializeField] private GameObject displayPatternObject;
    private InputGriddify _input;

    public Dictionary<Vector3Int, Module> ChildrenByCoordinate { get; set; } = new Dictionary<Vector3Int, Module>();
    [SerializeField] public Dictionary<int, GameObject> PrefabAndId { get; set; } = new Dictionary<int, GameObject>();
    public HashSet<Pattern> Patterns { get; set; } = new HashSet<Pattern>();
    public Matrix<Module> ModuleMatrix { get; set; }
    [SerializeField] public Dictionary<string, List<Possibility>> NeighbourPossibilitiesPerBit { get; set; } = new Dictionary<string, List<Possibility>>();

    public Dictionary<Pattern, Matrix<string>> PatternBits { get; set; } = new Dictionary<Pattern, Matrix<string>>();
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
        TranslatePrefabsToId();
    }
    public void TranslatePrefabsToId()
    {
        try
        {
            ClearPreviousData();
            GetResources();
            _input = GetComponent<InputGriddify>();
            AssignCoordinateToChildren();
            CalculateNeighbours();
            InitializeMatrix();
            DefinePatterns();
            DisplayPatterns();
            PatternToBits();
            FillNeighbourPossibilities();
        }
        catch (Exception) { }
    }
    private void ClearPreviousData()
    {
        ChildrenByCoordinate = new Dictionary<Vector3Int, Module>();
        PrefabAndId = new Dictionary<int, GameObject>();
        ModuleMatrix = new Matrix<Module>(Vector3Int.zero);
        Patterns = new HashSet<Pattern>();
        NeighbourPossibilitiesPerBit = new Dictionary<string, List<Possibility>>();
        PatternBits = new Dictionary<Pattern, Matrix<string>>();
    }
    private void GetResources()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Wfc");
        for (int i = 0; i < prefabs.Length; i++)
        {
            PrefabAndId.Add(i, prefabs[i]);
        }
    }
    private void AssignCoordinateToChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            ChildrenByCoordinate.Add(
                V3ToV3I(childTransform.localPosition),
                new Module(
                    PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject),
                    V3ToV3I(childTransform.localEulerAngles)
                )
            );
            childTransform.name = V3ToV3I(childTransform.localPosition).ToString() + " " + ((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject)).name;
        }
    }
    private void CalculateNeighbours()
    {
        Dictionary<Vector3Int, Module> childrenByCoordinateWithNeighbours = new Dictionary<Vector3Int, Module>();
        foreach (KeyValuePair<Vector3Int, Module> pair in ChildrenByCoordinate)
        {
            List<OrientationModule> neighbours = new List<OrientationModule>();
            foreach (Vector3Int orientation in Orientations.OrientationUnitVectors.Values)
            {
                Vector3Int neighbourCoordinate = pair.Key + orientation;
                if (ChildrenByCoordinate.ContainsKey(neighbourCoordinate))
                {
                    if (ChildrenByCoordinate.TryGetValue(neighbourCoordinate, out Module neighbourModule))
                    {
                        neighbours.Add(new OrientationModule(Orientations.DirToOrientation(orientation), neighbourModule));
                    }
                }
                else
                {
                    if (PrefabAndId.TryGetValue(0, out GameObject emptyPrefab))
                    {
                        neighbours.Add(new OrientationModule(Orientations.DirToOrientation(orientation), new Module(emptyPrefab, neighbourCoordinate)));
                    }
                }
            }
            Module updatedModule = pair.Value;
            updatedModule.ModuleNeighbours = neighbours;
            childrenByCoordinateWithNeighbours.Add(pair.Key, updatedModule);
        }
        ChildrenByCoordinate = childrenByCoordinateWithNeighbours;
    }
    private void InitializeMatrix()
    {
        ModuleMatrix = new Matrix<Module>(_input.inputSize);
        For3(ModuleMatrix, (x, y, z) =>
        {
            if (ChildrenByCoordinate.TryGetValue(new Vector3Int(x, y, z), out Module module))
            {
                ModuleMatrix.MatrixData[x, y, z] = module;
            }
        });
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
                    if (ChildrenByCoordinate.TryGetValue(new Vector3Int(x + nx, y + ny, z + nz), out Module module))
                    {
                        newTrainingData[nx, ny, nz] = module;
                        bIsNull = false;
                    }
                    else
                    {
                        if (PrefabAndId.TryGetValue(0, out GameObject emptyPrefab))
                        {
                            newTrainingData[nx, ny, nz] = new Module(emptyPrefab, Vector3Int.zero);
                        }
                    }
                });
                if (!bIsNull)
                {
                    Pattern newPattern = new Pattern(N, newTrainingData, new Vector3Int(x, y, z));
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
                            Pattern rotatedPattern = new Pattern(N, newTrainingData, new Vector3Int(x, y, z));
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
                Module[,,] data = pattern.MatrixData;
                For3(pattern, (x, y, z) =>
                {
                    if (data[x, y, z].Prefab != null)
                    {
                        GameObject patternData = Instantiate(data[x, y, z].Prefab, newPattern.transform);
                        patternData.transform.localPosition = new Vector3(x, y, z);
                        patternData.transform.localEulerAngles = data[x, y, z].RotationEuler;
                    }
                });
                index++;
            }
        }
    }
    private void PatternToBits()
    {
        foreach (Pattern pattern in Patterns)
        {
            Matrix<string> patternInBits = new Matrix<string>(new Vector3Int(pattern.SizeX, pattern.SizeY, pattern.SizeZ));
            For3(pattern, (x, y, z) =>
            {
                patternInBits.MatrixData[x, y, z] = pattern.MatrixData[x, y, z].GenerateBit(this);
            });
            PatternBits.Add(pattern, patternInBits);
        }
    }
    public bool GetPatternByBit(string bit, out List<Pattern> outPattern)
    {
        // need to be sure its on ze bottom and shit 
        outPattern = new List<Pattern>();
        List<Pattern> tempPatterns = new List<Pattern>();
        foreach (Pattern pattern in Patterns)
        {
            if (PatternBits.TryGetValue(pattern, out Matrix<string> patternBits))
            {
                For3(pattern, (x, y, z) =>
                {
                    if (pattern.MatrixData[x, y, z].GenerateBit(this) == patternBits.MatrixData[x, y, z])
                    {
                        tempPatterns.Add(pattern);
                    }
                });
            }
        }
        outPattern = tempPatterns;
        if (outPattern.Count > 0)
        {
            return true;
        }
        return false;
    }
    private void FillNeighbourPossibilities()
    {
        foreach (KeyValuePair<Vector3Int, Module> pair in ChildrenByCoordinate)
        {
            string bit = pair.Value.GenerateBit(this);
            if (!NeighbourPossibilitiesPerBit.ContainsKey(bit))
            {
                List<Possibility> newPossibilities = new List<Possibility>();
                foreach (Vector3Int orientationVector in Orientations.OrientationUnitVectors.Values)
                {
                    newPossibilities.Add(new Possibility(Orientations.DirToOrientation(orientationVector), new HashSet<string>()));
                }
                NeighbourPossibilitiesPerBit.Add(bit, newPossibilities);
            }
            if (NeighbourPossibilitiesPerBit.TryGetValue(bit, out List<Possibility> currentPossibilities))
            {
                foreach (OrientationModule orientationModule in pair.Value.ModuleNeighbours)
                {
                    foreach (Possibility possibilities in currentPossibilities)
                    {
                        if (orientationModule.Orientation == possibilities.Orientation)
                        {
                            possibilities.Possibilities.Add(orientationModule.NeighbourModule.GenerateBit(this));
                        }
                    }
                }
            }
            NeighbourPossibilitiesPerBit[bit] = currentPossibilities;
        }
    }
}
