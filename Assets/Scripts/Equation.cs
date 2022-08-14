using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Create_Shape {
    public class Equation {
        string eqStr;
        private delegate double Operation(double d1, double d2);
//        List<Operation> operationList = new List<Operation>();
        Dictionary<string, Equation> subEquasions = new Dictionary<string, Equation>();
     //   List<double> IndexNums = new List<double>();
        public List<object> splitEquation;
        List<char> variables = new List<char>();

        public Equation(string equation) {
            eqStr = equation;
            ParseInput(equation);
        }

        //need input for variable value
        public double Execute((char, double)[] values) {
            var temp = new List<object>(splitEquation);//prevent side effects
            InsertValues(values);
            //Utility.PrintEnumerable(splitEquation);
         /*   string output = "";
            for(int i = 0; i < splitEquation.Count; i++) { //(var value in splitEquation) {
                output += splitEquation[i].ToString() + ", ";
            }
                Debug.Log(output);*/

            for (int i = 0; i < splitEquation.Count; i++) {//exponent

                if (splitEquation[i].GetType() == typeof(char) && (char)splitEquation[i] == '^') {
                    ExecuteOperation(Exponent, i);
                }
            }

            for (int i = 0; i < splitEquation.Count; i++) {//multiply, divide
                if (splitEquation[i].GetType() != typeof(char)) continue;

                if ((char)splitEquation[i] == '*') {
                    ExecuteOperation(Multiply, i);

                } else if ((char)splitEquation[i] == '/') {
                    ExecuteOperation(Divide, i);

                }
            }

            for (int i = 0; i < splitEquation.Count; i++) {//add, subtract
                if (splitEquation[i].GetType() != typeof(char)) continue;

                if ((char)splitEquation[i] == '-') {
                    ExecuteOperation(Subtract, i);

                } else if ((char)splitEquation[i] == '+') {
                    ExecuteOperation(Add, i);

                }
            }

            string output1 = "";
            if (splitEquation.Count != 1) {
                for (int i = 0; i < splitEquation.Count; i++) {
                    output1 += (splitEquation[i]).GetType() + " " + splitEquation[i].ToString() + ", ";
                }
                Debug.Log(output1);
            }

            Debug.Assert(splitEquation.Count == 1, "equation not fully solved");

            double result = (double) splitEquation[0];
            splitEquation = temp;//prevent side effects
            //replace equations with eq.Execute
            return result;
        }

        //2x+3 = -x-2
        //3x = -5
        //x=-3/5
        public double[] GetIntersections() {
            double[] output = { };
                
            return output;
        }


        private void ExecuteOperation(Operation operation, int index) {
            double d1 = (double)splitEquation[index - 1];
            double d2 = (double)splitEquation[index + 1];
            splitEquation.RemoveRange(index, 2);
            splitEquation[index - 1] = operation(d1, d2);
        }

        private void InsertValues((char, double)[] values) {
            foreach (var item in values) {
                //Debug.Log("insert " + item.Item1 + " = " + item.Item2);
                for (int i = 0; i < splitEquation.Count; i++) {
                    string str = splitEquation[i] as string;
                    if (str != null) {
                        if (str.Length > 2) {//equation
                            splitEquation[i] = subEquasions[str].Execute(values);
                        } else if (str.Contains(item.Item1)) {
                            str = str.Replace(item.Item1.ToString(), item.Item2.ToString());
                            int numNegatives = 0; ;
                                for(int numNegativesIndex = 0; numNegativesIndex < str.Length; numNegativesIndex++) {
                                    if(str[numNegativesIndex] == '-') {
                                        numNegatives++;
                                        if(numNegatives == 2) {
                                        str = str.Substring(2);//remove double negatives cause by a -x where x < 0
                                    }
                                    } else {
                                    numNegatives = 0;
                                }
                                }
                            splitEquation[i] = Double.Parse(str);
                        }
                    }
                }
            }
        }

        private void ParseInput(string input) {
            input = SplitParantheseis(input);
            splitEquation = SplitEquation(input);
            Console.WriteLine(input);
        }

        private string SplitParantheseis(string input) {
            string output = input;
            int openIndex = input.IndexOf("(");
            int loop = 0;

            while (openIndex != -1) {
                int numOpen = 1;
                int numClose = 0;

                int index = openIndex + 1;
                int closeIndex = 0;
                int findParanthesisLoop = 0;
                while (numClose < numOpen) {
                    char letter = output[index];
                    if (letter == '(') {
                        numOpen++;
                    } else if (letter == ')') {
                        numClose++;
                        closeIndex = index;
                    }

                    index++;
                    findParanthesisLoop++;
                    if (input.Length < findParanthesisLoop) {
                        throw new Exception("infinite loop");
                    }

                }

                int startInnerEquation = openIndex + 1;
                int endInnerEquation = closeIndex - openIndex - 1;
                string innerParenthesis = input.Substring(startInnerEquation, endInnerEquation);

                string newEquationName = "eq" + (subEquasions.Count + 1);
                Equation newEquation = new Equation(innerParenthesis);
                output = output.Remove(startInnerEquation - 1, endInnerEquation + 2);//extra 1 to remove paranthesis
                output = output.Insert(openIndex, '#' + newEquationName + '#');//wrap name in #
                subEquasions[newEquationName] = newEquation;

                openIndex = output.IndexOf("(");
                loop++;
                if (loop > 30) {
                    throw new Exception("infinite loop");
                }
            }
            return output;
        }

        private List<object> SplitEquation(string input) {
            List<object> output = new List<object>();
            int i = 0;
            //-5*-x-(25/3.3)^2
            int currentLength = 0;
            if(input[0] == '-') {
                input = input.Remove(0,1);
                input = input.Insert(0, "-1*");
            }
            while (i < input.Length) {//equation
                if (input[i] == '#') {
                    i++;
                    int StartEq = i;

                    while (input[i] != '#') {
                        i++;
                    }
                    string EquationID = input.Substring(StartEq, i - StartEq);
                    output.Add(EquationID);
                }

                bool negNum = (input[i] == '-') && (i == 0 || (IsOperationToken(input[i]) && IsOperationToken(input[i - 1])));//determine if number is neg
                if (negNum) i++;

                if (Char.IsLetter(input[i])) {//variables
                    if (!variables.Contains(input[i])) variables.Add(input[i]);
                    if (negNum) {
                        negNum = false;
                        output.Add("-" + input[i]);
                    } else {
                        output.Add(input[i].ToString());
                    }

                }

                int startDigit = i;
                while (i < input.Length && (Char.IsDigit(input[i]) || input[i] == '.')) {
                    i++;
                }

                if (i != startDigit) {//add digits
                    string num = i == input.Length ? input.Substring(startDigit) : input.Substring(startDigit, i - startDigit);// if i is the length of string need other substring
                    string signedNum = (negNum ? "-" : "") + num;
                    negNum = false;
                    output.Add(Double.Parse(signedNum));
                    i--;
                }

                if (negNum) {//neg not part of digit or letter
                    i--;
                    output.Add(input[i]);
                } else if (IsOperationToken(input[i])) {//operation tokens
                                                        //if (input[i] != '-') {//prevent - being counted if used in neg letter or digit
                    output.Add(input[i]);
                    //}
                }

                if (currentLength == output.Count) {
                    throw new System.Exception("not valid character");
                }

                currentLength = output.Count;
                i++;
            }
            return output;
        }

        private bool IsOperationToken(char input) {
            string tokens = "+-*/^";
            return tokens.IndexOf(input) != -1;
        }

        private static double Add(double d1, double d2) => d1 + d2;
        private static double Subtract(double d1, double d2) => d1 - d2;
        private static double Multiply(double d1, double d2) => d1 * d2;
        private static double Divide(double d1, double d2) => d1 / d2;
        private static double Exponent(double d1, double d2) => Math.Pow(d1, d2);

    }
}
