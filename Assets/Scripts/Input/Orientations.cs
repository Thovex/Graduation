using System.Collections.Generic;
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

    RIGHT_UP,
    RIGHT_DOWN,

    LEFT_UP,
    LEFT_DOWN,

    FORWARD_RIGHT,
    FORWARD_LEFT,

    FORWARD_UP,
    BACK_DOWN,

    FORWARD_DOWN,
    BACK_UP,

    FORWARD_UP_LEFT,
    FORWARD_DOWN_LEFT,

    FORWARD_UP_RIGHT,
    FORWARD_DOWN_RIGHT,

    BACK_RIGHT,
    BACK_LEFT,

    BACK_UP_LEFT,
    BACK_DOWN_LEFT,

    BACK_UP_RIGHT,
    BACK_DOWN_RIGHT,

    NULL
};

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

    public static Dictionary<EOrientations, Vector3Int> OrientationUnitVectorsDefaultAngles = new Dictionary<EOrientations, Vector3Int>
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
            EOrientations.RIGHT_UP, Vector3Int.right + Vector3Int.up
        },
        {
            EOrientations.RIGHT_DOWN,  Vector3Int.right + Vector3Int.down
        },
        {
            EOrientations.LEFT_UP,  Vector3Int.left + Vector3Int.up
        },
        {
            EOrientations.LEFT_DOWN,  Vector3Int.left + Vector3Int.down
        },
        {
            EOrientations.FORWARD_RIGHT, new Vector3Int(0, 0, 1) + Vector3Int.right
        },
        {
            EOrientations.FORWARD_LEFT,new Vector3Int(0, 0, 1) + Vector3Int.left
        },
        {
            EOrientations.FORWARD_UP, new Vector3Int(0, 0, 1) + Vector3Int.up
        },
        {
            EOrientations.BACK_DOWN, new Vector3Int(0, 0, -1) + Vector3Int.down
        },
        {
            EOrientations.FORWARD_DOWN, new Vector3Int(0, 0, 1) + Vector3Int.down
        },
        {
            EOrientations.BACK_UP, new Vector3Int(0, 0, -1) + Vector3Int.up
        },
        {
            EOrientations.FORWARD_UP_LEFT, new Vector3Int(0, 0, 1) + Vector3Int.up + Vector3Int.left
        },
        {
            EOrientations.FORWARD_DOWN_LEFT, new Vector3Int(0, 0, 1) + Vector3Int.down + Vector3Int.left
        },
        {
            EOrientations.FORWARD_UP_RIGHT, new Vector3Int(0, 0, 1) + Vector3Int.up + Vector3Int.right
        },
        {
            EOrientations.FORWARD_DOWN_RIGHT, new Vector3Int(0, 0, 1) + Vector3Int.down + Vector3Int.right
        },
        {
            EOrientations.BACK_RIGHT, new Vector3Int(0, 0, -1) + Vector3Int.right
        },
        {
            EOrientations.BACK_LEFT, new Vector3Int(0, 0, -1) + Vector3Int.left
        },
        {
            EOrientations.BACK_UP_LEFT, new Vector3Int(0, 0, -1) + Vector3Int.up + Vector3Int.left
        },
        {
            EOrientations.BACK_DOWN_LEFT, new Vector3Int(0, 0, -1) + Vector3Int.down + Vector3Int.left
        },
        {
            EOrientations.BACK_UP_RIGHT,  new Vector3Int(0, 0, -1) + Vector3Int.up + Vector3Int.right
        },
        {
            EOrientations.BACK_DOWN_RIGHT,  new Vector3Int(0, 0, -1) + Vector3Int.down + Vector3Int.right
        },
        {
            EOrientations.NULL, Vector3Int.zero
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

    public static EOrientations CharToOrientation(char character)
    {
        OrientationByChar.TryGetValue(character, out EOrientations orientation);
        return orientation;
    }

    public static EOrientations FlipOrientation(EOrientations input)
    {
        OrientationUnitVectors.TryGetValue(input, out Vector3Int value);
        return DirToOrientation(value * -1);
    }

    public static Vector3Int InvertUnitVector(Vector3Int input)
    {
        return input * -1;
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