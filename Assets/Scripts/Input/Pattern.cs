﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static Thovex.Utility;

// used for testing purposes
[Serializable]
public struct ViewDict
{

}

[Serializable]
public class Pattern : Matrix3<Module>
{
    [SerializeField] private Dictionary<Vector3Int, List<int>> _propagator;
    public Dictionary<Vector3Int, List<int>> Propagator { get => _propagator; set => _propagator = value; }
    public int N { get => n; set => n = value; }

    private int n = 2;


    public Pattern(int patternSize)
    {
        MatrixData = new Module[patternSize, patternSize, patternSize];
        N = patternSize;
    }

    public Pattern(Vector3Int patternSize)
    {
        MatrixData = new Module[patternSize.x, patternSize.y, patternSize.z];
        N = patternSize.x;

    }

    public Pattern(Module[,,] patternData)
    {
        MatrixData = patternData;
        N = MatrixData.GetLength(0);

    }

    public override void RotateCounterClockwise(int times)
    {
        base.RotateCounterClockwise(times);

        for (int i = 0; i < times; i++)
        {
            For3(this, (x, y, z) =>
            {
                MatrixData[x, y, z].RotationEuler += new Vector3Int(0, -90, 0);
            });
        }
    }

    public bool CompareBitPatterns(TrainingScript training, Matrix3<string> bitMatrix)
    {
        Matrix3<string> patternAsBitMatrix = GenerateBits(training);

        bool bEqual = true;

        For3(bitMatrix, (x, y, z) =>
        {
            string bit = bitMatrix.GetDataAt(x, y, z);

            if (bit != "null")
            {
                if (bit != patternAsBitMatrix.GetDataAt(x, y, z))
                {
                    bEqual = false;
                }
            }
        });

        return bEqual;
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

    public override bool IsEqualToMatrix(Matrix3<Module> otherMatrix)
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

    public Matrix3<string> GenerateBits(TrainingScript training)
    {
        Matrix3<string> bits = new Matrix3<string>(new Vector3Int(SizeX, SizeY, SizeZ));

        For3(bits, (x, y, z) => { bits.MatrixData[x, y, z] = MatrixData[x, y, z].GenerateBit(training); });

        return bits;
    }

    public void BuildPropagator(TrainingScript training)
    {
        Dictionary<Vector3Int, List<int>> allowedPatterns = new Dictionary<Vector3Int, List<int>>();

        int halfN = N / 2;

        foreach (var direction in Orientations.OrientationUnitVectors)
        {
            if (direction.Key == EOrientations.NULL) continue;

            List<int> patternsFit = new List<int>();
            for (int i = 0; i < training.Patterns.Count; i++)
            {

                Pattern sidePattern = new Pattern(N);

                For3 (sidePattern, (x, y, z) =>
                {
                    sidePattern.MatrixData[x, y, z] =
                });

                if (sidePattern.CompareBitPatterns(training, training.Patterns[i].GenerateBits(training)))
                {
                    patternsFit.Add(i);
                }

            }

            allowedPatterns.Add(direction.Value * halfN, patternsFit);
        }






        //         foreach (var direction in Orientations.OrientationUnitVectors)
        //         {
        //             if (direction.Key == EOrientations.NULL) continue;
        // 
        //             Vector3Int middlePointWithDir = direction.Value * halfN;
        //             allowedPatterns.Add(middlePointWithDir, new List<Pattern>());
        // 
        //             foreach (var direction2 in Orientations.OrientationUnitVectors)
        //             {
        //                 if (direction2.Key == EOrientations.NULL) continue;
        //                 if (direction2.Key == direction.Key) continue;
        //                 if (direction2.Key == Orientations.FlipOrientation(direction.Key)) continue;
        // 
        //                 Vector3Int DirWithDir = middlePointWithDir + direction2.Value * halfN;
        // 
        //                 try
        //                 {
        //                     allowedPatterns.Add(DirWithDir, new List<Pattern>());
        //                 }
        //                 catch (Exception) { }
        //             }
        //         }


        //}
        Propagator = allowedPatterns;

    }
}