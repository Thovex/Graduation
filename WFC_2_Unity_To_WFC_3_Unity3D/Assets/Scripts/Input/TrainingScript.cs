using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrainingScript))]
public class TrainingScriptInspector : OdinEditor
{

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        TrainingScript training = (TrainingScript)target;
            
        if (GUILayout.Button("Start WFC")){
            training.WaveFunctionCollapse();
        }
    }
}

[System.Serializable]
public struct TrainingChild{
    [SerializeField] private readonly Vector3Int _localPosition;
    [SerializeField] private readonly Vector3Int _localRotation;
    [SerializeField] private readonly GameObject _gameObject;
    [SerializeField] private readonly int _gameObjectIndex;

    public TrainingChild(Vector3Int localPosition, Vector3Int localRotation, GameObject gameObject, int gameObjectIndex){
        _localPosition = localPosition;
        _localRotation = localRotation;
        _gameObject = gameObject;
        _gameObjectIndex = gameObjectIndex;

    }

    public Vector3Int LocalPosition => _localPosition;
    public Vector3Int LocalRotation => _localRotation;
    public GameObject GameObject => _gameObject;
    public int GameObjectIndex => _gameObjectIndex;

}

[System.Serializable]
public struct TrainingData{
    [SerializeField] private readonly Vector3Int _localRotation;
    [SerializeField] private readonly GameObject _gameObject;
    [SerializeField] private readonly int _gameObjectIndex;
    [SerializeField] private readonly Dictionary < EOrientations, TrainingNeighbourData > _neighbourData;
    
    public TrainingData(Vector3Int localRotation, GameObject gameObject, int gameObjectIndex, Dictionary <EOrientations, TrainingNeighbourData> neighbourData){
        _localRotation = localRotation;
        _gameObject = gameObject;
        _gameObjectIndex = gameObjectIndex;
        _neighbourData = neighbourData;
    }
    
    public Vector3Int LocalRotation => _localRotation;
    public GameObject GameObject => _gameObject;
    public int GameObjectIndex => _gameObjectIndex;
    public Dictionary < EOrientations, TrainingNeighbourData > NeighbourData => _neighbourData;
}

[System.Serializable]
public struct TrainingNeighbourData{
    [SerializeField] private readonly Vector3Int _localRotation;
    [SerializeField] private readonly int _gameObjectIndex;
    
    public TrainingNeighbourData(Vector3Int localRotation, int gameObjectIndex){
        _localRotation = localRotation;
        _gameObjectIndex = gameObjectIndex;
    }
    
    public Vector3Int LocalRotation => _localRotation;
    public int GameObjectIndex => _gameObjectIndex;
}

[ExecuteInEditMode]
public class TrainingScript : SerializedMonoBehaviour{
    [SerializeField] private Initializer _output;
    [SerializeField] private Vector3Int _outputSize;

    [SerializeField] private List<GameObject> _resources;
    [SerializeField] private Dictionary<TrainingData,int > _conversionDictionary = new Dictionary<TrainingData, int>();
    [SerializeField] private Dictionary<int, GameObject> _prefabConversionDictionary = new Dictionary<int, GameObject>();
    [SerializeField] private Dictionary<Vector3Int, TrainingChild> _children = new Dictionary<Vector3Int, TrainingChild>();
    [SerializeField] private List<Pattern> _patterns;


    private InputGriddify _input;

    private GameObject[,,] _inputMatrix;
    private int[,,] _inputIndexMatrix;


    private static int DefineNewIndex(int index) => index + 1;
    
    private int PrefabToId(GameObject prefab)
    {
        foreach (KeyValuePair<int, GameObject> entry in _prefabConversionDictionary)
        {
            if (entry.Value == IdToPrefab(prefab.gameObject.GetComponent<UniqueId>().uniquePrefabId))
            {
                return entry.Key;
            }
        }

        return 0;
    }

    private GameObject IdToPrefab(int id){
        GameObject prefab;
        
        _prefabConversionDictionary.TryGetValue(id, out prefab);
        return prefab;
    }
    
    private GameObject GetResourceById(int id)
    {
        foreach (GameObject g in _resources)
        {
            if (g.GetComponent<UniqueId>().uniquePrefabId == id)
            {
                return g;
            }
        }

        return null;
    }

    private void Update(){
        try{
            TranslatePrefabsToId();
        }
        catch ( Exception e ){
            Debug.LogError("Error in TrainingScript Update");
        }
    }

    public void TranslatePrefabsToId()
    {
        _conversionDictionary = new Dictionary<TrainingData, int>();

        _input = GetComponent<InputGriddify>();

        if (!_input)
        {
            Debug.LogError("No input (using InputGriddify)! Aborting.)");
            return;
        }

        InitializeMatrixValues();
        
        DefineMatrixSize();
        DefineConversion();

        ReinitializeMatrixValuesTEMP();
        
        CreatePatterns();
        ClearNullPatterns();
        
        ManipulateModuleData();
        
        RemoveNullValues();

    }

