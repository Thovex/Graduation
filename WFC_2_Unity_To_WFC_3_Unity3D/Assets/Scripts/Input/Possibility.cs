using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Possibility{
	[SerializeField] private EOrientations _orientation;
	[SerializeField] private HashSet <string> _possibilities;
    
	public EOrientations Orientation{
		get{ return _orientation; }
		set{ _orientation = value; }
	}

	public HashSet <string>  Possibilities{
		get{ return _possibilities; }
		set{ _possibilities = value; }
	}

	public Possibility(EOrientations orientation, HashSet <string> possibilities){
		this._orientation = orientation;
		this._possibilities = possibilities;
	}
}