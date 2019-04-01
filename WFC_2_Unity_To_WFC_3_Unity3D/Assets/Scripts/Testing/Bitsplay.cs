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
            _bits3D = new Matrix < string >(new Vector3Int(value.SizeX, value.SizeY, value.SizeZ));

            Nested3(_bits3D, (x, y, z) => {

                if ( _training ){
                    int id = _training.PrefabToId(_pattern.MatrixData[x, y, z].Prefab);


                    ModulePrototype modulePrototype = _pattern.MatrixData[x, y, z].Prefab.GetComponent < ModulePrototype >();

                    string bitString = id.ToString();

                    if ( modulePrototype ){
                        if ( modulePrototype.IsSymmetrical ){
                            bitString += "S";
                        }
                    }
                    else{
                        bitString += _pattern.MatrixData[x, y, z].RotationDir.ToString()[0];
                    }

                    _bits3D.MatrixData[x, y, z] = bitString;
                }
            });
        }
    }

    public TrainingScript Training{
        get{ return _training; }
        set{ _training = value; }
    }

    private Matrix<string> _bits3D;

    private void OnDrawGizmos(){

        Nested3(_bits3D, (x, y, z) => {
            Handles.Label(transform.position + new Vector3(x,y,z), _bits3D.MatrixData[x,y,z]); 
        });
    }
}
