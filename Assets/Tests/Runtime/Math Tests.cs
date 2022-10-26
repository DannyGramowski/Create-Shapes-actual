using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Create_Shape;


public class MathTests {
    private Mesh cube;
    private const int CubeWidth = 2;
    private Equation eq;
    
    [SetUp]
    public void SetUp() {
        #region createCube

        Vector3[] vertices = {
            new Vector3 (0, 0, 0),
            new Vector3 (CubeWidth, 0, 0),
            new Vector3 (CubeWidth, CubeWidth, 0),
            new Vector3 (0, CubeWidth, 0),
            new Vector3 (0, CubeWidth, CubeWidth),
            new Vector3 (CubeWidth, CubeWidth, CubeWidth),
            new Vector3 (CubeWidth, 0, CubeWidth),
            new Vector3 (0, 0, CubeWidth),
        };
        
        int[] triangles = {
            0, 2, 1, //face front
            0, 3, 2,
            2, 3, 4, //face top
            2, 4, 5,
            1, 2, 5, //face right
            1, 5, 6,
            0, 7, 4, //face left
            0, 4, 3,
            5, 4, 7, //face back
            5, 7, 6,
            0, 6, 7, //face bottom
            0, 1, 6
        };

        cube = new Mesh();
        cube.vertices = vertices;
        cube.triangles = triangles;
        #endregion

        eq = new Equation("x^2-4");
    }
    
    [Test]
    public void TestIntegral1() {
        var num = MathUtil.Integrate(0, 1, new Equation("x"));
        Assert.AreEqual(0.5, num, 0.01f);
    }
    
    [Test]
    public void TestIntegral2() {
        var num = MathUtil.Integrate(-3, 2, new Equation("x^2-4"));
        Assert.AreEqual(-8.333, num, 0.01f);
    }
    
    [Test]
    public void TestIntegral3() {
        var num = MathUtil.Integrate(0, 5, new Equation("(x-3)^2"));
        Assert.AreEqual(11.666, num, 0.01f);
    }
    
    [Test]
    public void TestIntegral4() {
        var num = MathUtil.Integrate(-2, 5, new Equation("x^3+2*x^2+1"));
        Assert.AreEqual(247.916, num, 0.1f);
    }

    [Test]
    public void TestIntegralInvertedBounds() {
        var num = MathUtil.Integrate(0, -2, new Equation("-x"));
        Assert.AreEqual(-2, num, 0.01f);
    }

    [Test]
    public void TestTriangleArea() {
        var a = new Vector3(0, 0, 0);
        var b = new Vector3(2, 0, 0);
        var c = new Vector3(0, 1, 0);
        var area = MathUtil.TriangleArea(a,b,c);
        Assert.AreEqual(1, area);
    }

    [Test]
    public void TeshMeshVolume() {
        var volume = MathUtil.MeshVolume(cube);
        Assert.AreEqual(8, volume, 0.01);
    }

    [Test]
    public void TestMeshSurfaceArea() {
        var sa = MathUtil.MeshSurfaceArea(cube);
        Assert.AreEqual(24, sa, 0.01);
    }

    /*[Test]
    public void TestActualAreaHemisphere() {
        var vol = MathUtil.Volume(eq, new Vector2(-2, 0), MeshType.Hemisphere);
        Assert.AreEqual(Math.PI*8/3d, vol);
    }


    [Test]
    public void TestActualAreaTriangle() {
        var vol = MathUtil.Volume(eq, new Vector2(-2, 0), MeshType.Triangle);
        Assert.AreEqual(16/3d, vol);
    }


    [Test]
    public void TestActualAreaSquare() {
        var vol = MathUtil.Volume(eq, new Vector2(-2, 0), MeshType.Square);
        Assert.AreEqual(8/3d, vol);
    }*/
    

}
