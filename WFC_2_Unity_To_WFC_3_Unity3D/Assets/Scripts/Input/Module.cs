using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Module{
	[SerializeField] private GameObject _prefab;
	[SerializeField] private EDirection _rotationDir;
	[SerializeField] private Vector3Int _rotationEuler;
	[SerializeField] private List < OrientationModule > _moduleNeighbours;


	public GameObject Prefab{
		get{ return _prefab; }
		set{ _prefab = value; }
	}

	public EDirection RotationDir{
		get{ return _rotationDir; }
		set{ _rotationDir = value; }
	}

	public List < OrientationModule > ModuleNeighbours{
		get{ return _moduleNeighbours; }
		set{ _moduleNeighbours = value; }
	}

	public Vector3Int RotationEuler{
		get{ return _rotationEuler; }
		set{
			_rotationEuler = value;
			RotationDir = Orientations.ReturnDirectionVal(value);
		}
	}

	public Module(GameObject prefab, Vector3Int rotationEuler){
		this._prefab = prefab;
		this._rotationEuler = rotationEuler;
		this._rotationDir = Orientations.ReturnDirectionVal(rotationEuler);
		this._moduleNeighbours = new List < OrientationModule >();
	}

	public string GenerateBit(TrainingScript training){

		int id = training.PrefabToId(this.Prefab);
		ModulePrototype modulePrototype = this.Prefab.GetComponent < ModulePrototype >();

		string bitString = id.ToString();

		if ( modulePrototype ){
			if ( modulePrototype.IsSymmetrical ){
				bitString += "S";
			}
		}
		else {
			bitString += this.RotationDir.ToString()[0];
		}

		return bitString;
	}
}