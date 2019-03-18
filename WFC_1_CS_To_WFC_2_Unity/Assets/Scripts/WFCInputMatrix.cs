using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Thovex.WFC;

public class WFCInputMatrix : SerializedMonoBehaviour
{
    [SerializeField] private Vector2Int matrixSize;
    [SerializeField] [TableMatrix(IsReadOnly = true)] private GameObject[,] inputMatrix;
    [SerializeField] Dictionary<Vector2Int, GameObject> children = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] Dictionary<GameObject, int> weights = new Dictionary<GameObject, int>();
    [SerializeField] List<CompatibilityRule> compatibilities = new List<CompatibilityRule>();
    [SerializeField] Dictionary<int, GameObject[,]> patterns = new Dictionary<int, GameObject[,]>();

    [SerializeField] private int N = 2;
    void Start()
    {
        InitializeMatrixValues();
        ParseMatrix();
        CountNxNPatterns();
    }

    private void CountNxNPatterns()
    {
        int patternCount = matrixSize.x / N * matrixSize.y / N;

        int currentPatternIndex = 0;
        Vector2Int currentCoordinate = new Vector2Int(0, 0);
        GameObject[,] pattern = new GameObject[N, N];

        for (int x = currentCoordinate.x; x < currentCoordinate.x + N; x++)
        {
            for (int y = currentCoordinate.y; y < currentCoordinate.y + N; y++)
            {
                pattern[x,y] = inputMatrix[currentCoordinate.x, currentCoordinate.y];
            }
        }

        if (currentCoordinate.x + N > matrixSize.x)
        {
            currentCoordinate.y += N;
        } else {
            currentCoordinate.x += N;
        }

        patterns.Add(currentPatternIndex, pattern);
        currentPatternIndex++;

        // todo fix

    }

    private void ParseMatrix()
    {

        for (int x = 0; x < matrixSize.x; x++)
        {
            for (int y = 0; y < matrixSize.y; y++)
            {
                GameObject currentTile = inputMatrix[x, y];
                Vector2Int matrixCoordinate = new Vector2Int(x, y);

                if (!weights.ContainsKey(currentTile))
                {
                    weights.Add(currentTile, 0);
                }

                weights[currentTile] += 1;

                foreach (Vector2Int dir in ValidDirs(matrixCoordinate, matrixSize))
                {
                    GameObject otherTile = inputMatrix[x + dir.x, y + dir.y];
                    //CompatibilityRule newRule = new CompatibilityRule(currentTile, otherTile, dir);

                   // if (!compatibilities.Contains(newRule))
                    //{
                      //  compatibilities.Add(newRule);
                   // }
                }
            }
        }
    }

    public List<Vector3Int> ValidDirs(Vector2Int matrixCoordinate, Vector2Int matrixSize)
    {
        List<Vector3Int> directions = new List<Vector3Int>();
        if (matrixCoordinate.x > 0)                 directions.Add(Directions.Left);
        if (matrixCoordinate.x < matrixSize.x - 1)  directions.Add(Directions.Right);
        if (matrixCoordinate.y > 0)                 directions.Add(Directions.Down);
        if (matrixCoordinate.y < matrixSize.y- 1)   directions.Add(Directions.Up);
        return directions;
    }

    // Algorithm:
    // 1. Read the input bitmap
    // Unity: Read gameobject children in list, assign coordinates and assign into 2D matrix.
    private void InitializeMatrixValues()
    {
        int maxX = 0, maxY = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            // Calculate maxX and maxY based on int 2D locations.
            Vector3 location = transform.GetChild(i).transform.localPosition;
            Vector2Int gridLocation = new Vector2Int(Mathf.RoundToInt(location.x), Mathf.RoundToInt(location.y));

            if (gridLocation.x > maxX)
            {
                maxX = gridLocation.x;
            }

            if (gridLocation.y > maxY)
            {
                maxY = gridLocation.y;
            }

            // Assign all children to dictionary with 2D coordinate.
            transform.GetChild(i).transform.name = gridLocation.ToString();
            children.Add(gridLocation, transform.GetChild(i).gameObject);
        }

        // Define matrix size
        matrixSize = new Vector2Int(maxX + 1, maxY + 1);
        inputMatrix = new GameObject[matrixSize.x, matrixSize.y];

        for (int x = 0; x < matrixSize.x; x++)
        {
            for (int y = 0; y < matrixSize.y; y++)
            {
                // Assign children to matrix location based on their 2D coordinates.
                GameObject child;
                children.TryGetValue(new Vector2Int(x, y), out child);
                inputMatrix[x, y] = child;
            }
        }
    }
}
