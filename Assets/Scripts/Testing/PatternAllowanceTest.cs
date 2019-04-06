using System;
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
    private int _value = 1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PatternAllowanceTest patternTest = (PatternAllowanceTest)target;
        if (GUILayout.Button("Set Patterns"))
        {
            patternTest.SetPatterns();
        }

        EditorGUILayout.BeginHorizontal();
        _value = EditorGUILayout.IntField("Pattern Index: ", _value);
        if (GUILayout.Button("Spawn Pattern"))
        {
            patternTest.SpawnPattern(_value);
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Move Up", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.UP);
        }

        if (GUILayout.Button("Move Down", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.DOWN);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Move Left", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.LEFT);
        }

        if (GUILayout.Button("Move Right", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.RIGHT);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Move Forward", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.FORWARD);
        }

        if (GUILayout.Button("Move Back", GUILayout.Height(50)))
        {
            patternTest.Move(EOrientations.BACK);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Collapse coordinate", GUILayout.Height(100)))
        {
            patternTest.CollapseCoordinate();
        }
    }
}

[ExecuteInEditMode]
public class PatternAllowanceTest : SerializedMonoBehaviour
{
    private readonly List<Vector3Int> _collidingCoordinates = new List<Vector3Int>();

    private Matrix<Vector3Int> _coordinateMatrix = new Matrix<Vector3Int>();
    private Matrix<List<EOrientations>> _coordinateMatrixOrientations = new Matrix<List<EOrientations>>();

    [FormerlySerializedAs("_outputSize")] [SerializeField] private Vector3Int outputSize = new Vector3Int(6, 6, 6);

    [SerializeField] private List<Pattern> patterns;
    [SerializeField] private Pattern patternToSpawn;
    [SerializeField] private TrainingScript training;

    [SerializeField] private Dictionary<string, int> weights = new Dictionary<string, int>();

    private Matrix<bool> _wave = new Matrix<bool>();
    private Matrix<Module> _modules = new Matrix<Module>();
    private Matrix<Coefficient> _coefficients = new Matrix<Coefficient>();

    public Vector3Int CollapsingCoordinate { get; set; } = Vector3Int.zero;

    [SerializeField] private Transform displayTarget;

