using System;
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

    public List<Tuple<Matrix3<string>, Vector3Int>> debugPatterns = new List<Tuple<Matrix3<string>, Vector3Int>>();

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
        return CompareBitPatterns(GenerateBits(training), bitMatrix);
    }

    public bool CompareBitPatterns(Matrix3<string> inMatrix, Matrix3<string> bitMatrix)
    {
        bool bEqual = true;

        For3(bitMatrix, (x, y, z) =>
        {
            string bit = bitMatrix.GetDataAt(x, y, z);

            // Check?
            if (bit != "null" && bit != "0S")
            {

                if (bit != inMatrix.GetDataAt(x, y, z))
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

        debugPatterns.Clear();

        BuildForStandardOrientations(training, allowedPatterns, halfN);
        BuildForAdvancedOrientations(training, allowedPatterns, halfN);


        // Propagator in training script (using sketch 1)
        // TODO VALIDATE ALL DATA USING "WHAT THE FUCK IS EVEN ALLOWED HERE" 
        // TODO OTHER ANGLES :-D

        Propagator = allowedPatterns;

    }

    private void BuildForStandardOrientations(TrainingScript training, Dictionary<Vector3Int, List<int>> allowedPatterns, int halfN)
    {
        foreach (var direction in Orientations.OrientationUnitVectors)
        {
            if (direction.Key == EOrientations.NULL) continue;

            List<int> patternsFit = new List<int>();
            for (int i = 0; i < training.Patterns.Count; i++)
            {
                Matrix3<string> sidePatternBits = GenerateBits(training);
                Matrix3<string> sidePatternBitsCopy = new Matrix3<string>(sidePatternBits.Size);
                sidePatternBits.PushData(direction.Value);

                Matrix3<string> checkPatternBits = training.Patterns[i].GenerateBits(training);
                checkPatternBits.Flip(direction.Key);
                checkPatternBits.PushData(direction.Value);

                if (this.CompareBitPatterns(sidePatternBits, checkPatternBits))
                {
                    patternsFit.Add(i);
                }
            }
            allowedPatterns.Add(direction.Value * halfN, patternsFit);
        }
    }

    private void BuildForAdvancedOrientations(TrainingScript training, Dictionary<Vector3Int, List<int>> allowedPatterns, int halfN)
    {
        foreach (var direction in Orientations.OrientationUnitVectorsAdvanced)
        {
            if (direction.Key == EOrientationsAdvanced.NULL) continue;

            List<int> patternsFit = new List<int>();
            for (int i = 0; i < training.Patterns.Count; i++)
            {
                Matrix3<string> sidePatternBits = GenerateBits(training);
                Matrix3<string> sidePatternBitsCopy = new Matrix3<string>(sidePatternBits.Size);

                sidePatternBits.PushData(direction.Value);

                Matrix3<string> checkPatternBits = training.Patterns[i].GenerateBits(training);
                checkPatternBits.PushData(direction.Value);
                checkPatternBits.RotateCounterClockwise(2);


                if (this.CompareBitPatterns(sidePatternBits, checkPatternBits))
                {
                    patternsFit.Add(i);
                }
            }
            allowedPatterns.Add(direction.Value * halfN, patternsFit);
        }
    }



}