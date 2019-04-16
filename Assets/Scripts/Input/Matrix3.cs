﻿using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditorInternal;
using UnityEngine;
using static Thovex.Utility;

[Serializable]
public class Matrix3<T>
{
    public Matrix3() { }

    public Matrix3(int matrixSize)
    {
        MatrixData = new T[matrixSize, matrixSize, matrixSize];
    }

    public Matrix3(Vector3Int matrixSize)
    {
        MatrixData = new T[matrixSize.x, matrixSize.y, matrixSize.z];
    }

    private T[,,] _matrixData;

    public T[,,] MatrixData {
        get => _matrixData;
        set {
            _matrixData = value;
            SetSize();
        }
    }

    public int SizeX { get; protected set; }
    public int SizeY { get; protected set; }
    public int SizeZ { get; protected set; }
    public Vector3Int Size { get; protected set; }

    protected void SetSize()
    {
        SizeX = _matrixData.GetLength(0);
        SizeY = _matrixData.GetLength(1);
        SizeZ = _matrixData.GetLength(2);

        Size = new Vector3Int(SizeX, SizeY, SizeZ);
    }

    public bool Valid(Vector3Int coord)
    {
        if (typeof(T) == typeof(Module))
        {
            Module dataModule = (Module)Convert.ChangeType(GetDataAt(coord), typeof(Module));

            if (dataModule.Prefab) return true;

            return false;
        }
        else
        {
            if (GetDataAt(coord) == default)
            {
                return false;
            }
            return true;
        }
    }

    public bool Valid(int x, int y, int z)
    {
        return Valid(new Vector3Int(x, y, z));
    }

    public T GetDataAt(Vector3Int coordinate)
    {
        try
        {
            return MatrixData[coordinate.x, coordinate.y, coordinate.z];
        }
        catch (Exception)
        {
            return default;
        }
    }

    public T GetDataAt(int x, int y, int z)
    {
        return GetDataAt(new Vector3Int(x, y, z));
    }

    public void SetDataAt(Vector3Int coordinate, T data)
    {
        MatrixData[coordinate.x, coordinate.y, coordinate.z] = data;
    }

    public void SetDataAt(int x, int y, int z, T data)
    {
        MatrixData[x, y, z] = data;
    }

    public bool Contains(T check, out Vector3Int coord)
    {
        bool contains = false;

        coord = Vector3Int.zero;
        Vector3Int containsCoord = Vector3Int.zero;

        For3(this, (x, y, z) =>
        {
            if (Equals(MatrixData[x, y, z], check))
            {
                contains = true;
                containsCoord = new Vector3Int(x, y, z);
            }
        });

        coord = containsCoord;
        return contains;
    }

    public bool Contains(T check)
    {
        bool contains = false;
        For3(this, (x, y, z) =>
        {
            if (Equals(MatrixData[x, y, z], check))
            {
                contains = true;
            }
        });
        return contains;
    }

    public void Clear()
    {
        MatrixData = new T[SizeX, SizeY, SizeZ];
    }

    public void Empty()
    {
        SizeX = 0;
        SizeY = 0;
        SizeZ = 0;

        Clear();
    }