    private void RemoveNullValues(){
        List < Vector3Int > indexValuesToRemove = new List < Vector3Int >();

        foreach ( KeyValuePair < Vector3Int, TrainingChild > pair in _children ){
            if ( pair.Value.GameObject == _resources[0].gameObject ){
                indexValuesToRemove.Add(pair.Key);
            }
        }

        foreach ( Vector3Int index in indexValuesToRemove ){
            _children.Remove(index);
        }
    }

    private void InitializeMatrixValues()
    {
        _children = new Dictionary < Vector3Int, TrainingChild >();
        _conversionDictionary = new Dictionary<TrainingData, int>();
        _prefabConversionDictionary = new Dictionary < int, GameObject >();
        _patterns = new List < Pattern >();

        if ( _resources.Count == 0 ){
            Debug.LogError("Resources Empty");
        }
        

        
        // this function is done another time in the TEMP function. need to fix later but for now is ok
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 location = transform.GetChild(i).transform.localPosition;
            Vector3Int gridLocation = new Vector3Int(
                Mathf.RoundToInt(location.x),
                Mathf.RoundToInt(location.y),
                Mathf.RoundToInt(location.z)
            );

            Vector3 rotation = transform.GetChild(i).transform.localEulerAngles;
            Vector3Int gridRotation = new Vector3Int(
                Mathf.RoundToInt(rotation.x),
                Mathf.RoundToInt(rotation.y),
                Mathf.RoundToInt(rotation.z)
            );

            transform.GetChild(i).transform.name = gridLocation.ToString();
            _children.Add(gridLocation, new TrainingChild(
                gridLocation, 
                gridRotation, 
                transform.GetChild(i).gameObject, 
                0)
            );

        }

