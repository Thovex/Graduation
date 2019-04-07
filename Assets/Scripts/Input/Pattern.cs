using System;
using System.Collections.Generic;
using UnityEngine;
using static Thovex.Utility;

// used for testing purposes
[Serializable]
public struct ViewDict
{
    public ViewDict(Vector3Int coordinate, Module trainingData)
    {
        _coordinate = coordinate;
        _trainingData = trainingData;
    }

    [SerializeField] private Vector3Int _coordinate;
    [SerializeField] private Module _trainingData;
}

[Serializable]
public class Pattern : Matrix<Module>
{
    [SerializeField] private readonly List<ViewDict> _patternDictionary = new List<ViewDict>();

    private int _patternSizeN;

    public Pattern(int patternSizeN)
    {
        _patternSizeN = patternSizeN;
        MatrixData = new Module[_patternSizeN, _patternSizeN, _patternSizeN];

        For3(this,
            (x, y, z) => { _patternDictionary.Add(new ViewDict(new Vector3Int(x, y, z), MatrixData[x, y, z])); });
    }

    public Pattern(int patternSizeN, Module[,,] patternData)
    {
        _patternSizeN = patternSizeN;
        MatrixData = patternData;


        For3(this, (x, y, z) =>
        {
            _patternDictionary.Add(new ViewDict(new Vector3Int(x, y, z), MatrixData[x, y, z]));
        });
    }

    public override void RotatePatternCounterClockwise(int times)
    {
        base.RotatePatternCounterClockwise(times);

        for (int i = 0; i < times; i++)
        {
            For3(this, (x, y, z) => { MatrixData[x, y, z].RotationEuler += new Vector3Int(0, -90, 0); });
        }
    }

    public bool HasEqualMatrixValue(Vector3Int coord, Module comparison, bool skipCheck = false)
    {
        if (!skipCheck)
        {
            if (coord.x < 0 || coord.x > SizeX) return false;
            if (coord.y < 0 || coord.y > SizeY) return false;
            if (coord.z < 0 || coord.z > SizeZ) return false;
        }

        bool bIsEqual = true;

        Module original = MatrixData[coord.x, coord.y, coord.z];

        if (original.Prefab != comparison.Prefab)
        {
            bIsEqual = false;
        }

        if (original.RotationEuler != comparison.RotationEuler)
        {
            bIsEqual = false;
        }

        return bIsEqual;
    }

    public override bool IsEqualToMatrix(Matrix<Module> otherMatrix)
    {
        for (int i = 0; i < 3; i++)
        {
            if (MatrixData.GetLength(i) != otherMatrix.MatrixData.GetLength(i))
            {
                return false;
            }
        }

        bool bIsEqual = true;

        For3(this, (x, y, z) =>
        {
            Module original = MatrixData[x, y, z];
            Module comparison = otherMatrix.MatrixData[x, y, z];

            if (original.Prefab != comparison.Prefab)
            {
                bIsEqual = false;
            }

            if (original.RotationEuler != comparison.RotationEuler)
            {
                bIsEqual = false;
            }
        });

        return bIsEqual;
    }

    public Matrix<string> GenerateBits(TrainingScript training)
    {
        Matrix<string> bits = new Matrix<string>(new Vector3Int(SizeX, SizeY, SizeZ));

        For3(bits, (x, y, z) => { bits.MatrixData[x, y, z] = MatrixData[x, y, z].GenerateBit(training); });

        return bits;
    }
}