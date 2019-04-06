using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initializer : MonoBehaviour{
	private bool[,,] _wave;
	
    public void InitializeWFC(Vector3Int outputSize){
	    _wave = new bool[outputSize.x, outputSize.y, outputSize.z];

	    for ( int x = 0; x < outputSize.x; x++ ){
		    for ( int y = 0; y < outputSize.y; y++ ){
			    for ( int z = 0; z < outputSize.z; z++ ){
				    _wave[x, y, z] = true;
			    }
		    }
	    }
    }
}
