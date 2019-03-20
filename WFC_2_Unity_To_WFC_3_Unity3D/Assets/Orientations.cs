using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable InconsistentNaming

[System.Serializable]
public enum EDirections{
	FORWARD,
	BACK,
	RIGHT,
	LEFT,
	UP,
	DOWN
};

public static class Orientations {
	private static readonly Vector3Int FORWARD =  new Vector3Int(0, 0, 1);
	private static readonly Vector3Int BACK = new Vector3Int(0, 0, -1);

	private static readonly Vector3Int RIGHT = Vector3Int.right;
	private static readonly Vector3Int LEFT = Vector3Int.left;

	private static readonly Vector3Int UP = Vector3Int.down;
	private static readonly Vector3Int DOWN = Vector3Int.down;
	
	public static Vector3Int[] Dirs = new Vector3Int[] {FORWARD, BACK, RIGHT, LEFT, UP, DOWN};

	public static Vector3Int ReturnDirectionVal(EDirections Direction){
		switch ( Direction ){
			case EDirections.FORWARD:
				return FORWARD;
				break;
			case EDirections.BACK:
				return BACK;
				break;
			case EDirections.RIGHT:
				return RIGHT;
				break;
			case EDirections.LEFT:
				return LEFT;
				break;
			case EDirections.UP:
				return UP;
				break;
			case EDirections.DOWN:
				return DOWN;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(Direction), Direction, null);
		}
	}
}