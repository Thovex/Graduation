using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;

[CustomEditor(typeof(PatternAllowanceTest))]
public class PatternAllowanceTestInspector : OdinEditor
{
    int value = 1;    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PatternAllowanceTest patternTest = (PatternAllowanceTest) target;

        if (GUILayout.Button("Set Patterns"))
        {
            patternTest.SetPatterns();
        }

        value = EditorGUILayout.IntField("Pattern Index: ", value);
        
        if (GUILayout.Button("Spawn Pattern"))
        {
            patternTest.SpawnPattern(value);
        }
        
        
        if (GUILayout.Button("Collapse Random coordinate"))
        {
            patternTest.CollapseRandomCoordinate();
        }
        

    }
}

public class PatternAllowanceTest : SerializedMonoBehaviour
{
    [SerializeField] private TrainingScript _training;
    [SerializeField] private List<Pattern> _patterns;

    [SerializeField] private Vector3Int _outputSize = new Vector3Int(6, 6, 6);
    private Matrix < bool > _wave = new Matrix < bool >(Vector3Int.zero);

    private Vector3Int _collapsingCoordinate = Vector3Int.zero;
    private string _bitToSpawn = "";

    [SerializeField] private Pattern _patternToSpawn;
    

    public void SetPatterns(){
        _patterns = _training.Patterns.ToList();
        InitWave();
    }

    private void InitWave(){
        _wave = new Matrix < bool >(_outputSize);
        
        Nested3(_wave, (x, y, z) => { _wave.MatrixData[x, y, z] = true; });
        
        for (int i = this.transform.childCount; i > 0; --i)
        {
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }
    }

    public void SpawnPattern(int index){
        InitWave();

        if (!(index >= 0 && index < _patterns.Count)) index = 0;

        Pattern selectedPattern = _patterns[index];

        GameObject newPattern = new GameObject("Pattern");
        newPattern.transform.parent = this.transform;

        newPattern.transform.localPosition = Vector3.zero;

        Nested3(selectedPattern, (x, y, z) => {
            GameObject patternData = Instantiate(selectedPattern.MatrixData[x, y, z].Prefab, new Vector3(x, y, z), Quaternion.Euler(selectedPattern.MatrixData[x, y, z].RotationEuler), newPattern.transform);
            patternData.transform.localPosition = new Vector3(_outputSize.x / 3 + x,  y, _outputSize.z/ 3 + z);
            _wave.MatrixData[_outputSize.x / 3 +x, y, _outputSize.z / 3+z] = false;
        });

        
    }

    public void CollapseRandomCoordinate(){
        List < Vector3Int > possibleCoordinates = new List < Vector3Int >();

        Nested3(_wave, (x, y, z) => {
            if ( !_wave.MatrixData[x, y, z] ){
                possibleCoordinates.Add(new Vector3Int(x, y, z));
            }
        });

        Vector3Int coordinate = possibleCoordinates[UnityEngine.Random.Range(0, possibleCoordinates.Count - 1)];
        Vector3Int newCoordinate = coordinate;


        while( possibleCoordinates.Contains(newCoordinate) ){
            int tries = 0;

            Vector3Int adding = Vector3Int.zero;

            bool useX = UnityEngine.Random.value > 0.5f;

            if ( useX )
                adding.x = ( UnityEngine.Random.value > 0.5f ) ? 1 : -1;
            else
                adding.z = ( UnityEngine.Random.value > 0.5f ) ? 1 : -1;

            if ( newCoordinate.x + adding.x > 0 && newCoordinate.x + adding.x < _wave.SizeX ){
                newCoordinate.x += adding.x;
            }

            if ( newCoordinate.z + adding.z > 0 && newCoordinate.z + adding.z < _wave.SizeZ ){
                newCoordinate.z += adding.z;
            }

            _collapsingCoordinate = newCoordinate;
            tries++;

            if ( tries > 25 ){
                coordinate = possibleCoordinates[UnityEngine.Random.Range(0, possibleCoordinates.Count - 1)];
                newCoordinate = coordinate;
            }

            if ( tries > 500 ){
                break;
            }
        }
        
        Module module;
        if ( _training.ChildrenByCoordinate.TryGetValue(coordinate, out module) ){

            EOrientations selectedDir = EOrientations.NULL;

            foreach ( Vector3Int orientationVector in Orientations.Dirs ){
                if ( coordinate - orientationVector == _collapsingCoordinate ){
                    selectedDir = Orientations.ReturnOrientationVal(orientationVector);
                }
            }

            List < Possibility > possibilities;
            if ( _training.NeighbourPossibilitiesPerBit.TryGetValue(module.GenerateBit(_training), out possibilities) ){
                foreach ( Possibility possibility in possibilities ){
                    if ( possibility.Orientation == selectedDir ){

                        List < string > possibilitiesStringList = possibility.Possibilities.ToList();

                        int randomIndex = UnityEngine.Random.Range(0, possibility.Possibilities.Count - 1);
                        _bitToSpawn = possibilitiesStringList[randomIndex];
                    }
                }
            }
        }

        _patternToSpawn = null;
        List<Pattern> newPattern;

        if ( _training.GetPatternByBit(_bitToSpawn, out newPattern) ){
            
            int randomIndex = UnityEngine.Random.Range(0, newPattern.Count - 1);
            _patternToSpawn = newPattern[randomIndex];
        }
    }

    private void OnDrawGizmos()
    {
        try{
            Nested3(_outputSize, (x, y, z) => {
                Color redColor = Color.red;
                redColor.a = 0.75F;

                Color greenColor = Color.green;
                greenColor.a = 0.15F;

                Gizmos.color = _wave.MatrixData[x, y, z] ? greenColor : redColor;
                Gizmos.DrawSphere(transform.position + new Vector3(x, y, z), 0.25F);
            });

            Color magentaColor = Color.magenta;
            magentaColor.a = 0.75F;
            
            Gizmos.color = magentaColor;
            Gizmos.DrawSphere(transform.position + V3ToV3I(_collapsingCoordinate), 0.25F);
            
            Handles.Label(transform.position + V3ToV3I(_collapsingCoordinate), _bitToSpawn);

            if ( _patternToSpawn != null){
                Matrix < string > bits;

                if ( _training.PatternBits.TryGetValue(_patternToSpawn, out bits) ){
                    Nested3(_patternToSpawn, (x, y, z) => {
                        Handles.Label(
                            transform.position + new Vector3(x, y, z) - ( Vector3.right * ( _outputSize.x / 2 ) ), 
                            bits.MatrixData[x,y,z]
                        );
                    });
                }
            }
        }
        catch ( Exception e ){
            InitWave();
        }
    }
}


