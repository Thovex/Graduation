using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;
using Vector3Int = UnityEngine.Vector3Int;

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : OdinEditor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        TrainingScript training = (TrainingScript)target;
            
//        if (GUILayout.Button("For1")){
//        }
//        
//        if (GUILayout.Button("For2")){
//        }
    }
}

[Serializable]
public struct Module{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Vector3Int rotationEuler;
    [SerializeField] private List < OrientationModule > moduleNeighbours;


    public GameObject Prefab{
        get{ return prefab; }
        set{ prefab = value; }
    }

    public Vector3Int RotationEuler{
        get{ return rotationEuler; }
        set{ rotationEuler = value; }
    }

    public List < OrientationModule > ModuleNeighbours{
        get{ return moduleNeighbours; }
        set{ moduleNeighbours = value; }
    }

    public Module(GameObject prefab, Vector3Int rotationEuler){
        this.prefab = prefab;
        this.rotationEuler = rotationEuler;
        this.moduleNeighbours = new List < OrientationModule >();
    }
}


[Serializable]
public struct OrientationModule{
    [SerializeField] private EOrientations orientation;
    [SerializeField] private Module neighbourModule;
    
    public EOrientations Orientation{
        get{ return orientation; }
        set{ orientation = value; }
    }

    public Module NeighbourModule{
        get{ return neighbourModule; }
        set{ neighbourModule = value; }
    }

    public OrientationModule(EOrientations orientation, Module neighbourModule){
        this.orientation = orientation;
        this.neighbourModule = neighbourModule;
    }
}

[ExecuteInEditMode]
public class TrainingScript : SerializedMonoBehaviour{

    [SerializeField] private Dictionary<Vector3Int, Module> _childrenByCoordinate = new Dictionary<Vector3Int, Module>();
    [SerializeField] private Dictionary<int, GameObject> _prefabAndId = new Dictionary<int, GameObject>();
    [SerializeField] private List<Pattern> _patterns = new List< Pattern>();

    [SerializeField] private GameObject _displayPatternObject;

    private Matrix < Module > _moduleMatrix;

    private InputGriddify _input;

    private void Update(){
        TranslatePrefabsToId();

    }

    private void TranslatePrefabsToId(){
        ClearPreviousData();
        
        GetResources();
        
        _input = GetComponent<InputGriddify>();

        AssignCoordinateToChildren();

        CalculateNeighbours();
        
        InitializeMatrix();

        DefinePatterns();
        
        DisplayPatterns();
    }
    
    private void ClearPreviousData(){
        _childrenByCoordinate = new Dictionary<Vector3Int, Module>();
        _prefabAndId = new Dictionary < int, GameObject >();
        _moduleMatrix = new Matrix < Module >(Vector3Int.zero);
        _patterns = new List < Pattern >();

    }

    private void GetResources(){
        GameObject[] Prefabs = Resources.LoadAll<GameObject>("Wfc");

        for ( int i = 0; i < Prefabs.Length; i++ ){
            _prefabAndId.Add(i, Prefabs[i]);
        }
    }

    private void AssignCoordinateToChildren(){
        for ( int i = 0; i < transform.childCount; i++ ){
            Transform childTransform = transform.GetChild(i);
            
            _childrenByCoordinate.Add(
                V3ToV3I(childTransform.localPosition), 
                new Module(
                    (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject),
                    V3ToV3I(childTransform.localEulerAngles)
                )
            );
            
            childTransform.name = V3ToV3I(childTransform.localPosition).ToString()  + " " +((GameObject)PrefabUtility.GetCorrespondingObjectFromSource(childTransform.gameObject)).name ;
        }
    }

    private void CalculateNeighbours(){
        Dictionary <Vector3Int, Module> ChildrenByCoordinateWithNeighbours = new Dictionary < Vector3Int, Module >();
        
        foreach ( KeyValuePair < Vector3Int, Module > Pair in _childrenByCoordinate ){
            List < OrientationModule > Neighbours = new List < OrientationModule >();
            
            foreach ( Vector3Int orientation in Orientations.Dirs ){
                Vector3Int neighbourCoordinate = Pair.Key + orientation;

                if ( _childrenByCoordinate.ContainsKey(neighbourCoordinate) ){
                    Module neighbourModule;
                    if (_childrenByCoordinate.TryGetValue(neighbourCoordinate, out neighbourModule) ) {
                        Neighbours.Add(new OrientationModule(Orientations.ReturnOrientationVal(orientation), neighbourModule));
                    }
                }
            }

            Module updatedModule = Pair.Value;
            updatedModule.ModuleNeighbours = Neighbours;
            ChildrenByCoordinateWithNeighbours.Add(Pair.Key, updatedModule);
        }

        _childrenByCoordinate = ChildrenByCoordinateWithNeighbours;
    }

    private void InitializeMatrix(){
        _moduleMatrix = new Matrix < Module >(_input.inputSize);

        Nested3(_moduleMatrix, (x, y, z) => {
            Module module;
                    
            if (_childrenByCoordinate.TryGetValue(new Vector3Int(x,y,z), out module)){
                _moduleMatrix.MatrixData[x, y, z] = module;
            }
        });
    }
    
    private void DefinePatterns(){
        int n = _input.NValue;
        
        if ( n > 0 ){
            
            Nested3(_input.inputSize, n, (x, y, z) => {
                Module[,,] newTrainingData = new Module[n,n,n];

                bool bIsNull = true;

                Nested3(new Vector3Int(n, n, n), (nx, ny, nz) => {
                    Module module;

                    if ( _childrenByCoordinate.TryGetValue(new Vector3Int(x + nx, y + ny, z + nz), out module) ){
                        newTrainingData[nx, ny, nz] = module;
                        bIsNull = false;
                    }
                });
                        
                if ( !bIsNull ){
                    Pattern newPattern = new Pattern(n, newTrainingData, new Vector3Int(x, y, z));
                    _patterns.Add(newPattern);

                    for ( int i = 1; i < 4; i++ ){
                        Pattern rotatedPattern = new Pattern(n, newTrainingData, new Vector3Int(x, y, z));
                        rotatedPattern.RotatePatternCounterClockwise(i);
                        _patterns.Add(rotatedPattern);
                    }
                }
            });
        }
    }

    private void DisplayPatterns(){

        if ( _displayPatternObject ){

            for ( int i = _displayPatternObject.transform.childCount; i > 0; --i ){
                DestroyImmediate(_displayPatternObject.transform.GetChild(0).gameObject);
            }

            int index = 0;

            foreach ( Pattern pattern in _patterns ){
                GameObject newPattern = new GameObject("Pattern");

                newPattern.transform.localPosition = Vector3.zero + ( index * 3 ) * Vector3.left;
                newPattern.transform.parent = _displayPatternObject.transform;

                Module[,,] data = pattern.MatrixData;
                
                Nested3(pattern, (x, y, z) => {
                    if ( data[x, y, z].Prefab != null ){
                        GameObject patternData = Instantiate(data[x, y, z].Prefab, newPattern.transform);
                        patternData.transform.localPosition = new Vector3(x, y, z );
                        patternData.transform.localEulerAngles = data[x, y, z].RotationEuler;

                    }   
                });
                index++;
            }
        }
    }
}