﻿using System.Collections.Generic;
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

public enum EOrientationsAdvanced
{
    FORWARDRIGHT,
    FORWARDLEFT,
    BACKRIGHT,
    BACKLEFT
}


public static class Orientations
{

    // Old orientation values
    public static Dictionary<EOrientations, Vector3Int> OrientationEulers = new Dictionary<EOrientations, Vector3Int>
    {
        {
            EOrientations.FORWARD, Vector3Int.zero
        },
        {
            EOrientations.BACK, new Vector3Int(0, 180, 0)
        },
        {
            EOrientations.RIGHT, new Vector3Int(0, 90, 0)
        },
        {
            EOrientations.LEFT,  new Vector3Int(0, -90, 0)
        },
        {
            EOrientations.UP, new Vector3Int(-90, 0, 0)
        },
        {
            EOrientations.DOWN, new Vector3Int(90, 0, 0)
        },
        {
            EOrientations.NULL, Vector3Int.zero
        }
    };

    public static Dictionary<EOrientations, Vector3Int> OrientationUnitVectors = new Dictionary<EOrientations, Vector3Int>
    {
        {
            EOrientations.FORWARD, new Vector3Int(0, 0, 1)
        },
        {
            EOrientations.BACK, new Vector3Int(0, 0, -1)
        },
        {
            EOrientations.RIGHT, Vector3Int.right
        },
        {
            EOrientations.LEFT, Vector3Int.left
        },
        {
            EOrientations.UP, Vector3Int.up
        },
        {
            EOrientations.DOWN, Vector3Int.down
        },
        {
            EOrientations.NULL, Vector3Int.zero
        }
    };

    public static Dictionary<EOrientationsAdvanced, Vector3Int> OrientationUnitVectorsAdvanced = new Dictionary<EOrientationsAdvanced, Vector3Int>
    {
        {
            EOrientationsAdvanced.FORWARDRIGHT, (new Vector3Int(0, 0, 1) + Vector3Int.right)
        },
        {
            EOrientationsAdvanced.FORWARDLEFT,  (new Vector3Int(0, 0, 1) + Vector3Int.left)
        },
        {
            EOrientationsAdvanced.BACKRIGHT, (new Vector3Int(0, 0, -1) - Vector3Int.right)
        },
        {
            EOrientationsAdvanced.BACKLEFT, (new Vector3Int(0, 0, -1) - Vector3Int.left)
        }
    };


    public static Dictionary<char, EOrientations> OrientationByChar = new Dictionary<char, EOrientations>
    {
        {
            'F', EOrientations.FORWARD
        },
        {
            'B', EOrientations.BACK
        },
        {
            'R', EOrientations.RIGHT
        },
         {
            'L', EOrientations.LEFT
        },
        {
            'U', EOrientations.UP
        },
        {
            'D', EOrientations.DOWN
        }
    };

    public static EOrientations RotateCounterClockwise(EOrientations orientation)
    {
        if (orientation == EOrientations.FORWARD) return EOrientations.RIGHT;
        if (orientation == EOrientations.RIGHT) return EOrientations.BACK;
        if (orientation == EOrientations.BACK) return EOrientations.LEFT;
        if (orientation == EOrientations.LEFT) return EOrientations.FORWARD;

        return EOrientations.NULL;
    }

    public static EOrientations CharToOrientation(char character)
    {
        OrientationByChar.TryGetValue(character, out EOrientations orientation);
        return orientation;
    }

    public static EOrientations FlipOrientation(EOrientations input)
    {
        if (input == EOrientations.FORWARD) return EOrientations.BACK;
        if (input == EOrientations.BACK) return EOrientations.FORWARD;
        if (input == EOrientations.RIGHT) return EOrientations.LEFT;
        if (input == EOrientations.LEFT) return EOrientations.RIGHT;
        if (input == EOrientations.UP) return EOrientations.DOWN;
        if (input == EOrientations.DOWN) return EOrientations.UP;
        return EOrientations.NULL;
    }

    public static Vector3Int CharToEulerRotation(char character)
    {
        return ToRotationEuler(CharToOrientation(character));
    }

    public static char ToChar(EOrientations orientation)
    {
        char value = ' ';

        foreach (KeyValuePair<char, EOrientations> pair in OrientationByChar)
        {
            if (pair.Value == orientation)
            {
                value = pair.Key;
                break;
            }
        }
        return value;
    }

    public static Vector3Int ToRotationEuler(EOrientations orientation)
    {
        OrientationEulers.TryGetValue(orientation, out Vector3Int rotationEuler);
        return rotationEuler;
    }

    public static Vector3Int ToUnitVector(EOrientations orientation)
    {
        OrientationUnitVectors.TryGetValue(orientation, out Vector3Int unitVector);
        return unitVector;
    }

    public static EOrientations DirToOrientation(Vector3Int directionUnitVector)
    {
        EOrientations value = EOrientations.NULL;

        foreach (KeyValuePair<EOrientations, Vector3Int> pair in OrientationUnitVectors)
        {
            if (pair.Value == directionUnitVector)
            {
                value = pair.Key;
                break;
            }
        }
        return value;
    }

    public static EOrientations EulerToOrientation(Vector3Int rotationVector)
    {
        int rotationValue = rotationVector.y;

        // Get rid of giant rotated 360 spinning wheel bro's.
        rotationValue %= 360;

        if (rotationValue % 360 == 0)
        {
            return EOrientations.FORWARD;
        }

        if (rotationValue % 180 == 0)
        {
            return EOrientations.BACK;
        }

        if (rotationValue % 90 == 0)
        {
            if (rotationValue == -90 || rotationValue == 270)
            {
                return EOrientations.LEFT;
            }

            if (rotationValue == 90 || rotationValue == -270)
            {
                return EOrientations.RIGHT;
            }
        }

        Debug.Log(rotationValue);

        return EOrientations.NULL;
    }
}