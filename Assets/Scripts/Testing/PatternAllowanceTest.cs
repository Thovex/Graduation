using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{
    int value = 1;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PatternAllowanceTest patternTest = (PatternAllowanceTest)target;
        if (GUILayout.Button("Set Patterns"))
        {
            patternTest.SetPatterns();
        }
        EditorGUILayout.BeginHorizontal();
        value = EditorGUILayout.IntField("Pattern Index: ", value);
        if (GUILayout.Button("Spawn Pattern"))
        {
            patternTest.SpawnPattern(value);
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
    [SerializeField] private TrainingScript _training;

    [SerializeField] private List<Pattern> _patterns;
    [SerializeField] private Pattern _patternToSpawn;

    [SerializeField] private Vector3Int _outputSize = new Vector3Int(6, 6, 6);
    private Vector3Int _collapsingCoordinate = Vector3Int.zero;

    private Matrix<bool> _wave = new Matrix<bool>();
    private Matrix<Module> _modules = new Matrix<Module>();

    private Matrix<Vector3Int> _coordinateMatrix = new Matrix<Vector3Int>();
    private Matrix<List<EOrientations>> _coordinateMatrixOrientations = new Matrix<List<EOrientations>>();

    private string _bitToSpawn = "";
    private List<Vector3Int> _collidingCoordinates = new List<Vector3Int>();
    public List<Tuple<Vector3Int, Vector3Int>> _debugThing = new List<Tuple<Vector3Int, Vector3Int>>();

    public Vector3Int CollapsingCoordinate { get => _collapsingCoordinate; set => _collapsingCoordinate = value; }

    public void SetPatterns()
    {
        _patterns = _training.Patterns.ToList();

    }
    private void InitWave()
    {
        _wave = new Matrix<bool>(_outputSize);
        _modules = new Matrix<Module>(_outputSize);
        _collidingCoordinates.Clear();


        For3(_wave, (x, y, z) =>
        {
            _wave.MatrixData[x, y, z] = true;
        });

        for (int i = this.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }

    }
    public void SpawnPattern(int index)
    {
        InitWave();

        if (!(index >= 0 && index < _patterns.Count))
        {
            index = 0;
        }

        Pattern selectedPattern = _patterns[index];
        GameObject newPattern = new GameObject("Pattern");
        newPattern.transform.parent = this.transform;
        newPattern.transform.localPosition = Vector3.zero;

        if (_patterns.Count > 0)
        {
            For3(selectedPattern, (x, y, z) =>
            {
                GameObject patternData = Instantiate(
                    selectedPattern.MatrixData[x, y, z].Prefab,
                    new Vector3(x, y, z),
                    Quaternion.Euler(selectedPattern.MatrixData[x, y, z].RotationEuler),
                    newPattern.transform
                );

                patternData.transform.localPosition = new Vector3(_outputSize.x / 3 + x, y, _outputSize.z / 3 + z);
                _wave.MatrixData[_outputSize.x / 3 + x, y, _outputSize.z / 3 + z] = false;
                _modules.MatrixData[_outputSize.x / 3 + x, y, _outputSize.z / 3 + z] = selectedPattern.MatrixData[x, y, z];
            });

            Vector3Int NSize = new Vector3Int(_training.N, _training.N, _training.N);
            _coordinateMatrix = new Matrix<Vector3Int>(NSize);
            _coordinateMatrixOrientations = new Matrix<List<EOrientations>>(NSize);

            For3(_coordinateMatrix, (x, y, z) =>
            {
                _coordinateMatrix.MatrixData[x, y, z] = new Vector3Int(x, y, z);
                _coordinateMatrixOrientations.MatrixData[x, y, z] = new List<EOrientations>();
            });

        }
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

        CollapseCoordinate();
    }
    public void CollapseCoordinate()
    {
        _collidingCoordinates.Clear();
        _debugThing.Clear();

        For3(_coordinateMatrix, (x, y, z) =>
        {
            _coordinateMatrixOrientations.MatrixData[x, y, z] = new List<EOrientations>();

            Vector3Int coordinate = _coordinateMatrix.MatrixData[x, y, z];
            if (!_wave.GetDataAt(coordinate)) {
                string bit = _modules.GetDataAt(coordinate).GenerateBit(_training);


                _collidingCoordinates.Add(coordinate);
                foreach (Vector3Int direction in Orientations.Dirs)
                {
                    EOrientations orientation = Orientations.ReturnOrientationVal(direction);

                    if (orientation != EOrientations.UP && orientation != EOrientations.DOWN)
                    {

                        if (_coordinateMatrix.Contains(coordinate + direction) && _wave.GetDataAt(coordinate + direction))
                        {
                            Debug.DrawLine(coordinate, coordinate+direction);

                            Tuple<Vector3Int, Vector3Int> debug = new Tuple<Vector3Int, Vector3Int>(coordinate, coordinate + direction);

                            _debugThing.Add(debug);
                            _coordinateMatrixOrientations.MatrixData[x + direction.x,y + direction.y,z + direction.z].Add(orientation);
                        }
                    }
                }
            }
        });
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

            For3(_outputSize, (x, y, z) =>
            {


                Gizmos.color = _wave.MatrixData[x, y, z] ? greenColor : redColor;
                Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.25F);

            });


            Gizmos.color = magentaColor;

            For3(_coordinateMatrix, (x, y, z) =>
            {
                Gizmos.DrawSphere(transform.position + _coordinateMatrix.MatrixData[x, y, z], 0.25F);

            });

            For3(_coordinateMatrixOrientations, (x, y, z) =>
            {
                foreach (EOrientations orientation in _coordinateMatrixOrientations.MatrixData[x, y, z])
                {
                    if (orientation != EOrientations.NULL)
                    {
                        Gizmos.color = magentaColor;
                        Gizmos.DrawCube(
                            transform.position + _coordinateMatrix.MatrixData[x, y, x],
                            Vector3.one / 10
                        );

                        Gizmos.DrawLine(
                            transform.position + _coordinateMatrix.MatrixData[x, y, x],
                            transform.position + _coordinateMatrix.MatrixData[x, y, x]
                                + Orientations.ReturnDirectionVal(orientation)
                        );

                        Gizmos.color = yellowColor;

                        Gizmos.DrawCube(
                            transform.position + _coordinateMatrix.MatrixData[x, y, x]
                                + Orientations.ReturnDirectionVal(orientation),
                            Vector3.one / 10
                        );

                        // Handles.Label(transform.position + _coordinateMatrix.MatrixData[x, y, x] + Vector3.up * .5F, _coordinateMatrix.MatrixData[x, y, z].ToString());
                    }
                }

            });

            Debug.Log(_debugThing.Count());
            foreach (Tuple<Vector3Int, Vector3Int> thing in _debugThing)
            {

                Gizmos.color = Color.red;
                Gizmos.DrawLine(thing.Item1, thing.Item2);

            }
        }
        catch (Exception)
        {
            SetPatterns();
            SpawnPattern(0);
        }
    }
}
