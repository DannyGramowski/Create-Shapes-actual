using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using Create_Shape;


public class MathTests {
    [Test]
    public void TestIntegral() {
        var num = MathUtil.Integrate(0, 1, new Equation("x"));
        Assert.AreEqual(0.5, num, 0.01f);
    }
    
    
}
