using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using static Thovex.Utility;
using Random = System.Random;

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

public class PatternAllowanceTest : SerializedMonoBehaviour
{
    private string _bitToSpawn = "";
    private readonly List<Vector3Int> _collidingCoordinates = new List<Vector3Int>();

    private Matrix<Vector3Int> _coordinateMatrix = new Matrix<Vector3Int>();
    private Matrix<List<EOrientations>> _coordinateMatrixOrientations = new Matrix<List<EOrientations>>();
    private Matrix<Module> _modules = new Matrix<Module>();

    [FormerlySerializedAs("_outputSize")] [SerializeField] private Vector3Int outputSize = new Vector3Int(6, 6, 6);

    [SerializeField] private List<Pattern> patterns;
    [SerializeField] private Pattern patternToSpawn;
    [SerializeField] private TrainingScript training;

    private Matrix<bool> _wave = new Matrix<bool>();

    public Vector3Int CollapsingCoordinate { get; set; } = Vector3Int.zero;

    public void SetPatterns()
    {
        patterns = training.Patterns.ToList();
    }

    private void InitWave()
    {
        _wave = new Matrix<bool>(outputSize);
        _modules = new Matrix<Module>(outputSize);
        _collidingCoordinates.Clear();


        For3(_wave, (x, y, z) => { _wave.MatrixData[x, y, z] = true; });

        for (int i = transform.childCount; i > 0; --i)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    public void SpawnPattern(int index)
    {
        InitWave();

        if (!(index >= 0 && index < patterns.Count))
        {
            index = 0;
        }

        Pattern selectedPattern = patterns[index];
        GameObject newPattern = new GameObject("Pattern");
        newPattern.transform.parent = transform;
        newPattern.transform.localPosition = Vector3.zero;

        if (patterns.Count > 0)
        {
            For3(selectedPattern, (x, y, z) =>
            {
                GameObject patternData = Instantiate(
                    selectedPattern.MatrixData[x, y, z].Prefab,
                    new Vector3(x, y, z),
                    Quaternion.Euler(selectedPattern.MatrixData[x, y, z].RotationEuler),
                    newPattern.transform
                );

                patternData.transform.localPosition = new Vector3(outputSize.x / 3 + x, y, outputSize.z / 3 + z);
                _wave.MatrixData[outputSize.x / 3 + x, y, outputSize.z / 3 + z] = false;
                _modules.MatrixData[outputSize.x / 3 + x, y, outputSize.z / 3 + z] =
                    selectedPattern.MatrixData[x, y, z];
            });

            InitSelectionMatrix();
        }
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

    public void Move(EOrientations orientation)
    {


        For3(_coordinateMatrix, (x, y, z) =>
        {
            switch (orientation)
            {
                case EOrientations.FORWARD:
                    _coordinateMatrix.MatrixData[x, y, z] += V3ToV3I(Vector3.forward);
                    break;
                case EOrientations.BACK:
                    _coordinateMatrix.MatrixData[x, y, z] += V3ToV3I(Vector3.back);
                    break;
                case EOrientations.RIGHT:
                    _coordinateMatrix.MatrixData[x, y, z] += Vector3Int.right;
                    break;
                case EOrientations.LEFT:
                    _coordinateMatrix.MatrixData[x, y, z] += Vector3Int.left;
                    break;
                case EOrientations.UP:
                    _coordinateMatrix.MatrixData[x, y, z] += Vector3Int.up;
                    break;
                case EOrientations.DOWN:
                    _coordinateMatrix.MatrixData[x, y, z] += Vector3Int.down;
                    break;
                case EOrientations.NULL:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
            }
        });

        _collidingCoordinates.Clear();

        For3(_coordinateMatrix, (x, y, z) =>
        {
            _coordinateMatrixOrientations.MatrixData[x, y, z] = new List<EOrientations>();

            Vector3Int coordinate = _coordinateMatrix.MatrixData[x, y, z];
            if (!_wave.GetDataAt(coordinate))
            {
                string bit = _modules.GetDataAt(coordinate).GenerateBit(training);

                _collidingCoordinates.Add(coordinate);
                foreach (Vector3Int direction in Orientations.Dirs)
                {
                    EOrientations dirOrientation = Orientations.ReturnOrientationVal(direction);

                    if (dirOrientation != EOrientations.UP && dirOrientation != EOrientations.DOWN)
                    {
                        if (_coordinateMatrix.Contains(coordinate + direction) && _wave.GetDataAt(coordinate + direction))
                        {
                            _coordinateMatrixOrientations.MatrixData[x, y, z].Add(dirOrientation);
                        }
                    }
                }
            }
        });
    }

    public void CollapseCoordinate()
    {
        For3(outputSize, (x, y, z) =>
        {
            Vector3Int coord = new Vector3Int(x, y, z);

            if (_coordinateMatrix.Contains(coord, out Vector3Int localCoord))
            {
                List<EOrientations> orientations = _coordinateMatrixOrientations.GetDataAt(localCoord);
                foreach (EOrientations orientation in orientations)
                {
                    string bit = _modules.GetDataAt(coord).GenerateBit(training);

                    List<Possibility> outPossibilities = new List<Possibility>();

                    if (training.NeighbourPossibilitiesPerBit.TryGetValue(bit, out outPossibilities))
                    {
                        foreach (Possibility possibility in outPossibilities)
                        {
                            if (possibility.Orientation == orientation)
                            {
                                List<string> possibilitiesList = possibility.Possibilities.ToList();
                                string selectedBit =
                                    possibilitiesList[
                                        UnityEngine.Random.Range(0, possibility.Possibilities.Count() - 1)];
                                Collapse(coord + Orientations.ReturnDirectionVal(orientation), selectedBit);

                            }
                        }

                        
                        // todo: not break

                    }
                }
            }
        });
    }

    private void Collapse(Vector3Int coord, string bit)
    {
        int key = bit[0] - '0';
        Vector3Int rot = Orientations.ReturnRotationEulerFromChar(bit[1]);


        if (training.PrefabAndId.TryGetValue(key, out GameObject prefab))
        {
            GameObject gameObject = Instantiate(prefab, transform.position + coord, Quaternion.Euler(rot), transform);
            gameObject.name = "TEST";
        }

        _wave.MatrixData[coord.x, coord.y, coord.z] = false;

        //InitSelectionMatrix();
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
                Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.25F);

                if (_coordinateMatrix.Contains(coord, out Vector3Int localCoord))
                {
                    List<EOrientations> orientations = _coordinateMatrixOrientations.GetDataAt(localCoord);
                    foreach (EOrientations orientation in orientations)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(
                            transform.position + coord,
                            transform.position + coord + Orientations.ReturnDirectionVal(orientation)
                        );

                        Handles.Label(
                            transform.position + coord + (((Vector3)Orientations.ReturnDirectionVal(orientation)) / 2),
                            orientation.ToString()
                       );
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