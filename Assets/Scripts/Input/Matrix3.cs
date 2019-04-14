using System;
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

    public virtual void RotateCounterClockwise(int times)
    {
        for (int i = 0; i < times; i++)
        {

            // Todo: Replace with Quaternion rotation
            for (int n = 0; n < SizeX - 1; n++)
            {

                T[,,] originalData = MatrixData;
                T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];

                For3(this, (x, y, z) =>
                {
                    if (x < SizeX - 1 && z == 0)
                    {
                        copyMatrix[x + 1, y, z] = originalData[x, y, z];
                    }
                    else if (x == SizeX - 1 && z < SizeZ - 1)
                    {
                        copyMatrix[x, y, z + 1] = originalData[x, y, z];
                    }
                    else if (z == SizeZ - 1 && x > 0)
                    {
                        copyMatrix[x - 1, y, z] = originalData[x, y, z];
                    }
                    else if (z > 0 && x == 0)
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

    // Only supports 2D flipping (left/right & forward/back). ONLY FOR N == 2 && N == 3
    // Todo support N>3

    // Example: N = 5
    // *     *     *     *     * | Matrix
    //       ^-----------^       | This code has to be created.
    // ^-----------------------^ | This code is created.

    public virtual void Flip(EOrientations orientation)
    {
        T[,,] originalData = MatrixData;
        T[,,] copyMatrix = new T[SizeX, SizeY, SizeZ];

        if (SizeX > 3) {
            Debug.LogError("N > 3 not implemented!");
        }

        if (orientation == EOrientations.RIGHT || orientation == EOrientations.LEFT)
        {
            int x = SizeX - 1;
            int z = SizeZ - 1;

            for (int y = 0; y < SizeY; y++)
            {
                copyMatrix[0, y, 0] = originalData[x, y, 0];
                copyMatrix[x, y, 0] = originalData[0, y, 0];
                copyMatrix[0, y, z] = originalData[x, y, z];
                copyMatrix[x, y, z] = originalData[0, y, z];
            }
        }
        else if (orientation == EOrientations.FORWARD || orientation == EOrientations.BACK)
        {
            int x = SizeX - 1;
            int z = SizeZ - 1;

            for (int y = 0; y < SizeY; y++)
            {
                copyMatrix[0, y, 0] = originalData[0, y, z];
                copyMatrix[0, y, z] = originalData[0, y, 0];
                copyMatrix[x, y, 0] = originalData[x, y, z];
                copyMatrix[x, y, z] = originalData[x, y, 0];
            }
        }
        else if (orientation == EOrientations.UP || orientation == EOrientations.DOWN)
        {

            int y = SizeY - 1;

            for (int x = 0; x < SizeX; x++)
            {
                for (int z = 0; z < SizeZ; z++)
                {
                    copyMatrix[x, 0, z] = originalData[x, y, z];
                    copyMatrix[x, y, z] = originalData[x, 0, z];
                }
            }
        }
        else
        {
            Debug.Log("Flipping NULL");
            return;
        }

        MatrixData = copyMatrix;

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