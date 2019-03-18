using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Thovex.WFC
{
    public class Initializer : MonoBehaviour
    {
        private Vector3Int inputMatrixSize;
        private GameObject[,,] inputMatrix;
        private string[,,] inputStringMatrix;
        private Vector3Int outputMatrixSize;

        private GameObject[,,] outputMatrix;
        private string[,,] outputStringMatrix;
        Dictionary<Vector3Int, GameObject> children = new Dictionary<Vector3Int, GameObject>();
        Dictionary<string, GameObject> conversionDictionary = new Dictionary<string, GameObject>();
        [SerializeField] private List<GameObject> resources;


        private void Start()
        {
            conversionDictionary = new Dictionary<string, GameObject>();

            var allRes = Resources.LoadAll("WFC", typeof(GameObject));

            foreach (var resource in allRes)
            {
                if (resource != null)
                {
                    if (resource.GetType() == typeof(GameObject))
                    {
                        GameObject r = (GameObject)resource;

                        //resources.Add(r);
                    }
                }
            }

            InitializeMatrixValues();
            CompatibilityData data = ParseExampleMatrix(GameObjectMatrixToStringMatrix());
            CompatibilityOracle compatibilityOracle = new CompatibilityOracle(data.Compatibilities);

            outputMatrixSize = new Vector3Int(10, 5, 10);


            StartCoroutine(RenderOutput(data, compatibilityOracle));
        }

        private char DefineNewChar(int index)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
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

        private string[,,] GameObjectMatrixToStringMatrix()
        {
            int maxX = inputMatrix.GetLength(0), maxY = inputMatrix.GetLength(1), maxZ = inputMatrix.GetLength(2);

            inputStringMatrix = new string[maxX, maxY, maxZ];

            HashSet<string> currentIds = new HashSet<string>();

            int index = 0;

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        if (currentIds.Add(inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId))
                        {
                            string newChar = DefineNewChar(index).ToString();
                            conversionDictionary.Add(newChar, GetResourceById(inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId));
                            index++;
                        }
                    }
                }
            }

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {

                        inputStringMatrix[x, y, z] = PrefabToString(GetResourceById(inputMatrix[x, y, z].GetComponent<UniqueId>().uniquePrefabId));
                    }
                }
            }

            return inputStringMatrix;
        }

        private void InitializeMatrixValues()
        {
            // Algorithm:
            // 1. Read the input bitmap
            // Unity: Read gameobject children in list, assign coordinates and assign into 2D matrix.

            int maxX = 0, maxY = 0, maxZ = 0;

            for (int i = 0; i < transform.childCount; i++)
            {
                // Calculate maxX and maxY based on int 2D locations.
                Vector3 location = transform.GetChild(i).transform.localPosition;
                Vector3Int gridLocation = new Vector3Int(Mathf.RoundToInt(location.x), Mathf.RoundToInt(location.y), Mathf.RoundToInt(location.z));

                if (gridLocation.x > maxX)
                {
                    maxX = gridLocation.x;
                }

                if (gridLocation.y > maxY)
                {
                    maxY = gridLocation.y;
                }

                if (gridLocation.y > maxZ)
                {
                    maxZ = gridLocation.z;
                }


                // Assign all children to dictionary with 2D coordinate.
                transform.GetChild(i).transform.name = gridLocation.ToString();
                children.Add(gridLocation, transform.GetChild(i).gameObject);
            }

            // Define matrix size
            inputMatrixSize = new Vector3Int(maxX + 1, maxY + 1, maxZ + 1);
            inputMatrix = new GameObject[inputMatrixSize.x, inputMatrixSize.y, inputMatrixSize.z];

            for (int x = 0; x < inputMatrixSize.x; x++)
            {
                for (int y = 0; y < inputMatrixSize.y; y++)
                {
                    for (int z = 0; z < inputMatrixSize.z; z++)
                    {
                        // Assign children to matrix location based on their 2D coordinates.
                        GameObject child;
                        children.TryGetValue(new Vector3Int(x, y, z), out child);
                        inputMatrix[x, y, z] = child;
                    }
                }
            }
        }

        private IEnumerator RenderOutput(CompatibilityData data, CompatibilityOracle compatibilityOracle)
        {
            int index = 0;

            while (true)
            {
                Model model = new Model(outputMatrixSize, data.Weights, compatibilityOracle);
                List<List<List<string>>> output = model.Run();

                outputMatrix = new GameObject[outputMatrixSize.x, outputMatrixSize.y, outputMatrixSize.z];
                outputStringMatrix = new string[outputMatrixSize.x, outputMatrixSize.y, outputMatrixSize.z];

                for (int x = 0; x < output.Count; x++)
                {
                    for (int y = 0; y < output[x].Count; y++)
                    {
                        for (int z = 0; z < output[x][y].Count; z++)
                        {
                            outputStringMatrix[x, y, z] = output[x][y][z];
                            outputMatrix[x, y, z] = Instantiate(StringToPrefab(output[x][y][z]), 
                            new Vector3(x + ((index * outputMatrixSize.x) + (index * 1)), y, z), Quaternion.identity);
                        }
                    }
                }

                index++;

                yield return new WaitForSeconds(20   );

            }
        }
        static CompatibilityData ParseExampleMatrix(string[,,] matrix)
        {

            Vector3Int matrixSize = new Vector3Int(matrix.GetLength(0), matrix.GetLength(1), matrix.GetLength(2));

            Dictionary<string, int> weights = new Dictionary<string, int>();
            List<CompatibilityRule> compatibilities = new List<CompatibilityRule>();

            for (int x = 0; x < matrixSize.x; x++)
            {
                for (int y = 0; y < matrixSize.y; y++)
                {
                    for (int z = 0; z < matrixSize.z; z++)
                    {
                        string currentTile = matrix[x, y, z];

                        Vector3Int matrixCoordinate = new Vector3Int(x, y, z);

                        if (!weights.ContainsKey(currentTile))
                        {
                            weights.Add(currentTile, 0);
                        }

                        weights[currentTile] += 1;

                        foreach (Vector3Int dir in ValidDirs(matrixCoordinate, matrixSize))
                        {
                            string otherTile = matrix[x + dir.x, y + dir.y, z + dir.z];

                            CompatibilityRule newRule = new CompatibilityRule(currentTile, otherTile, dir);

                            if (!compatibilities.Contains(newRule))
                            {
                                compatibilities.Add(newRule);
                            }
                        }
                    }
                }
            }


            return new CompatibilityData(weights, compatibilities);
        }

        public static List<Vector3Int> ValidDirs(Vector3Int matrixCoordinate, Vector3Int matrixSize)
        {

            List<Vector3Int> directions = new List<Vector3Int>();

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