    private int patternIndex = 0;
    private float patternChangeTimer = 1F;

    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update += CyclePattern;
#endif
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= CyclePattern;
#endif
    }


    public void SetPatterns()
    {
        patterns = training.Patterns.ToList();
    }

    private void DisplayPattern()
    {
        if (displayTarget)
        {
            for (int i = displayTarget.childCount; i > 0; --i)
            {
                DestroyImmediate(displayTarget.GetChild(0).gameObject);
            }


            For3(_coordinateMatrix, (x, y, z) =>
            {
                Module module = training.Patterns.ToList()[patternIndex].GetDataAt(x, y, z);

                GameObject patternObject = Instantiate(module.Prefab,
                    transform.position + _coordinateMatrix.GetDataAt(x, y, z), Quaternion.Euler(module.RotationEuler),
                    displayTarget);

                patternObject.name = "PREVIEW";

                for (int i = 0; i < patternObject.transform.childCount; i++)
                {
                    Transform child = patternObject.transform.GetChild(i);

                    if (child.GetComponent<MeshRenderer>())
                    {
                        MeshRenderer mr = child.GetComponent<MeshRenderer>();

                        var tempMaterial = new Material(Shader.Find("Transparent/Diffuse"));
                        tempMaterial.color = new Color(1, 0, 1, 0.25F);

                        mr.sharedMaterial = tempMaterial;
                    }
                }
            });
        }
    }

    private void ChangePattern()
    {
        if (displayTarget)
        {
            if (patternIndex + 1 > training.Patterns.Count() - 1)
            {
                patternIndex = 0;
            }
            else
            {
                patternIndex++;
            }

            DisplayPattern();
        }

    }

    private void CyclePattern()
    {
        if (displayTarget)
        {
            patternChangeTimer -= Time.deltaTime;

            if (patternChangeTimer < 0)
            {
                ChangePattern();
                patternChangeTimer = 5F;
            }
        }
    }

    private void InitWave()
    {
        _wave = new Matrix<bool>(outputSize);
        _modules = new Matrix<Module>(outputSize);
        _coefficients = new Matrix<Coefficient>(outputSize);

        _collidingCoordinates.Clear();
        weights.Clear();

        if (training)
        {

            foreach (KeyValuePair<string, List<Possibility>> pair in training.NeighbourPossibilitiesPerBit)
            {
                weights.Add(pair.Key, 100);
            }

            For3(_wave, (x, y, z) =>
            {
                _wave.MatrixData[x, y, z] = true;
                _coefficients.MatrixData[x, y, z] = new Coefficient(training.NeighbourPossibilitiesPerBit);
            });

            for (int i = transform.childCount; i > 0; --i)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }

    public void SpawnPattern(int index)
    {
        InitWave();

        if (!(index >= 0 && index < patterns.Count))
        {
            index = 0;
        }

        if (patterns.Count > 0)
        {

            Pattern selectedPattern = patterns[index];
            GameObject newPattern = new GameObject("Pattern");
            newPattern.transform.parent = transform;
            newPattern.transform.localPosition = Vector3.zero;

 
            For3(selectedPattern, (x, y, z) =>
            {
                GameObject patternData = Instantiate(
                    selectedPattern.MatrixData[x, y, z].Prefab,
                    new Vector3(x, y, z),
                    Quaternion.Euler(selectedPattern.MatrixData[x, y, z].RotationEuler),
                    newPattern.transform
                );

                Vector3Int coords = new Vector3Int(2 + x, y, 2+  z);

                patternData.transform.localPosition = coords;
                _wave.MatrixData[coords.x, coords.y, coords.z] = false;

                _modules.MatrixData[coords.x, coords.y, coords.z] =
                    selectedPattern.MatrixData[x, y, z];

                Propagate(coords);
            });

            InitSelectionMatrix();
        }
    }

    private void Propagate(Vector3Int coords)
    {
        _coefficients.MatrixData[coords.x, coords.y, coords.z] = new Coefficient(new Dictionary<string, List<Possibility>>());
        
        foreach (Vector3Int direction in Orientations.OrientationUnitVectors.Values)
        {
            Vector3Int neighbourCoord = coords + direction;

            if (_wave.ValidCoordinate(neighbourCoord))
            {
                if (_wave.GetDataAt(neighbourCoord))
                {
                    Dictionary<string, List<Possibility>> adjustedPossibilites =
                        new Dictionary<string, List<Possibility>>();
                    string bit = _modules.GetDataAt(coords).GenerateBit(training);

                    adjustedPossibilites.Add(bit, CheckCompatibilities(bit));

                    _coefficients.MatrixData[neighbourCoord.x, neighbourCoord.y, neighbourCoord.z].Keys =
                        adjustedPossibilites;
                }
            }
        }
    }



    private List<Possibility> CheckCompatibilities(string insertBit)
    {
        training.NeighbourPossibilitiesPerBit.TryGetValue(insertBit, out List<Possibility> possibilities);
        return possibilities;
    }

    public void InitSelectionMatrix()
    {
        Vector3Int nSize = new Vector3Int(training.N, training.N, training.N);
        _coordinateMatrix = new Matrix<Vector3Int>(nSize);
        _coordinateMatrixOrientations = new Matrix<List<EOrientations>>(nSize);

        For3(_coordinateMatrix, (x, y, z) =>
        {
            _coordinateMatrix.MatrixData[x, y, z] = new Vector3Int(x, y, z);
            _coordinateMatrixOrientations.MatrixData[x, y, z] = new List<EOrientations>();
        });
    }

    private Vector3Int MinEntropyCoords()
    {
        float minEntropy = 0;
        Vector3Int minEntropyCoords = new Vector3Int();

        System.Random random = new System.Random();

        For3(outputSize, (x, y, z) =>
        {
            Vector3Int currentCoordinates = new Vector3Int(x, y, z);
            if (_coefficients.MatrixData[x, y, z].Keys.Any())
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

        foreach (KeyValuePair<string, List<Possibility>> pair in _coefficients.MatrixData[currentCoordinates.x, currentCoordinates.y, currentCoordinates.z].Keys)
        {
            weights.TryGetValue(pair.Key, out int weight);

            sumOfWeights += weight;
            sumOfWeightsLogWeights += weight * (float)Math.Log(weight);
        }
        return (float)Math.Log(sumOfWeights) - (sumOfWeightsLogWeights / sumOfWeights);
    }

    public void Move(EOrientations orientation)
    {
        _collidingCoordinates.Clear();

        For3(_coordinateMatrix, (x, y, z) =>
        {
            _coordinateMatrix.MatrixData[x, y, z] += Orientations.ToUnitVector(orientation);
            _coordinateMatrixOrientations.MatrixData[x, y, z] = new List<EOrientations>();

            Vector3Int coordinate = _coordinateMatrix.MatrixData[x, y, z];
            if (_wave.GetDataAt(coordinate))
            {
                return;
            }

            _collidingCoordinates.Add(coordinate);
            foreach (Vector3Int direction in Orientations.OrientationUnitVectors.Values)
            {
                EOrientations dirOrientation = Orientations.DirToOrientation(direction);

                if (dirOrientation == EOrientations.UP || dirOrientation == EOrientations.DOWN)
                {
                    continue;
                }

                if (_coordinateMatrix.Contains(coordinate + direction) && _wave.GetDataAt(coordinate + direction))
                {
                    _coordinateMatrixOrientations.MatrixData[x, y, z].Add(dirOrientation);
                }
            }
        });

        DisplayPattern();
    }
    public void CollapseCoordinate()
    {
        List<Vector3Int> collapseableCoordinates = new List<Vector3Int>();

        For3(outputSize, (x, y, z) =>
        {
            Vector3Int coord = new Vector3Int(x, y, z);

            if (_coordinateMatrix.Contains(coord, out Vector3Int localCoord))
            {
                List<EOrientations> orientations = _coordinateMatrixOrientations.GetDataAt(localCoord);
                foreach (EOrientations orientation in orientations)
                {
                    collapseableCoordinates.Add(coord + Orientations.ToUnitVector(orientation));
                }
            }
        });

        if (!collapseableCoordinates.Any()) return;
        collapseableCoordinates.RemoveAll((s => s.y > collapseableCoordinates.Min(v => v.y)));
        Collapse(collapseableCoordinates.PickRandom());
    }

    private void CheckNeighbours(Vector3Int coordToCheck, Vector3Int cameFromDirection)
    {
        List<Vector3Int> coordinatesToCheck = new List<Vector3Int>();

        if (!_wave.MatrixData[coordToCheck.x, coordToCheck.y, coordToCheck.z])
        {
            coordinatesToCheck.Add(coordToCheck);
        }

        foreach (Vector3Int direction in Orientations.OrientationUnitVectors.Values)
        {
            Vector3Int coordNeighbour = coordToCheck + direction;
            if (!_wave.MatrixData[coordNeighbour.x, coordNeighbour.y, coordNeighbour.z])
            {
                coordinatesToCheck.Add(coordNeighbour);
            }
        }

        foreach (Vector3Int coordinateToCheck in coordinatesToCheck)
        {

        }
    }

    private void Collapse(Vector3Int coord)
    {
        Dictionary<Vector3Int, bool> neighbourAllowance = new Dictionary<Vector3Int, bool>();

        //int key = bit[0] - '0';
        // Vector3Int rot = Orientations.ReturnRotationEulerFromChar(bit[1]);


        //if (training.PrefabAndId.TryGetValue(key, out GameObject prefab))
        //{
        //     GameObject gameObject = Instantiate(prefab, transform.position + coord, Quaternion.Euler(rot), transform);
        //     gameObject.name = "TEST";
        // }

        _wave.MatrixData[coord.x, coord.y, coord.z] = false;
        _modules.MatrixData[coord.x, coord.y, coord.z] = new Module();
        //_coefficients.MatrixData[coord.x,coord.y,coord.z] = new Coefficient(new Dictionary<string, List<Possibility>>());

        //InitSelectionMatrix();
        Propagate(coord);
        Move(EOrientations.NULL);


    }

    private void OnDrawGizmos()
    {
        try
        {
            Color redColor = Color.red;
            redColor.a = 0.75F;

            Color greenColor = Color.green;
            greenColor.a = 0.15F;

            Color yellowColor = Color.yellow;
            yellowColor.a = .5F;

            Color magentaColor = Color.magenta;
            magentaColor.a = 0.75F;

            For3(outputSize, (x, y, z) =>
            {
                Vector3Int coord = new Vector3Int(x, y, z);

                Gizmos.color = _wave.MatrixData[x, y, z] ? greenColor : redColor;

                if (coord == MinEntropyCoords())
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.35F);

                }
                else
                {

                    Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.25F);

                    if (_coordinateMatrix.Contains(coord, out Vector3Int localCoord))
                    {
                        List<EOrientations> orientations = _coordinateMatrixOrientations.GetDataAt(localCoord);
                        foreach (EOrientations orientation in orientations)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(
                                transform.position + coord,
                                transform.position + coord + Orientations.ToUnitVector(orientation)
                            );

                            Handles.Label(
                                transform.position + coord +
                                (((Vector3)Orientations.ToUnitVector(orientation)) / 2),
                                orientation.ToString()
                            );
                        }
                    }
                }
            });


            Gizmos.color = magentaColor;

            For3(_coordinateMatrix, (x, y, z) =>
            {
                Gizmos.DrawSphere(transform.position + _coordinateMatrix.MatrixData[x, y, z], 0.25F);
            });
        }
        catch (Exception)
        {
            SetPatterns();
            SpawnPattern(0);
        }
    }
}