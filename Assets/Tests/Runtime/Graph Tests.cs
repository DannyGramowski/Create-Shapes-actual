using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Create_Shape;

public class GraphTests {
    private UnityEngine.Vector2 range = new UnityEngine.Vector2(-5, 5);

    [Test]
    public void TestGraphPloting() {
        List<Graph.EquationInput> input = new List<Graph.EquationInput>();
        input.Add(new Graph.EquationInput("-x-1"));//-5//wrong, absolute value?, parabola
        input.Add(new Graph.EquationInput("x+2"));//10
        UnityEngine.Vector2 graphRange = new UnityEngine.Vector2(-2, 2);
        Graph graph = new Graph(input, graphRange, 4);
        float ySum = 0;
        foreach(var points in graph.points) {
            foreach(var point in points) {
                ySum += point.y;
            }
        }
        Assert.AreEqual(5, ySum);
    }

    [Test]
    public void TestStraightLine() {
        List<Graph.EquationInput> input = new List<Graph.EquationInput>();
        input.Add(new Graph.EquationInput("x+1"));    
        input.Add(new Graph.EquationInput("2*x-1"));
        Vector2 newRange = new Vector2(0, 4);
        Graph graph = new Graph(input, newRange, 1);
        float intersectionSum = 0;
        foreach (var intersection in graph.intersections) intersectionSum += intersection;
        Assert.AreEqual(2, intersectionSum);
    }


    [Test]
    public void TestParabolaLine() {
        List<Graph.EquationInput> input = new List<Graph.EquationInput>();
        input.Add(new Graph.EquationInput("x+2"));
        input.Add(new Graph.EquationInput("x^2"));
        Graph graph = new Graph(input, range);
        float intersectionSum = 0;
        foreach (var intersection in graph.intersections) intersectionSum += intersection;
        Assert.AreEqual(0.6666f, intersectionSum,.001);
    }

    [Test]
    public void TestIntersections() {
        Graph graph = new Graph("x^3-x", new Vector2(-2, 2), 10);

        double distance = 0;
        for(int i = 0; i < graph.intersections.Count; i++) {
            distance += graph.intersections[i];
        }
        
        Assert.AreEqual(0, distance, 0.1);
    }

}
