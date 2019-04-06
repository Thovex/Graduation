using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Possibility{
	[SerializeField] private EOrientations orientation;
	[SerializeField] private HashSet <string> possibilities;
    
	public EOrientations Orientation{
		get => orientation;
        set => orientation = value;
    }

	public HashSet <string>  Possibilities{
		get => possibilities;
        set => possibilities = value;
    }

	public Possibility(EOrientations orientation, HashSet <string> possibilities){
		this.orientation = orientation;
		this.possibilities = possibilities;
	}
}