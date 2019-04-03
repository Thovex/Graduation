using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

[System.Serializable]
public enum EOrientations
{
    FORWARD,
    BACK,
    RIGHT,
    LEFT,
    UP,
    DOWN,
    NULL
};

[System.Serializable]
public enum EDirection
{
    FORWARD,
    RIGHT,
    BACK,
    LEFT,
    NULL
}

public static class Orientations
{
    private static readonly Vector3Int FORWARD = new Vector3Int(0, 0, 1);
    private static readonly Vector3Int BACK = new Vector3Int(0, 0, -1);

    private static readonly Vector3Int RIGHT = Vector3Int.right;
    private static readonly Vector3Int LEFT = Vector3Int.left;

    private static readonly Vector3Int UP = Vector3Int.up;
    private static readonly Vector3Int DOWN = Vector3Int.down;

    private static readonly Vector3Int FORWARD_ROTATION = Vector3Int.zero;
    private static readonly Vector3Int BACK_ROTATION = new Vector3Int(0, 180, 0);

    private static readonly Vector3Int RIGHT_ROTATION = new Vector3Int(0, 90, 0);
    private static readonly Vector3Int LEFT_ROTATION = new Vector3Int(0, -90, 0);

    private static readonly Vector3Int UP_ROTATION = new Vector3Int(-90, 0, 0);
    private static readonly Vector3Int DOWN_ROTATION = new Vector3Int(90, 0, 0);

    public static readonly Vector3Int[] Dirs = { FORWARD, BACK, RIGHT, LEFT, UP, DOWN };

    public static readonly Vector3Int[] Rots = { FORWARD_ROTATION, BACK_ROTATION, RIGHT_ROTATION, LEFT_ROTATION, UP_ROTATION, DOWN_ROTATION };
    public static Vector3Int ReturnRotationEulerFromChar(char character)
    {
        switch (character)
        {
            case 'F':
            {
                return FORWARD_ROTATION;
            }
            case 'B':
            {
                return BACK_ROTATION;
            }
            case 'R':
            {
                    return RIGHT_ROTATION;
            }
            case 'L':
            {
                return LEFT_ROTATION;
            }
            case 'U':
            {
                return UP_ROTATION;
            }
            case 'D':
            {
                return DOWN_ROTATION;
            }
        }

        return Vector3Int.zero;
    }

    public static Vector3Int ReturnDirectionVal(EOrientations orientation)
    {
        switch (orientation)
        {
            case EOrientations.FORWARD:
                return FORWARD;
            case EOrientations.BACK:
                return BACK;
            case EOrientations.RIGHT:
                return RIGHT;
            case EOrientations.LEFT:
                return LEFT;
            case EOrientations.UP:
                return UP;
            case EOrientations.DOWN:
                return DOWN;
            default:
                throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
        }
    }

    public static EOrientations ReturnOrientationVal(Vector3Int directionVector)
    {
        if (directionVector == FORWARD) return EOrientations.FORWARD;
        if (directionVector == BACK) return EOrientations.BACK;
        if (directionVector == RIGHT) return EOrientations.RIGHT;
        if (directionVector == LEFT) return EOrientations.LEFT;
        if (directionVector == UP) return EOrientations.UP;
        if (directionVector == DOWN) return EOrientations.DOWN;

        return EOrientations.NULL;
    }

    public static EDirection ReturnDirectionVal(Vector3Int rotationVector)
    {
        int rotationValue = rotationVector.y;

        if (rotationValue % 360 == 0)
        {
            return EDirection.FORWARD;
        }

        if (rotationValue % 180 == 0)
        {
            return EDirection.BACK;
        }

        if (rotationValue % 90 == 0)
        {
            if (rotationValue == -90 || rotationValue == 270)
            {
                return EDirection.LEFT;
            }

            if (rotationValue == 90 | rotationValue == -270)
            {
                return EDirection.RIGHT;
            }
        }

        return EDirection.NULL;
    }
}
