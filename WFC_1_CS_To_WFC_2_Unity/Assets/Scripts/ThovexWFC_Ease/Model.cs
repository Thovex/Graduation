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
        public static readonly Vector2Int Up = new Vector2Int(0, 1);
        public static readonly Vector2Int Left = new Vector2Int(-1, 0);
        public static readonly Vector2Int Down = new Vector2Int(0, -1);
        public static readonly Vector2Int Right = new Vector2Int(1, 0);
        public static readonly List<Vector2Int> Dirs = new List<Vector2Int>() { Up, Left, Down, Right };
    }

    class Model
    {
        private Vector2Int outputSize;
        private Dictionary<string, int> weights;
        private CompatibilityOracle compatibilityOracle;
        private Wavefunction wavefunction;

        public Model(Vector2Int _outputSize, Dictionary<string, int> _weights, CompatibilityOracle _compatibilityOracle)
        {
            outputSize = _outputSize;
            weights = _weights;
            compatibilityOracle = _compatibilityOracle;

            wavefunction = Wavefunction.Mk(outputSize, weights);
        }

        public List<List<string>> Run()
        {
            while (!wavefunction.IsFullyCollapsed())
            {
                Iterate();
            }
            return wavefunction.GetAllCollapsed();
        }

        private void Iterate()
        {
            Vector2Int coords = MinEntropyCoords();

            wavefunction.Collapse(coords);
            Propagate(coords);

        }

        private Vector2Int MinEntropyCoords()
        {
            float minEntropy = 0;
            Vector2Int minEntropyCoords = new Vector2Int();

            System.Random random = new System.Random(Mathf.RoundToInt(UnityEngine.Random.Range(0, 5000000)));

            for (int x = 0; x < outputSize.x; x++)
            {
                for (int y = 0; y < outputSize.y; y++)
                {

                    Vector2Int currentCoordinates = new Vector2Int(x, y);
                    if (wavefunction.Get(currentCoordinates).Keys.Length == 1)
                    {
                        continue;
                    }

                    float entropy = wavefunction.ShannonEntropy(currentCoordinates);
                    float entropyPlusNoise = entropy - (float)random.NextDouble() / 1000;

                    if (minEntropy == 0 || entropyPlusNoise < minEntropy)
                    {
                        minEntropy = entropyPlusNoise;
                        minEntropyCoords = new Vector2Int(x, y);
                    }
                }
            }
            return minEntropyCoords;
        }

        private void Propagate(Vector2Int coords)
        {
            Stack<Vector2Int> stack = new Stack<Vector2Int>(new Vector2Int[] { coords });

            while (stack.Count > 0)
            {
                Vector2Int currentCoords = stack.Pop();
                Coefficient currentPossibleTiles = wavefunction.Get(currentCoords);


                foreach (Vector2Int dir in Initializer.ValidDirs(currentCoords, outputSize))
                {
                    Vector2Int otherCoords = new Vector2Int(currentCoords.x + dir.x, currentCoords.y + dir.y);


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