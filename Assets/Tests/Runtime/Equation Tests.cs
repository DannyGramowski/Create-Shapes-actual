using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Create_Shape;

public class EquationTests {

        private (char, double)[] args = { ('x', 5) };

        [Test]
        public void TestAdd() {
            string str = "1.5+3";
            Equation eq = new Equation(str);
            Assert.AreEqual(4.5, eq.Execute(args));
        }

        [Test]
        public void TestSubtract() {
            string str = "1-3";
            Equation eq = new Equation(str);
            Assert.AreEqual(-2, eq.Execute(args));
        }

        [Test]
        public void TestDoubleNeg() {
            string str = "1--3";
            Equation eq = new Equation(str);
            Assert.AreEqual(4, eq.Execute(args));
        }

        [Test]
        public void TestMultiply() {
            string str = "2*3";
            Equation eq = new Equation(str);
            Assert.AreEqual(6, eq.Execute(args));
        }

        [Test]
        public void TestDivide() {
            string str = "4/2";
            Equation eq = new Equation(str);
            Assert.AreEqual(2, eq.Execute(args));
        }

        [Test]
        public void TestExponent() {
            string str = "3^2";
            Equation eq = new Equation(str);
            Assert.AreEqual(9, eq.Execute(args));
        }

        [Test]
        public void TestVariable() {

            string str = "3*x";//x = 5
            Equation eq = new Equation(str);
            Assert.AreEqual(5, args[0].Item2);
            Assert.AreEqual(15, eq.Execute(args));
        }

        [Test]
        public void TestNegVariable() {
            var negArgs = new (char, double)[] { ('x', -2) };
            string str = "-x+2";

            Equation eq = new Equation(str);
            Assert.AreEqual(4, eq.Execute(negArgs));
        }

    [Test]
        public void TestParenthesis() {
            string str = "1+(3*4)";
            Equation eq = new Equation(str);
            Assert.AreEqual(13, eq.Execute(args));
        }

        [Test]
        public void CompleteTest() {
            string str = "-5*-x-(25/3.3)^2";//x = 5
            Equation eq = new Equation(str);
            Assert.AreEqual(-32.39, eq.Execute(args), 0.01);
        }
    }