        for (int x = 0; x < _input.inputSize.x; x++)
        {
            for (int y = 0; y < _input.inputSize.y; y++)
            {
                for (int z = 0; z < _input.inputSize.z; z++){
                   
                    if (!_children.ContainsKey(new Vector3Int(x,y,z))){
                        
                        TrainingChild emptyChild = new TrainingChild(
                            new Vector3Int(x, y, z), 
                            Vector3Int.zero, 
                            _resources[0].gameObject,
                            0
                         );
                        _children.Add(new Vector3Int(x, y, z), emptyChild);
                    }
                }
            }
        }
    }

    private void ReinitializeMatrixValuesTEMP(){
        _children.Clear();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 location = transform.GetChild(i).transform.localPosition;
            Vector3Int gridLocation = new Vector3Int(
                Mathf.RoundToInt(location.x),
                Mathf.RoundToInt(location.y),
                Mathf.RoundToInt(location.z)
            );

            Vector3 rotation = transform.GetChild(i).transform.localEulerAngles;
            Vector3Int gridRotation = new Vector3Int(
                Mathf.RoundToInt(rotation.x),
                Mathf.RoundToInt(rotation.y),
                Mathf.RoundToInt(rotation.z)
            );

            transform.GetChild(i).transform.name = gridLocation.ToString();
            _children.Add(gridLocation, new TrainingChild(
                gridLocation, 
                gridRotation, 
                IdToPrefab(transform.GetChild(i).gameObject.GetComponent < UniqueId >().uniquePrefabId), 
                PrefabToId(IdToPrefab(transform.GetChild(i).gameObject.GetComponent < UniqueId >().uniquePrefabId)))
            );

        }

        for (int x = 0; x < _input.inputSize.x; x++)
        {
            for (int y = 0; y < _input.inputSize.y; y++)
            {
                for (int z = 0; z < _input.inputSize.z; z++){
                   
                    if (!_children.ContainsKey(new Vector3Int(x,y,z))){
                        
                        TrainingChild emptyChild = new TrainingChild(
                            new Vector3Int(x, y, z), 
                            Vector3Int.zero, 
                            _resources[0].gameObject,
                            0
                        );
                        _children.Add(new Vector3Int(x, y, z), emptyChild);
                    }
                }
            }
        }
    }

    private void DefineMatrixSize()
    {
        // Define matrix size
        _inputMatrix = new GameObject[_input.inputSize.x, _input.inputSize.y, _input.inputSize.z];

        for (int x = 0; x < _input.inputSize.x; x++)
        {
            for (int y = 0; y < _input.inputSize.y; y++)
            {
                for (int z = 0; z < _input.inputSize.z; z++)
                {
                    TrainingChild child;
                    _children.TryGetValue(new Vector3Int(x, y, z), out child);
                    _inputMatrix[x, y, z] = child.GameObject;
                }
            }
        }
    }

    private void DefineConversion()
    {
        int maxX = _input.inputSize.x, maxY = _input.inputSize.y, maxZ = _input.inputSize.z;

        HashSet<int> currentIds = new HashSet<int>();

        int index = 0;

        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    if (currentIds.Add(_inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId))
                    {
                        int newChar = DefineNewIndex(index);
                        
                        _prefabConversionDictionary.Add(newChar, GetResourceById(_inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId));
                        index++;
                    }
                }
            }
        }
        
        foreach ( KeyValuePair < Vector3Int, TrainingChild > pair in _children ){

            if ( pair.Value.GameObject != _resources[0].gameObject ){

                Dictionary < EOrientations, TrainingNeighbourData > neighbourData = new Dictionary < EOrientations, TrainingNeighbourData >();

                foreach ( Vector3Int orientation in Orientations.Dirs ){
                    Vector3Int neighbourCoordinate = pair.Key + orientation;

                    if ( _children.ContainsKey(neighbourCoordinate) ){

                        TrainingChild trainingChild;
                        _children.TryGetValue(neighbourCoordinate, out trainingChild);

                        if ( trainingChild.GameObject != _resources[0].gameObject ){

                            TrainingNeighbourData trainingNeighbourData = new TrainingNeighbourData(
                                trainingChild.LocalRotation, 
                                PrefabToId(IdToPrefab(trainingChild.GameObject.GetComponent<UniqueId>().uniquePrefabId)));

                            neighbourData.Add(Orientations.ReturnOrientationVal(orientation), trainingNeighbourData);
                        }
                    }
                }

                TrainingData newTrainingData = new TrainingData(
                    pair.Value.LocalRotation, 
                    pair.Value.GameObject,
                    PrefabToId(IdToPrefab(pair.Value.GameObject.GetComponent<UniqueId>().uniquePrefabId)),
                    neighbourData);
                
                _conversionDictionary.Add(newTrainingData, _conversionDictionary.Count + 1);
            }
        }
    }

    private TrainingData GetDataByPrefabIdAndRotation(int prefabId, Vector3Int rotation){
        foreach ( KeyValuePair < TrainingData, int > pair in _conversionDictionary ){
            if ( pair.Key.GameObjectIndex == prefabId && pair.Key.LocalRotation == rotation ) return pair.Key;
        }
        
        return new TrainingData();
    }

    private void CreatePatterns(){

        int n = _input.NValue;
        
        if ( n > 0 ){
            
            for ( int x = 0; x < _input.inputSize.x; x+=n ){
                for ( int y = 0; y < _input.inputSize.y ; y+=n ){
                    for ( int z = 0; z < _input.inputSize.z; z+=n ){
                        // Only optimized for N == 2 in current state.

                        TrainingData[,,] newTrainingData = new TrainingData[n,n,n];
                        
                        for ( int nx = 0; nx < n; nx++ ){
                            for ( int ny = 0; ny < n; ny++ ){
                                for ( int nz = 0; nz < n; nz++ ){
                                    TrainingChild coordinateChild;
                                    _children.TryGetValue(new Vector3Int (x + nx, y + ny, z + nz), out coordinateChild);

                                    if ( coordinateChild.GameObject != _resources[0].gameObject ){
                                        newTrainingData[nx, ny, nz] = GetDataByPrefabIdAndRotation(coordinateChild.GameObjectIndex, coordinateChild.LocalRotation);
                                    }
                                }
                            }
                        }

                        Pattern newPattern = new Pattern(n, newTrainingData, new Vector3Int(x, y, z));
                        _patterns.Add(newPattern);
                    }
                }
            }
        }
    }

    private void ClearNullPatterns(){
        List < Pattern > validPatterns = new List< Pattern >();
        foreach ( Pattern pattern in _patterns ){
            TrainingData[,,] data = pattern.MatrixData;

            bool bHasNonNullValue = false;
            
            for ( int x = 0; x < data.GetLength(0); x++ ){
                for ( int y = 0; y < data.GetLength(1); y++ ){
                    for ( int z = 0; z < data.GetLength(2); z++ ){
                        if ( data[x, y, z].GameObject != null ){
                            bHasNonNullValue = true;
                        }
                    }
                }
            }

            if ( bHasNonNullValue ){
                validPatterns.Add(pattern);
            }
        }

        _patterns = validPatterns;
    }

    private void ManipulateModuleData(){
        foreach ( KeyValuePair < TrainingData, int > pair in _conversionDictionary ){
            ModulePrototype modulePrototype = pair.Key.GameObject.GetComponent < ModulePrototype >();

            if ( modulePrototype ){    
                modulePrototype.ModuleOrientations.Clear();
                modulePrototype.ModuleOrientations = pair.Key.NeighbourData;
            }
        }
    }

    private void OnDrawGizmos(){
        
        try {
            foreach ( KeyValuePair < TrainingData, int > pair in _conversionDictionary){
                Gizmos.color = Color.yellow;

                foreach ( KeyValuePair <EOrientations, TrainingNeighbourData> neighbourPair in pair.Key.NeighbourData ){
                    Gizmos.DrawLine(pair.Key.GameObject.transform.position, pair.Key.GameObject.transform.position+ (Orientations.ReturnDirectionVal(neighbourPair.Key)));
                }
             
            }
        }
        catch ( Exception e ){
            TranslatePrefabsToId();
        }
        
    }

    public void WaveFunctionCollapse(){
        _output.InitializeWFC(_outputSize);
    }
}