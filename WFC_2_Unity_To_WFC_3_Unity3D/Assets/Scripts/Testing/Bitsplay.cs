using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using static Thovex.Utility;

public class Bitsplay : MonoBehaviour{
    [SerializeField] private Pattern _pattern;
    [SerializeField] private TrainingScript _training;

    public Pattern Pattern{
        get{ return _pattern; }
        set{
            _pattern = value;

            if ( _training ){
                _bits = value.GenerateBits(Training);
            }
        }
    }

    public TrainingScript Training{
        private get{ return _training; }
        set{ _training = value; }
    }

    private Matrix<string> _bits;

    private void OnDrawGizmos(){

        For3(_bits, (x, y, z) => {
            Handles.Label(transform.position + new Vector3(x,y,z), _bits.MatrixData[x,y,z]); 
        });
    }
}
