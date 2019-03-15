using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFC_Cs {
    public struct Vector2 {
        public Vector2(int _x, int _y) {
            X = _x;
            Y = _y;
        }

        public override string ToString() {
            return String.Format("({0}-{1})", X, Y);
        }


        public int X { get; set; }
        public int Y { get; set; }
    }


    public struct CompatibilityData {
        public List<CompatibilityRule> Compatibilities { get; set; }
        public Dictionary<string, int> Weights { get; set; }

        public CompatibilityData(Dictionary<string, int> _weights, List<CompatibilityRule> _compatibilities) {
            Weights = _weights;
            Compatibilities = _compatibilities;
        }
    }

    public struct CompatibilityRule {
        public string Current { get; set; }
        public string NextInDirection { get; set; }
        public Vector2 Direction { get; set; }

        public CompatibilityRule(string _current, string _nextInDirection, Vector2 _direction) {
            Current = _current;
            NextInDirection = _nextInDirection;
            Direction = _direction;
        }
    }

    public struct Coefficient {
        public Coefficient(string[] _keys) {
            Keys = _keys;
        }


        public override string ToString() {
            return string.Join(",", Keys);
        }

        public string[] Keys { get; set; }
    }

    public static class Directions {
        public static readonly Vector2 Up = new Vector2(0, 1);
        public static readonly Vector2 Left = new Vector2(-1, 0);
        public static readonly Vector2 Down = new Vector2(0, -1);
        public static readonly Vector2 Right = new Vector2(1, 0);
        public static readonly List<Vector2> Dirs = new List<Vector2>() { Up, Left, Down, Right };
    }

    class CompatibilityOracle {
        public List<CompatibilityRule> Data { get; set; }

        public CompatibilityOracle(List<CompatibilityRule> _data) {
            Data = _data;
        }

        public bool Check(CompatibilityRule ruleToCheck) {
            return Data.Contains(ruleToCheck);
        }
    }

    class Model {
        private Vector2 outputSize;
        private Dictionary<string, int> weights;
        private CompatibilityOracle compatibilityOracle;
        private Wavefunction wavefunction;

        public Model(Vector2 _outputSize, Dictionary<string, int> _weights, CompatibilityOracle _compatibilityOracle) {
            outputSize = _outputSize;
            weights = _weights;
            compatibilityOracle = _compatibilityOracle;

            wavefunction = Wavefunction.Mk(outputSize, weights);
        }

        public List<List<string>> Run() {


            while (!wavefunction.IsFullyCollapsed()) {
                Iterate();
            }
            return wavefunction.GetAllCollapsed();
        }

        private void Iterate() {
            Vector2 coords = MinEntropyCoords();

            wavefunction.Collapse(coords);
            Propagate(coords);

        }

        private Vector2 MinEntropyCoords() {
            float minEntropy = 0;
            Vector2 minEntropyCoords = new Vector2();

            Random random = new Random();

            for (int x = 0; x < outputSize.X; x++) {
                for (int y = 0; y < outputSize.Y; y++) {

                    Vector2 currentCoordinates = new Vector2(x, y);
                    if (wavefunction.Get(currentCoordinates).Keys.Length == 1) {
                        continue;
                    }

                    float entropy = wavefunction.ShannonEntropy(currentCoordinates);
                    float entropyPlusNoise = entropy - (float)random.NextDouble() / 1000;

                    // (float)random.NextDouble() / 1000;

                    if (minEntropy == 0 || entropyPlusNoise < minEntropy) {
                        minEntropy = entropyPlusNoise;
                        minEntropyCoords = new Vector2(x, y);
                    }
                }
            }
            return minEntropyCoords;
        }

        private void Propagate(Vector2 coords) {
            Stack<Vector2> stack = new Stack<Vector2>(new Vector2[] { coords });

            while (stack.Count > 0) {
                Vector2 currentCoords = stack.Pop();
                Coefficient currentPossibleTiles = wavefunction.Get(currentCoords);


                foreach (Vector2 dir in Program.ValidDirs(currentCoords, outputSize)) {
                    Vector2 otherCoords = new Vector2(currentCoords.X + dir.X, currentCoords.Y + dir.Y);


                    foreach (string other in wavefunction.Get(otherCoords).Keys) {
                        string otherTile = "";
                        bool isAnyPossible = false;
                        otherTile = other; // NOT SURE IF THIS IS CORRECT.

                        foreach (string current in currentPossibleTiles.Keys) {
                            if (compatibilityOracle.Check(new CompatibilityRule(current, other, dir))) {
                                isAnyPossible = true;
                            }
                        }

                        if (!isAnyPossible) {
                            wavefunction.Constrain(otherCoords, otherTile);
                            stack.Push(otherCoords);
                        }
                    }
                }
            }
        }
    }

    class Wavefunction {
        private List<List<Coefficient>> coefficients;
        private Dictionary<string, int> weights;

        public Wavefunction(List<List<Coefficient>> _coefficients, Dictionary<string, int> _weights) {
            coefficients = _coefficients;
            weights = _weights;
        }

        static public Wavefunction Mk(Vector2 size, Dictionary<string, int> weights) {
            List<List<Coefficient>> Coefficients = InitCoefficients(size, weights.Keys);

            return new Wavefunction(Coefficients, weights);

        }


        private static List<List<Coefficient>> InitCoefficients(Vector2 size, Dictionary<string, int>.KeyCollection keys) {
            List<List<Coefficient>> Coefficients = new List<List<Coefficient>>();
            Coefficient coefficient = new Coefficient(keys.ToArray());


            for (int x = 0; x < size.X; x++) {
                Coefficients.Add(new List<Coefficient>());

                for (int y = 0; y < size.Y; y++) {
                    Coefficients[x].Add(new Coefficient());
                    Coefficients[x][y] = coefficient;

                }
            }
            return Coefficients;
        }

        public bool IsFullyCollapsed() {
            for (int x = 0; x < coefficients.Count; x++) {
                for (int y = 0; y < coefficients[x].Count; y++) {
                    if (coefficients[x][y].Keys.Length > 1) {
                        return false;
                    }
                }
            }
            return true;
        }

        public Coefficient Get(Vector2 coords) {
            return coefficients[coords.X][coords.Y];
        }

        public string GetCollapsed(Vector2 coords) {
            Coefficient opts = Get(coords);

            if (opts.Keys.Length == 1) {
                return opts.Keys[0];
            }

            throw new Exception();
        }

        public List<List<string>> GetAllCollapsed() {
            int width = coefficients.Count;
            int height = coefficients[0].Count;

            List<List<string>> collapsed = new List<List<string>>();

            for (int x = 0; x < width; x++) {
                collapsed.Add(new List<string>());
                for (int y = 0; y < height; y++) {
                    collapsed[x].Add(GetCollapsed(new Vector2(x, y)));
                }
            }

            return collapsed;
        }

        public float ShannonEntropy(Vector2 currentCoordinates) {
            int sumOfWeights = 0;
            float sumOfWeightsLogWeights = 0;

            foreach (string coefficient in coefficients[currentCoordinates.X][currentCoordinates.Y].Keys) {
                int weight = weights[coefficient];
                sumOfWeights += weight;
                sumOfWeightsLogWeights += weight * (float)Math.Log(weight);
            }
            return (float)Math.Log(sumOfWeights) - (sumOfWeightsLogWeights / sumOfWeights);
        }

        public void Collapse(Vector2 coords) {
            Coefficient opts = coefficients[coords.X][coords.Y];

            Dictionary<string, int> validWeights = new Dictionary<string, int>();

            foreach (KeyValuePair<string, int> item in weights) {
                if (opts.Keys.Contains(item.Key)) {
                    validWeights.Add(item.Key, item.Value);
                }
            }

            int totalWeights = validWeights.Sum(x => x.Value);

            Random random = new Random();
            float rnd = (float)random.NextDouble() * totalWeights;

            string chosen = "";

            foreach (KeyValuePair<string, int> item in validWeights) {
                rnd -= item.Value;
                if (rnd < 0) {
                    chosen = item.Key;
                    break;
                }
            }
            coefficients[coords.X][coords.Y] = new Coefficient(new string[] { chosen });

        }

        public void Constrain(Vector2 otherCoords, string otherTile) {
            string[] oldKeys = coefficients[otherCoords.X][otherCoords.Y].Keys;

            List<string> newKeys = new List<string>();



            foreach (string s in oldKeys) {
                if (otherTile != s) {

                    newKeys.Add(s);
                }
             }


            coefficients[otherCoords.X][otherCoords.Y] = new Coefficient(newKeys.ToArray());


            
        }
    }

    public class Program {
        static void Main(string[] args) {
            string[,] inputMatrix = new string[,] {

                {"L","L","L","L"},
                {"L","L","L","L"},
                {"L","L","L","L"},
                {"L","C","C","L"},
                {"C","S","S","C"},
                {"S","S","S","S"},
                {"S","S","S","S"},

            };

            string[,] inputMatrix2 = new string[,] {

                {"A","A","A","A"},
                {"A","C","C","A"},
                {"C","B","B","C"},
                {"C","B","B","C"},
                {"C","B","B","C"},
                {"C","B","B","C"},
                {"A","C","C","A"},

            };

            string[,] inputMatrix3 = new string[,] {

                {"A","A","A","A","A"},
                {"A","A","B","A","B"},
                {"A","B","C","B","C"},
                {"A","A","B","A","B"},
                {"A","A","A","A","A"},

            };

            string[,] inputMatrix4 = new string[,] {

                {"A","A","C","B"},
                {"A","A","C","B"},
                {"A","C","C","B"},
                {"A","A","B","B"},

            };

            CompatibilityData data = ParseExampleMatrix(inputMatrix4);
            CompatibilityOracle compatibilityOracle = new CompatibilityOracle(data.Compatibilities);

            Model model = new Model(new Vector2(25, 75), data.Weights, compatibilityOracle);
            List<List<string>> output = model.Run();

            RenderOutput(output);

            var name = Console.ReadLine();
        }

        static void RenderOutput(List<List<string>> output) {
            for (int x = 0; x < output.Count; x++) {
                Console.WriteLine("");
                for (int y = 0; y < output[x].Count; y++) {
                    Console.BackgroundColor = ConsoleColor.Black;

                    if (output[x][y] == "L" || output[x][y] == "A") {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }

                    if (output[x][y] == "S" || output[x][y] == "B") {
                        Console.BackgroundColor = ConsoleColor.Blue;
                    }

                    if (output[x][y] == "C") {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                    }

                    Console.Write(" ");



                }
            }

            Console.ResetColor();
        }



        static CompatibilityData ParseExampleMatrix(string[,] matrix) {

            Vector2 matrixSize = new Vector2(matrix.GetLength(0), matrix.GetLength(1));

            Dictionary<string, int> weights = new Dictionary<string, int>();
            List<CompatibilityRule> compatibilities = new List<CompatibilityRule>();

            int count = 0;

            for (int x = 0; x < matrixSize.X; x++) {
                for (int y = 0; y < matrixSize.Y; y++) {

                    string currentTile = matrix[x, y];

                    Vector2 matrixCoordinate = new Vector2(x, y);

                    if (!weights.ContainsKey(currentTile)) {
                        weights.Add(currentTile, 0);
                    }

                    weights[currentTile] += 1;

                    foreach (Vector2 dir in ValidDirs(matrixCoordinate, matrixSize)) {
                        string otherTile = matrix[x + dir.X, y + dir.Y];

                        CompatibilityRule newRule = new CompatibilityRule(currentTile, otherTile, dir);

                        if (!compatibilities.Contains(newRule)) {
                            compatibilities.Add(newRule);
                        }
                    }
                }
            }


            return new CompatibilityData(weights, compatibilities);
        }

        public static List<Vector2> ValidDirs(Vector2 matrixCoordinate, Vector2 matrixSize) {

            List<Vector2> directions = new List<Vector2>();

            if (matrixCoordinate.X > 0) {
                directions.Add(Directions.Left);
            }

            if (matrixCoordinate.X < matrixSize.X - 1) {
                directions.Add(Directions.Right);
            }

            if (matrixCoordinate.Y > 0) {
                directions.Add(Directions.Down);
            }

            if (matrixCoordinate.Y < matrixSize.Y - 1) {
                directions.Add(Directions.Up);
            }
            return directions;
        }
    }
}
