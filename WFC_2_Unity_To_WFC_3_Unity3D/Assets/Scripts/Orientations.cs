using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

[System.Serializable]
public enum EOrientations{
	FORWARD,
	BACK,
	RIGHT,
	LEFT,
	UP,
	DOWN,
	NULL
};

public static class Orientations {
	private static readonly Vector3Int FORWARD =  new Vector3Int(0, 0, -1);
	private static readonly Vector3Int BACK =new Vector3Int(0, 0, 1);

	private static readonly Vector3Int RIGHT = Vector3Int.right;
	private static readonly Vector3Int LEFT = Vector3Int.left;

	private static readonly Vector3Int UP = Vector3Int.up;
	private static readonly Vector3Int DOWN =Vector3Int.down;
	
	public static readonly Vector3Int[] Dirs = new Vector3Int[] {FORWARD, BACK, RIGHT, LEFT, UP, DOWN};

	public static Vector3Int ReturnDirectionVal(EOrientations orientation){
		switch ( orientation ){
			case EOrientations.FORWARD:
				return BACK;
				break;
			case EOrientations.BACK:
				return FORWARD;
				break;
			case EOrientations.RIGHT:
				return LEFT;
				break;
			case EOrientations.LEFT:
				return RIGHT;
				break;
			case EOrientations.UP:
				return DOWN;
				break;
			case EOrientations.DOWN:
				return UP;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
		}
	}

	public static EOrientations ReturnOrientationVal(Vector3Int directionVector){
		if ( directionVector == FORWARD ) return EOrientations.FORWARD;
		if ( directionVector == BACK ) return EOrientations.BACK;
		if ( directionVector == RIGHT ) return EOrientations.RIGHT;
		if ( directionVector == LEFT ) return EOrientations.LEFT;
		if ( directionVector == UP ) return EOrientations.UP;
		if ( directionVector == DOWN ) return EOrientations.DOWN;
		
		return EOrientations.NULL;
	}
}