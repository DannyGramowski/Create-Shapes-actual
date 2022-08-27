using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Create_Shape {
    [SelectionBase]
    public class Point : MonoBehaviour{
        public float x, y, z;
        public string equationName;
        public void SetData(Vector3 pointData, string equation) {
            x = pointData.x;
            y = pointData.y;
            z = pointData.z;
            transform.position = new Vector3(x, y, z);
            equationName = equation;
            transform.name = equation;
        }
    }
}
