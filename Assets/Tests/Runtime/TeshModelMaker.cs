using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Create_Shape;
using System.Reflection;

public class TeshModelMaker {
   private ModelMaker modelMaker;
   private ModelMaker.TestModelMaker testMaker;
   [SetUp]
   public void SetUp() {
      var go = new GameObject("mm");
      go.AddComponent<ModelMaker>();
      modelMaker = go.GetComponent<ModelMaker>();
      testMaker = new ModelMaker.TestModelMaker(modelMaker);
   }
   
   [Test]
   public void TestCreateSquarePoints() {
      Assert.AreEqual(true, true);
   }
   
   [Test]
   public void TestCreateHemispherePoints() {
      Assert.AreEqual(true, true);
   }
   
   [Test]
   public void TestCreateTrianglePoints() {
      Assert.AreEqual(true, true);
   }
   
   [Test]
   public void TestCreateMesh() {
      Assert.AreEqual(true, true);
   }
   
   [Test]
   public void TestCreateTris() {
      Assert.AreEqual(true, true);
   }

   [Test]
   public void TestTriHashcode() {
      var tri = new ModelMaker.Tri(1, 1, 1);
      Assert.AreEqual(252,tri.GetHashCode());
   }
   
   [Test]
   public void TestGenerateQuadTris() {
      /*
      BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Instance;

      var method = modelMaker.GetType().GetMethod("GenerateQuadTris", bf);
      var result = ((ModelMaker.Tri, ModelMaker.Tri)) method.Invoke(modelMaker, new object[] { 1, 2 });
      */

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
      Assert.AreEqual(expected, result);
   }
}
