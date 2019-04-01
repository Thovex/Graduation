using System;
using UnityEngine;

[Serializable]
public struct OrientationModule{
	[SerializeField] private EOrientations _orientation;
	[SerializeField] private Module _neighbourModule;
    
	public EOrientations Orientation{
		get{ return _orientation; }
		set{ _orientation = value; }
	}

	public Module NeighbourModule{
		get{ return _neighbourModule; }
		set{ _neighbourModule = value; }
	}

	public OrientationModule(EOrientations orientation, Module neighbourModule){
		this._orientation = orientation;
		this._neighbourModule = neighbourModule;
	}
}