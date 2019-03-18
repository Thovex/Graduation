
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
namespace Thovex.WFC
{
    public struct Coefficient
    {
        public Coefficient(string[] _keys)
        {
            Keys = _keys;
        }

        public override string ToString()
        {
            return string.Join(",", Keys);
        }

        public string[] Keys { get; set; }
    }

    class Wavefunction
    {
        private List<List<List<Coefficient>>> coefficients;
        private Dictionary<string, int> weights;

        public Wavefunction(List<List<List<Coefficient>>> _coefficients, Dictionary<string, int> _weights)
        {
            coefficients = _coefficients;
            weights = _weights;
        }

        static public Wavefunction Mk(Vector3Int size, Dictionary<string, int> weights)
        {
            List<List<List<Coefficient>>> Coefficients = InitCoefficients(size, weights.Keys);
            return new Wavefunction(Coefficients, weights);

        }


        private static List<List<List<Coefficient>>> InitCoefficients(Vector3Int size, Dictionary<string, int>.KeyCollection keys)
        {
            List<List<List<Coefficient>>> Coefficients = new List<List<List<Coefficient>>>();
            Coefficient coefficient = new Coefficient(keys.ToArray());


            for (int x = 0; x < size.x; x++)
            {
                Coefficients.Add(new List<List<Coefficient>>());

                for (int y = 0; y < size.y; y++)
                {
                    Coefficients[x].Add(new List<Coefficient>());

                    for (int z = 0; z < size.z; z++)
                    {
                        Coefficients[x][y].Add(new Coefficient());
                        Coefficients[x][y][z] = coefficient;
                    }

                }
            }
            return Coefficients;
        }

        public bool IsFullyCollapsed()
        {
            for (int x = 0; x < coefficients.Count; x++)
            {
                for (int y = 0; y < coefficients[x].Count; y++)
                {
                    for (int z = 0; z < coefficients[x][y].Count; z++)
                    {
                        if (coefficients[x][y][z].Keys.Length > 1)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public Coefficient Get(Vector3Int coords)
        {
            return coefficients[coords.x][coords.y][coords.z];
        }

        public string GetCollapsed(Vector3Int coords)
        {
            Coefficient opts = Get(coords);

            if (opts.Keys.Length == 1)
            {
                return opts.Keys[0];
            }

            throw new Exception();
        }

        public List<List<List<string>>> GetAllCollapsed()
        {
            int width = coefficients.Count;
            int height = coefficients[0].Count;
            int depth = coefficients[0][0].Count;

            List<List<List<string>>> collapsed = new List<List<List<string>>>();

            for (int x = 0; x < width; x++)
            {
                collapsed.Add(new List<List<string>>());
                for (int y = 0; y < height; y++)
                {
                    collapsed[x].Add(new List<string>());
                    for (int z = 0; z < depth; z++)
                    {
                        collapsed[x][y].Add(GetCollapsed(new Vector3Int(x, y, z)));
                    }
                }
            }

            return collapsed;
        }

        public float ShannonEntropy(Vector3Int currentCoordinates)
        {
            int sumOfWeights = 0;
            float sumOfWeightsLogWeights = 0;

            foreach (string coefficient in coefficients[currentCoordinates.x][currentCoordinates.y][currentCoordinates.z].Keys)
            {
                int weight = weights[coefficient];
                sumOfWeights += weight;
                sumOfWeightsLogWeights += weight * (float)Math.Log(weight);
            }
            return (float)Math.Log(sumOfWeights) - (sumOfWeightsLogWeights / sumOfWeights);
        }

        public void Collapse(Vector3Int coords)
        {
            Coefficient opts = coefficients[coords.x][coords.y][coords.z];

            Dictionary<string, int> validWeights = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> item in weights)
            {
                if (opts.Keys.Contains(item.Key))
                {
                    validWeights.Add(item.Key, item.Value);
                }
            }

            int totalWeights = validWeights.Sum(x => x.Value);

            System.Random random = new System.Random(Mathf.RoundToInt(UnityEngine.Random.Range(0, 1000000)));
            float rnd = (float)random.NextDouble() * totalWeights;

            string chosen = "";

            foreach (KeyValuePair<string, int> item in validWeights)
            {
                rnd -= item.Value;
                if (rnd < 0)
                {
                    chosen = item.Key;
                    break;
                }
            }
            coefficients[coords.x][coords.y][coords.z] = new Coefficient(new string[] { chosen });

        }

        public void Constrain(Vector3Int otherCoords, string otherTile)
        {
            string[] oldKeys = coefficients[otherCoords.x][otherCoords.y][otherCoords.z].Keys;
            List<string> newKeys = new List<string>();

            foreach (string s in oldKeys)
            {
                if (otherTile != s)
                {
                    newKeys.Add(s);
                }


                coefficients[otherCoords.x][otherCoords.y][otherCoords.z] = new Coefficient(newKeys.ToArray());
            }
        }
    }
}
