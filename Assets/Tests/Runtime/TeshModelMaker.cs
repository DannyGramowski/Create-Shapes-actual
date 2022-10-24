using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Create_Shape;
using System.Reflection;

public class TeshModelMaker {
   private ModelMaker modelMaker;
   private ModelMaker.TestModelMaker testMaker;
   private List<Vector2> mainGraphPts;
   private List<Vector2> boundPts;
   private Graph graph;
   private Vector2 domain;
   [SetUp]
   public void SetUp() {
      var go = new GameObject("mm");
      go.AddComponent<ModelMaker>();
      modelMaker = go.GetComponent<ModelMaker>();
      testMaker = new ModelMaker.TestModelMaker(modelMaker);
      graph = new Graph("x^2-4", new UnityEngine.Vector2(-5, 5));
      mainGraphPts = graph.points[0];
      boundPts = graph.points[1];
      domain = new Vector2(graph.intersections[0], graph.intersections[1]);
   }
   
   [Test]
   public void TestCreateSquarePoints() {
      var pts = testMaker.TestCreateSquarePoints(mainGraphPts, boundPts, domain);
      var flattened = testMaker.TestFlattenVertices(pts);

      double distance = 0;
      foreach (var p in flattened) {
         distance += p.magnitude;
      }
      Assert.AreEqual(158.0/flattened.Length, distance/flattened.Length, 1);
      //dividened by flattened.length to make the assertion itteration independant
   }
   
   [Test]
   public void TestCreateHemispherePoints() {
      var pts = testMaker.TestCreateHemispherePoints(mainGraphPts, boundPts, domain);
      var flattened = testMaker.TestFlattenVertices(pts);

      double distance = 0;
      foreach (var p in flattened) {
         distance += Math.Abs(p.magnitude);
      }
      Assert.AreEqual(177.8/flattened.Length, distance/flattened.Length, 0.5);   
   }
   
   [Test]
   public void TestCreateTrianglePoints() {
      var pts = testMaker.TestCreateTrianglePoints(mainGraphPts, boundPts, domain);
      var flattened = testMaker.TestFlattenVertices(pts);

      double distance = 0;
      foreach (var p in flattened) {
         distance += Math.Abs(p.magnitude);
      }
      Assert.AreEqual(116.7/flattened.Length, distance/flattened.Length, 0.5);      }

   [Test]
   public void TestCreateTris() {
      var pts = testMaker.TestCreateTrianglePoints(mainGraphPts, boundPts, domain);
      var tris = testMaker.TestCreateTris(pts);
      double hash = 0;
      foreach (var tri in tris) {
         hash += tri.GetHashCode();
      }
      
      Assert.AreEqual( -1325163, hash);
   }

   [Test]
   public void TestTriHashcode() {
      var tri = new ModelMaker.Tri(1, 1, 1);
      Assert.AreEqual(252,tri.GetHashCode());
   }
   
   [Test]
   public void TestGenerateQuadTris() {
      var result = testMaker.TestGenerateQuadTris(1, 2);
      var num = result.Item1.GetHashCode() + result.Item2.GetHashCode();
      Assert.AreEqual(-280, num);
   }
   
   [Test]
   public void TestFlattenVertices() {
      var list = new List<int[]> {
         Enumerable.Range(1, 10).ToArray(),
         Enumerable.Range(100, 10).ToArray(),
         Enumerable.Range(50,10).ToArray()
      };
      
      var result = testMaker.TestFlattenVertices(list);

      var expected = Enumerable.Range(1, 10).Union(Enumerable.Range(100, 10)).Union(Enumerable.Range(50, 10)).ToArray();
      Assert.AreEqual( result, expected);
   }
}
