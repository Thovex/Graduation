using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coefficient{
	
}

public class Initializer : MonoBehaviour{
	private bool[,,] _wave;
	
    public void InitializeWFC(Vector3Int outputSize){
	    _wave = new bool[outputSize.x, outputSize.y, outputSize.z];
    }
}