    public void TransposeFlip()
    {
        //         for (int x = 0; x < width; x++)
        //         {
        //             for (int y = 0; y < height / 2 + 1; y++)
        //             {
        //                 int element = matrix[x, y];
        //                 int neighbour = matrix[x, height - y - 1];
        //                 matrix[x, y] = neighbour;
        //                 matrix[x, height - y - 1] = element;
        // 
        // 
        //             }
        //         }

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                for (int z = 0; z < SizeZ / 2 + 1; z++)
                {
                    T elem = MatrixData[x, y, z];
                    T neighbour = MatrixData[x, y, SizeZ - z - 1];
                    MatrixData[x, y, z] = neighbour;
                    MatrixData[x, y, SizeZ - z - 1] = elem;
                }
            }
        }
    }

    public virtual void RotateCounterClockwise(int times)
    {
//         TransposeFlip();
// 
//         Matrix3<T> transposedMatrix = new Matrix3<T>(new Vector3Int(Size.z, Size.y, Size.x));
// 
//         For3(this, (x, y, z) =>
//         {
//             transposedMatrix.MatrixData[z, y, x] = MatrixData[x, y, z];
//         });
// 
//         MatrixData = transposedMatrix.MatrixData;


        int MinX = 0;
        int MaxX = SizeX - 1;

        int MinZ = 0;
        int MaxZ = SizeZ - 1;

        for (int i = 0; i < times; i++)
        {
            for (int increment = 0; increment < SizeX / 2; increment++)
            {
                for (int n = 0 + increment; n < MaxX - increment; n++)
                {

                    T[,,] originalData = MatrixData;
                    T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];

                    For3(this, (x, y, z) =>
                    {
                        if (x >= MinX + increment && x <= (MaxX - 1) - increment && z == MinZ + increment)
                        {
                            copyMatrix[x + 1, y, z] = originalData[x, y, z];
                        }
                        else if (x == MaxX - increment && z >= MinZ + increment && z <= (MaxZ - 1) - increment)
                        {
                            copyMatrix[x, y, z + 1] = originalData[x, y, z];
                        }
                        else if (x >= (MinX + 1) + increment && x <= MaxX - increment && z == MaxZ - increment)
                        {
                            copyMatrix[x - 1, y, z] = originalData[x, y, z];
                        }
                        else if (x == MinX + increment && z >= (MinZ + 1) + increment && z <= MaxZ - increment)
                        {
                            copyMatrix[x, y, z - 1] = originalData[x, y, z];
                        }
                        else
                        {
                            copyMatrix[x, y, z] = originalData[x, y, z];
                        }
                    });

                    MatrixData = copyMatrix;
                }
            }
        }
    }

    // ONLY FOR N == 2 && N == 3
    // Todo support N>3



    // Example: N = 5
    // *     *     *     *     * | Matrix
    //       ^-----------^       | This code has to be created.
    // ^-----------------------^ | This code is created.

    public virtual void Flip(EOrientations orientation)
    {

        if (orientation == EOrientations.NULL)
        {
            Debug.LogError("Flipping NULL");
            return;
        }

        if (SizeX > 3)
        {
            Debug.LogError("N > 3 not implemented!");
            return;
        }

        string orientationToString = Enum.GetName(typeof(EOrientations), orientation);
        string[] orientations = orientationToString.Split('_');

        foreach (string o in orientations)
        {
            Flip(o);
        }

    }

    public void Flip(string enumToString)
    {

        if (enumToString == "FORWARD" || enumToString == "BACK")
        {
            MatrixData = FlipForwardBack();
        }

        if (enumToString == "RIGHT" || enumToString == "LEFT")
        {
            MatrixData = FlipRightLeft();
        }

        if (enumToString == "UP" || enumToString == "DOWN")
        {
            MatrixData = FlipUpDown();
        }
    }

    private T[,,] FlipRightLeft()
    {
        T[,,] originalData = MatrixData;
        T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];

        int x = SizeX - 1;
        int z = SizeZ - 1;

        for (int y = 0; y < SizeY; y++)
        {
            copyMatrix[0, y, 0] = originalData[x, y, 0];
            copyMatrix[x, y, 0] = originalData[0, y, 0];
            copyMatrix[0, y, z] = originalData[x, y, z];
            copyMatrix[x, y, z] = originalData[0, y, z];
        }

        return copyMatrix;
    }

    private T[,,] FlipForwardBack()
    {
        T[,,] originalData = MatrixData;
        T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];

        int x = SizeX - 1;
        int z = SizeZ - 1;

        for (int y = 0; y < SizeY; y++)
        {
            copyMatrix[0, y, 0] = originalData[0, y, z];
            copyMatrix[0, y, z] = originalData[0, y, 0];
            copyMatrix[x, y, 0] = originalData[x, y, z];
            copyMatrix[x, y, z] = originalData[x, y, 0];
        }

        return copyMatrix;
    }

    private T[,,] FlipUpDown()
    {
        T[,,] originalData = MatrixData;
        T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];

        int y = SizeY - 1;

        for (int x = 0; x < SizeX; x++)
        {
            for (int z = 0; z < SizeZ; z++)
            {
                copyMatrix[x, 0, z] = originalData[x, y, z];
                copyMatrix[x, y, z] = originalData[x, 0, z];
            }
        }

        return copyMatrix;
    }

    public virtual void PushData(Vector3Int direction)
    {
        T[,,] originalData = MatrixData;
        T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];


        For3(this, (x, y, z) =>
        {
            Vector3Int sweepCoord = new Vector3Int(x + -direction.x, y + -direction.y, z + -direction.z);

            if (ValidCoordinate(sweepCoord))
            {
                copyMatrix[x, y, z] = originalData[sweepCoord.x, sweepCoord.y, sweepCoord.z];
            }
            else
            {
                if (typeof(T) == typeof(string))
                {
                    copyMatrix[x, y, z] = (T)Convert.ChangeType((string)"null", typeof(T));
                }
                else
                {
                    copyMatrix[x, y, z] = default;
                }
            }
        });

        MatrixData = copyMatrix;
    }

    public bool ValidCoordinate(Vector3Int coords)
    {
        if (coords.x < 0) return false;
        if (coords.x >= SizeX) return false;

        if (coords.y < 0) return false;
        if (coords.y >= SizeY) return false;

        if (coords.z < 0) return false;
        if (coords.z >= SizeZ) return false;

        return true;
    }

    public void RotatePatternClockwise()
    {
        throw new NotImplementedException();
    }


    public virtual bool IsEqualToMatrix(Matrix3<T> otherMatrix)
    {
        throw new NotImplementedException();
    }

    public bool Equals(T data, T equal)
    {
        if (typeof(T) == typeof(Vector3Int))
        {
            Vector3Int dataVector3Int = (Vector3Int)Convert.ChangeType(data, typeof(Vector3Int));
            Vector3Int equalVector3Int = (Vector3Int)Convert.ChangeType(equal, typeof(Vector3Int));

            if (dataVector3Int == equalVector3Int) return true;
        }

        if (typeof(T) == typeof(bool))
        {
            bool databool = (bool)Convert.ChangeType(data, typeof(bool));
            bool equabool = (bool)Convert.ChangeType(equal, typeof(bool));

            if (databool == equabool) return true;
        }

        else
        {
            Debug.LogError("Cannot check Equals T == " + typeof(T).ToString() + " because it is not implemented specifically!");
            return false;
        }

        return false;
    }
}