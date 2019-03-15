using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Thovex.WFC
{
    public class Initializer : SerializedMonoBehaviour
    {
        [SerializeField] private Vector2Int inputMatrixSize;
        [SerializeField] [TableMatrix(IsReadOnly = true)] private GameObject[,] inputMatrix;
        [SerializeField] [TableMatrix(IsReadOnly = true)] private string[,] inputStringMatrix;
        [SerializeField] private Vector2Int outputMatrixSize;

        private GameObject[,] outputMatrix;
        [SerializeField] [TableMatrix(IsReadOnly = true)] private string[,] outputStringMatrix;
        [SerializeField] Dictionary<Vector2Int, GameObject> children = new Dictionary<Vector2Int, GameObject>();
        [SerializeField] Dictionary<string, GameObject> conversionDictionary = new Dictionary<string, GameObject>();
        [SerializeField] private List<GameObject> resources;


        private void Start()
        {
            conversionDictionary = new Dictionary<string, GameObject>();

            var allRes = Resources.LoadAll("WFC", typeof(GameObject));

            foreach (var resource in allRes)
            {
                resources.Add((GameObject)resource);
            }

            InitializeMatrixValues();
            CompatibilityData data = ParseExampleMatrix(GameObjectMatrixToStringMatrix());
            CompatibilityOracle compatibilityOracle = new CompatibilityOracle(data.Compatibilities);

            outputMatrixSize = new Vector2Int(25, 25);


            StartCoroutine(RenderOutput(data, compatibilityOracle));
        }

        private char DefineNewChar(int index)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return chars[index];
        }

        private string PrefabToString(GameObject prefab)
        {
            foreach (KeyValuePair<string, GameObject> entry in conversionDictionary)
            {
                if (entry.Value == prefab.gameObject)
                {
                    return entry.Key;
                }
            }
            return "";
        }

        private GameObject StringToPrefab(string s)
        {
            GameObject g;
            conversionDictionary.TryGetValue(s, out g);
            return g;
        }

        private GameObject GetResourceById(string id)
        {
            foreach (GameObject g in resources)
            {
                if (g.GetComponent<UniqueId>().uniquePrefabId == id)
                {
                    return g;
                }
            }
            return null;
        }

        private string[,] GameObjectMatrixToStringMatrix()
        {
            int maxX = inputMatrix.GetLength(0), maxY = inputMatrix.GetLength(1);
            inputStringMatrix = new string[maxX, maxY];

            HashSet<string> currentIds = new HashSet<string>();

            int index = 0;

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    if (currentIds.Add(inputMatrix[x, y].GetComponent<UniqueId>().uniquePrefabId))
                    {
                        string newChar = DefineNewChar(index).ToString();
                        conversionDictionary.Add(newChar, GetResourceById(inputMatrix[x, y].GetComponent<UniqueId>().uniquePrefabId));
                        index++;
                    }
                }
            }

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    inputStringMatrix[x, y] = PrefabToString(GetResourceById(inputMatrix[x, y].GetComponent<UniqueId>().uniquePrefabId));
                }
            }

            return inputStringMatrix;
        }

        private void InitializeMatrixValues()
        {
            // Algorithm:
            // 1. Read the input bitmap
            // Unity: Read gameobject children in list, assign coordinates and assign into 2D matrix.

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
            inputMatrixSize = new Vector2Int(maxX + 1, maxY + 1);
            inputMatrix = new GameObject[inputMatrixSize.x, inputMatrixSize.y];

            for (int x = 0; x < inputMatrixSize.x; x++)
            {
                for (int y = 0; y < inputMatrixSize.y; y++)
                {
                    // Assign children to matrix location based on their 2D coordinates.
                    GameObject child;
                    children.TryGetValue(new Vector2Int(x, y), out child);
                    inputMatrix[x, y] = child;
                }
            }
        }

        private IEnumerator RenderOutput(CompatibilityData data, CompatibilityOracle compatibilityOracle)
        {



            int index = 0;

            while (true)
            {
            Model model = new Model(outputMatrixSize, data.Weights, compatibilityOracle);
            List<List<string>> output = model.Run();

                outputMatrix = new GameObject[outputMatrixSize.x, outputMatrixSize.y];
                outputStringMatrix = new string[outputMatrixSize.x, outputMatrixSize.y];

                for (int x = 0; x < output.Count; x++)
                {
                    for (int y = 0; y < output[x].Count; y++)
                    {
                        outputStringMatrix[x, y] = output[x][y];
                        outputMatrix[x, y] = Instantiate(StringToPrefab(output[x][y]), new Vector3(x + ((index * outputMatrixSize.x ) + (index*1)), 0, y), Quaternion.identity);
                    }
                }

                index++;

                yield return new WaitForSeconds(1000);

            }
        }
        static CompatibilityData ParseExampleMatrix(string[,] matrix)
        {

            Vector2Int matrixSize = new Vector2Int(matrix.GetLength(0), matrix.GetLength(1));

            Dictionary<string, int> weights = new Dictionary<string, int>();
            List<CompatibilityRule> compatibilities = new List<CompatibilityRule>();

            for (int x = 0; x < matrixSize.x; x++)
            {
                for (int y = 0; y < matrixSize.y; y++)
                {

                    string currentTile = matrix[x, y];

                    Vector2Int matrixCoordinate = new Vector2Int(x, y);

                    if (!weights.ContainsKey(currentTile))
                    {
                        weights.Add(currentTile, 0);
                    }

                    weights[currentTile] += 1;

                    foreach (Vector2Int dir in ValidDirs(matrixCoordinate, matrixSize))
                    {
                        string otherTile = matrix[x + dir.x, y + dir.y];

                        CompatibilityRule newRule = new CompatibilityRule(currentTile, otherTile, dir);

                        if (!compatibilities.Contains(newRule))
                        {
                            compatibilities.Add(newRule);
                        }
                    }
                }
            }


            return new CompatibilityData(weights, compatibilities);
        }

        public static List<Vector2Int> ValidDirs(Vector2Int matrixCoordinate, Vector2Int matrixSize)
        {

            List<Vector2Int> directions = new List<Vector2Int>();

            if (matrixCoordinate.x > 0)
            {
                directions.Add(Directions.Left);
            }

            if (matrixCoordinate.x < matrixSize.x - 1)
            {
                directions.Add(Directions.Right);
            }

            if (matrixCoordinate.y > 0)
            {
                directions.Add(Directions.Down);
            }

            if (matrixCoordinate.y < matrixSize.y - 1)
            {
                directions.Add(Directions.Up);
            }
            return directions;
        }
    }
}