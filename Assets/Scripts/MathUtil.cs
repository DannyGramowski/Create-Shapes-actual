using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Create_Shape {
    public static class MathUtil {

        //I also trust this https://stackoverflow.com/questions/1406029/how-to-calculate-the-volume-of-a-3d-mesh-object-the-surface-of-which-is-made-up
        public static float MeshVolume(Mesh mesh) {
            float output = 0;
            for (int i = 0; i < mesh.triangles.Length; i += 3) {
                Vector3 a = mesh.vertices[mesh.triangles[i]];
                Vector3 b = mesh.vertices[mesh.triangles[i+1]];
                Vector3 c = mesh.vertices[mesh.triangles[i+2]];
                output += SignedVolumeOfTriangle(a, b, c);
            }

            return Mathf.Abs(output);        }
        
        private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            var v321 = p3.x*p2.y*p1.z;
            var v231 = p2.x*p3.y*p1.z;
            var v312 = p3.x*p1.y*p2.z;
            var v132 = p1.x*p3.y*p2.z;
            var v213 = p2.x*p1.y*p3.z;
            var v123 = p1.x*p2.y*p3.z;
            return (1.0f/6.0f)*(-v321 + v231 + v312 - v132 - v213 + v123);
        }

        public static float MeshSurfaceArea(Mesh mesh) {
            float output = 0;
            for (int i = 0; i < mesh.triangles.Length; i += 3) {
                Vector3 a = mesh.vertices[mesh.triangles[i]];
                Vector3 b = mesh.vertices[mesh.triangles[i+1]];
                Vector3 c = mesh.vertices[mesh.triangles[i+2]];
                output += TriangleArea(a, b, c);
            }

            return output;
        }

        //I trust this https://math.stackexchange.com/questions/128991/how-to-calculate-the-area-of-a-3d-triangle
        public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c) {
            return Vector3.Cross(b-a, c-a).magnitude / 2f;
        }

        public static double Integrate(float lower, float upper, Equation eq) {
            int subDivisions = 100;
            double output = 0;
            double previous = eq.Execute(lower);
            for (int i = 1; i <= subDivisions; i++) {
                var value = (upper - lower) /subDivisions * i + lower;
                var newy = eq.Execute(value) / subDivisions; 
                output += (previous + newy)/2;
                previous = newy;
            }
            return output;
        }
    }
}