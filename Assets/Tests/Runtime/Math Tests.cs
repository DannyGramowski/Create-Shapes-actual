using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Create_Shape;


public class MathTests {
    private Mesh cube;
    private const int CubeWidth = 2;
    
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
    }
    
    [Test]
    public void TestIntegral() {
        var num = MathUtil.Integrate(0, 1, new Equation("x"));
        Assert.AreEqual(0.5, num, 0.01f);
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
    
}
