using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Create_Shape {
    public class ModelMaker : MonoBehaviour {

        [SerializeField] Point pointPrefab;
        [SerializeField] Transform modelPointsParent;
        //[SerializeField] List<Graph.EquationInput> equationInputs;
        [SerializeField] Graph.EquationInput mainEquation;
        [SerializeField] Graph.EquationInput yBound;//axis of rotation in rotate meshes
       // [SerializeField] Graph.EquationInput optionalXbound;
        [SerializeField] Vector2 graphRange;
        [SerializeField] MeshType meshType;
        [SerializeField] int numSteps = 5;

        private delegate List<(Vector3, string)> Create3DPoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain);
        private Create3DPoints create3DPoints;

        private void Start() {
            List<Graph.EquationInput> equationInputs = new List<Graph.EquationInput>();
            equationInputs.Add(mainEquation);
            equationInputs.Add(yBound);
            Vector3 vector3 = new Vector2(2, 3);
            // if (optionalXbound != null) equationInputs.Add(optionalXbound);
            print("test " + vector3);
            Graph graph = new Graph(equationInputs, graphRange, numSteps);
            switch (meshType) {
                case MeshType.square:
                    create3DPoints = CreateSquarePoints;
                    break;
                case MeshType.hemisphere:
                    create3DPoints = CreateHemispherePoints;
                    break;
            }
            var p1 = graph.points[0];
            var p2 = graph.points[1];
            var domain = new Vector2(graph.intersections[0], graph.intersections[1]);
            var modelPoints= create3DPoints?.Invoke(p1, p2, domain);
            var parent = Instantiate(modelPointsParent);

            foreach(var point in modelPoints) {//each point
                Point newPoint = Instantiate(pointPrefab, parent);
                newPoint.SetData(point.Item1, point.Item2);
            }

                    
        }

            List<(Vector3,string)> CreateSquarePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
                List<(Vector3,string)> squarePoints = new List<(Vector3, string)>();
                int mid = mainGraphPoints.Count/2 + 1;
                print("main graph count " + mainGraphPoints.Count);
                print("bounding graph count " + boundingLinePoints.Count);
                print("mid point " + mid);
                //int left = 0, right = mainGraphPoints.Count/2+2;
                for (int i = 0; i < mid; i++) {
                    print("i " + i + " main graph " + mainGraphPoints[i]);
                    print("i " + i + " main graph " + mainGraphPoints[mainGraphPoints.Count - i - 1]);
                    Vector2 point1 = mainGraphPoints[i];
                    Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];
                    float distance = Vector2.Distance(point1, point2);
                    squarePoints.Add((new Vector3(point1.x, 0, point1.y),"left bot " + i));
                    squarePoints.Add((new Vector3(point1.x, distance, point1.y),"left top " + i));
                    squarePoints.Add((new Vector3(point2.x, distance, point2.y),"right top " + i));
                    squarePoints.Add((new Vector3(point2.x, 0, point2.y),"right bot " + i));
                }
                return squarePoints;
            }

            List<(Vector3, string)> CreateHemispherePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
                List< (Vector3,string)> hemispherePoints = new List<(Vector3, string)> ();
                int mid = mainGraphPoints.Count / 2 + 1;
                int numVertices = 6;
                for (int i = 0; i < mid; i++) {
                    Vector2 point1 = mainGraphPoints[i];
                    Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];
                    float radius = Vector2.Distance(point1, point2)/2;

                    hemispherePoints.Add((new Vector3(point1.x, 0, point1.y), "left bot " + i));
                    for(int vertex = 1; vertex < numVertices; vertex++) {
                        float angle = (Mathf.PI) / numVertices * vertex;
                        float cartesianX = radius * Mathf.Cos(angle);
                        float cartesianY = radius * Mathf.Sin(angle);
                        hemispherePoints.Add((new Vector3(cartesianX, cartesianY, point1.y), "vertex " + vertex + " layer " + i));
                    }
                    hemispherePoints.Add((new Vector3(point2.x, 0, point2.y), "right bot " + i));
                }

                return hemispherePoints;
            }
        }

        public enum MeshType {
            square,
            hemisphere
        }

    } 

/*float height = 1;
float width = 1;


void Start() {
    Mesh mesh = new Mesh();
    mesh.vertices = GenerateVertices();
    mesh.uv = GenerateUV();
    mesh.normals = GenerateNormals();
    mesh.triangles = GenerateTris();
    GetComponent<MeshFilter>().mesh = mesh;

}

private Vector3[] GenerateVertices() {
    Vector3[] vertices = new Vector3[4] {
            new Vector3(0, 0, 0),
            new Vector3(width, 0, 0),
            new Vector3(0, height, 0),
            new Vector3(width, height, 0)
        };
    return vertices;
}

private int[] GenerateTris() {
    int[] tris = new int[6]
    {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
    };
    return tris;
}

private Vector3[] GenerateNormals() {
    Vector3[] normals = new Vector3[4]
   {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
   };
    return normals;
}

private Vector2[] GenerateUV() {
    Vector2[] uv = new Vector2[4]
    {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
    };
    return uv;
}*/
