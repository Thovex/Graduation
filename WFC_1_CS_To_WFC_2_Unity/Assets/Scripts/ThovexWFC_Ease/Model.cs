using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Thovex.WFC
{
    public static class Directions
    {
        public static readonly Vector3Int Up = Vector3Int.up;

        public static readonly Vector3Int Left = Vector3Int.left;
        public static readonly Vector3Int Down = Vector3Int.down;
        public static readonly Vector3Int Right = Vector3Int.right;
        public static readonly Vector3Int Forward = new Vector3Int(
            Mathf.RoundToInt(Vector3.forward.x),
            Mathf.RoundToInt(Vector3.forward.y),
            Mathf.RoundToInt(Vector3.forward.z)
        );
        public static readonly Vector3Int Backward = new Vector3Int(
            Mathf.RoundToInt(Vector3.back.x),
            Mathf.RoundToInt(Vector3.back.y),
            Mathf.RoundToInt(Vector3.back.z)
        );

        public static readonly List<Vector3Int> Dirs = new List<Vector3Int>() { Up, Left, Down, Right };
    }

    class Model
    {
        private Vector3Int outputSize;
        private Dictionary<string, int> weights;
        private CompatibilityOracle compatibilityOracle;
        private Wavefunction wavefunction;

        public Model(Vector3Int _outputSize, Dictionary<string, int> _weights, CompatibilityOracle _compatibilityOracle)
        {
            outputSize = _outputSize;
            weights = _weights;
            compatibilityOracle = _compatibilityOracle;

            wavefunction = Wavefunction.Mk(outputSize, weights);
        }

        public List<List<List<string>>> Run()
        {
            while (!wavefunction.IsFullyCollapsed())
            {
                Iterate();
            }
            return wavefunction.GetAllCollapsed();
        }

        private void Iterate()
        {
            Vector3Int coords = MinEntropyCoords();

            wavefunction.Collapse(coords);
            Propagate(coords);

        }

        private Vector3Int MinEntropyCoords()
        {
            float minEntropy = 0;
            Vector3Int minEntropyCoords = new Vector3Int();

            System.Random random = new System.Random(Mathf.RoundToInt(UnityEngine.Random.Range(0, 5000000)));

            for (int x = 0; x < outputSize.x; x++)
            {
                for (int y = 0; y < outputSize.y; y++)
                {
                    for (int z = 0; z < outputSize.z; z++)
                    {
                        Vector3Int currentCoordinates = new Vector3Int(x, y, z);
                        if (wavefunction.Get(currentCoordinates).Keys.Length == 1)
                        {
                            continue;
                        }

                        float entropy = wavefunction.ShannonEntropy(currentCoordinates);
                        float entropyPlusNoise = entropy - (float)random.NextDouble() / 1000;

                        if (minEntropy == 0 || entropyPlusNoise < minEntropy)
                        {
                            minEntropy = entropyPlusNoise;
                            minEntropyCoords = new Vector3Int(x, y, z);
                        }
                    }
                }
            }
            return minEntropyCoords;
        }

        private void Propagate(Vector3Int coords)
        {
            Stack<Vector3Int> stack = new Stack<Vector3Int>(new Vector3Int[] { coords });

            while (stack.Count > 0)
            {
                Vector3Int currentCoords = stack.Pop();
                Coefficient currentPossibleTiles = wavefunction.Get(currentCoords);


                foreach (Vector3Int dir in Initializer.ValidDirs(currentCoords, outputSize))
                {
                    Vector3Int otherCoords = new Vector3Int(currentCoords.x + dir.x, currentCoords.y + dir.y, currentCoords.z + dir.z);


                    foreach (string other in wavefunction.Get(otherCoords).Keys)
                    {
                        string otherTile = "";
                        bool isAnyPossible = false;
                        otherTile = other;

                        foreach (string current in currentPossibleTiles.Keys)
                        {
                            if (compatibilityOracle.Check(new CompatibilityRule(current, other, dir)))
                            {
                                isAnyPossible = true;
                            }
                        }

                        if (!isAnyPossible)
                        {
                            wavefunction.Constrain(otherCoords, otherTile);
                            stack.Push(otherCoords);
                        }
                    }
                }
            }
        }
    }
}