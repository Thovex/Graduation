using System;
using System.Collections.Generic;
using UnityEngine;
using static Thovex.Utility;

[Serializable]
public class Pattern : Matrix3<Module>
{
    [SerializeField] private Dictionary<Vector3Int, List<Pattern>> _propagator;
    public Dictionary<Vector3Int, List<Pattern>> Propagator { get => _propagator; set => _propagator = value; }

    public int id;

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


            if (bit != inMatrix.GetDataAt(x, y, z))
            {
                bEqual = false;
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

    public virtual bool BitEqualMatrix(Matrix3<string> thisMatrix, Matrix3<string> otherMatrix)
    {
        for (int i = 0; i < 3; i++)
        {
            if (thisMatrix.MatrixData.GetLength(i) != otherMatrix.MatrixData.GetLength(i))
            {
                return false;
            }
        }

        bool bIsEqual = true;

        For3(thisMatrix, (x, y, z) =>
        {
            string original = thisMatrix.MatrixData[x, y, z];
            string comparison = otherMatrix.MatrixData[x, y, z];

            if (original != "null" || comparison != "null")
            {

                if (original != comparison)
                {
                    bIsEqual = false;
                }
            }
        });

        return bIsEqual;

    }

    public Matrix3<string> GenerateBits(TrainingScript training)
    {
        Matrix3<string> bits = new Matrix3<string>(new Vector3Int(SizeX, SizeY, SizeZ));

        For3(bits, (x, y, z) =>
        {
            bits.MatrixData[x, y, z] = MatrixData[x, y, z].GenerateBit(training);
        });

        return bits;
    }

    public void BuildPropagator(TrainingScript training, EOrientations orientation)
    {
        Dictionary<Vector3Int, List<Pattern>> allowedPatterns = new Dictionary<Vector3Int, List<Pattern>>();
        PropagateDefaultDirections(training, orientation, allowedPatterns);

        Propagator = allowedPatterns;

    }

    private void PropagateDefaultDirections(TrainingScript training, EOrientations orientation, Dictionary<Vector3Int, List<Pattern>> allowedPatterns)
    {

        if (orientation == EOrientations.NULL)
        {
            foreach (var direction in Orientations.OrientationUnitVectors)
            {
                if (direction.Key == EOrientations.NULL) continue;

                CreatePropagator(training, allowedPatterns, direction);
            }
        }
        else
        {
            foreach (var direction in Orientations.OrientationUnitVectors)
            {
                if (direction.Key != orientation) continue;

                CreatePropagator(training, allowedPatterns, direction);
            }
        }
    }

    private void CreatePropagator(TrainingScript training, Dictionary<Vector3Int, List<Pattern>> allowedPatterns, KeyValuePair<EOrientations, Vector3Int> direction)
    {
        Matrix3<string> bitPattern = GenerateBits(training);

        //GameObject.FindObjectOfType<MatrixVisualizer>().InMatrix.Clear();


        bitPattern.Flip(direction.Key);
        bitPattern.PushData(direction.Value);

        // GameObject.FindObjectOfType<MatrixVisualizer>().InMatrix.Add(new Tuple<Matrix3<string>, Color, int, HashSet<string>>(bitPattern, Color.yellow, 0, new HashSet<string>()));

        List<Pattern> patternsFit = new List<Pattern>();
        for (int i = 0; i < training.Patterns.Count; i++)
        {
            Matrix3<string> checkPatternBits = training.Patterns[i].GenerateBits(training);
            checkPatternBits.PushData(direction.Value);

            bool isAllowed = false;

            if (CompareBitPatterns(bitPattern, checkPatternBits))
            {
                isAllowed = true;
            }

            if (isAllowed)
            {
                patternsFit.Add(training.Patterns[i]);
                // GameObject.FindObjectOfType<MatrixVisualizer>().InMatrix.Add(new Tuple<Matrix3<string>, Color, int, HashSet<string>>(checkPatternBits, Color.green, i, new HashSet<string>()));
            }
            else
            {
                // GameObject.FindObjectOfType<MatrixVisualizer>().InMatrix.Add(new Tuple<Matrix3<string>, Color, int, HashSet<string>>(checkPatternBits, Color.red, i, new HashSet<string>()));

            }
        }
        allowedPatterns.Add(direction.Value, patternsFit);
    }

    public void Reflect(EOrientations axis)
    {
        if (axis == EOrientations.LEFT || axis == EOrientations.RIGHT)
        {

            Flip(EOrientations.RIGHT);

            For3(this, (x, y, z) =>
            {
                if (MatrixData[x, y, z].RotationDir == EOrientations.FORWARD || MatrixData[x, y, z].RotationDir == EOrientations.BACK)
                {
                    MatrixData[x, y, z].Scale = new Vector3Int(-1, 1, 1);
                }
                else
                {
                    MatrixData[x, y, z].RotationEuler -= new Vector3Int(0, 180, 0);
                    MatrixData[x, y, z].Scale = new Vector3Int(-1, 1, 1);
                }

            });
        }

        if (axis == EOrientations.FORWARD || axis == EOrientations.BACK)
        {
            Flip(EOrientations.RIGHT);

            For3(this, (x, y, z) =>
            {

                if (MatrixData[x, y, z].RotationDir == EOrientations.FORWARD || MatrixData[x, y, z].RotationDir == EOrientations.BACK)
                {
                    MatrixData[x, y, z].Scale = new Vector3Int(1, 1, -1);
                    MatrixData[x, y, z].RotationEuler -= new Vector3Int(0, 180, 0);

                }
                else
                {
                    MatrixData[x, y, z].Scale = new Vector3Int(1, 1, -1);
                }

            });

        }
    }

}