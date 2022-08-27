using System;
using System.Threading;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Create_Shape{ 
    public class Graph {
        public List<Vector2>[] points;
        public List<float> intersections;

        //[SerializeField] ;
        private Equation[] equations;
        private UnityEngine.Vector2 graphRange;
        private float graphDistance;//distance between graphRange.y and graphRange.x
        private int numSteps;
        private float stepSize;
        //[SerializeField] Vector2 graphRange;


        public Graph(List<Graph.EquationInput> equationInputs, UnityEngine.Vector2 graphRangeInput, int numSteps = 5) {
            equations = new Equation[equationInputs.Count];
            points = new List<Vector2>[equationInputs.Count];
            graphRange = graphRangeInput;
            graphDistance = graphRange.y - graphRange.x;
            this.numSteps = numSteps;
            stepSize = graphDistance / numSteps;

            //multithread
            for (int i = 0; i < equationInputs.Count; i++) {
                //ThreadPool.QueueUserWorkItem<string>();
                equations[i] = new Equation(equationInputs[i].equation);
            }

            for (int i = 0; i < equations.Length; i++) {
                points[i] = GenerateGraph(equations[i], equationInputs[i].dotColor);
            }

            intersections = GetIntersections(points[0], points[1]);

            Debug.Log(Utility.EnumerableElementsToString(intersections));
        }

        //multithread
        //does not add final element in array, needs one more iterations
        List<Vector2> GenerateGraph(Equation equation, Color color) {
            List<Vector2> graph = new List<Vector2>(numSteps+1);
            var tasks = new Task<Vector2>[numSteps+1];
            for(int i = 0; i <= numSteps; i++) {
               // Debug.Log("created task for " + i);
                int input = i ;
                //Action temp = () => graph[input] = CreatePointValues(input, equation);
                //Debug.Log("input " + input + " i " + i);
                Task<Vector2> task = Task<Vector2>.Factory.StartNew(() => {
                   // Debug.Log("Input " + input + " graph length " + graph.Count);
                    return CreatePointValues(input, equation);
                    });
                tasks[input] = task;
                //tasks[i].Start();
//                tasks[i] = Task.Factory.StartNew(temp);
            }
            Task.WaitAll(tasks);

            for(int i = 0; i < tasks.Length; i++) {
                graph.Add(tasks[i].Result);
            }

            graph.OrderBy(a => a.x);
            if(numSteps % 2 != 0) {//need to insert midpoint b/c number of points is even since points = numsteps - 1
                float x = graphDistance / 2 + graphRange.x;
                float y = (float)equation.Execute(new[] { ('x', (double)x) });
                Vector2 middleVector = new Vector2(x, y);
                graph.Insert(graph.Count/2, middleVector);// +1 to get the middle index
            }
            Debug.Log(Utility.EnumerableElementsToString(graph));

            Debug.Log("graph " + graph.Count);
            Debug.Log(Utility.EnumerableElementsToString(graph));
            return graph;
        }

        Vector2 CreatePointValues(int index, Equation equation) {
            double input = graphRange.x + (index * stepSize);
            //Debug.Log("created point value thread " + index + " with input " + input + " with equation parse " + Utility.EnumerableElementsToString(equation.splitEquation));
            float result = (float)equation.Execute(new[] { ('x', input) });
            //Debug.Log("created point value thread " + index + " with input " + input + " with result " + result + " with equation parse " + //Utility.EnumerableElementsToString(equation.splitEquation));
            return new Vector2((float)input, result);
        }

        //mutithread
        private List<float> GetIntersections(List<UnityEngine.Vector2> equation1, List<Vector2> equation2) {
            Debug.Assert(equation1.Count == equation2.Count, "equations dont match");
            List<float> output = new List<float>();

            for(int i = 1; i < equation1.Count; i++) {
                if (GetOrder(equation1[i-1], equation2[i-1]) != GetOrder(equation1[i], equation2[i])) {
                    float intercept = CalcIntersection(GetLineEquation(equation1[i - 1], equation1[i]), GetLineEquation(equation2[i - 1], equation2[i]), new UnityEngine.Vector2(equation1[i - 1].x, equation1[i].x));
                    if (intercept != float.NaN && !output.Contains(intercept)) {
                        output.Add(intercept);
                    }
                }
            }

            return output;
        }

        //domain is inclusive
        private float CalcIntersection(Vector2 line1, Vector2 line2, UnityEngine.Vector2 domain) {
            float intersection = (float) (((double)line2.y - (double)line1.y)/((double)line1.x - (double)line2.x));
            if(intersection >= domain.x && intersection <= domain.y) return intersection;
            return float.NaN;
        }

        //same index of eqaution 1 & 2
        //gets the order in which the points are stacked to be able to tell if the lines need to cross
        private float GetOrder(Vector2 p1, Vector2 p2) {
            float output = p1.y - p2.y < 0 ? -1 : 1;
            return output;
        }


        //index i & i + 1 of same equation
        //encoded vector.x = slope, vector.y = y intercept
        private Vector2 GetLineEquation(Vector2 p1, Vector2 p2) {
            float m = (p2.y-p1.y)/(p2.x-p1.x);
            float b = p1.y - (p1.x * m);
            return new Vector2(m, b);
        }

        [System.Serializable]
        public class EquationInput {
            public string equation;
            public Color dotColor;

            public EquationInput(string equation, Color dotColor = default) {
                this.equation = equation;
                this.dotColor = dotColor;
            }
           
        }
    }
}
