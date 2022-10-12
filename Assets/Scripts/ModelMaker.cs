using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

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
        [SerializeField] private bool useDebugGizmos = true;

        private delegate List<(Vector3, string)> Create3DPoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain);
        private readonly List<(Action<object[]>, object[])> gizmosDrawList = new List<(Action<object[]>, object[])>();
        private Create3DPoints create3DPoints;
        private Mesh modelMesh = null;


        private void Start() {
            List<Graph.EquationInput> equationInputs = new List<Graph.EquationInput>();
            equationInputs.Add(mainEquation);
            equationInputs.Add(yBound);
            // if (optionalXbound != null) equationInputs.Add(optionalXbound);
            Graph graph = new Graph(equationInputs, graphRange, numSteps);
            var p1 = graph.points[0];
            var p2 = graph.points[1];
            var domain = new Vector2(graph.intersections[0], graph.intersections[1]);
            List<Vector3[]> points = new List<Vector3[]>();

            switch (meshType) {
                case MeshType.square:
                    points = CreateSquarePoints(p1, p2, domain);
                    break;
                case MeshType.hemisphere:
                    points = CreateHemispherePoints(p1, p2, domain, numSteps);
                    break;
                case MeshType.triangle:
                    points = CreateHemispherePoints(p1, p2, domain, 1);
                    break;
            }

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            var mesh = CreateMesh(points);
            meshFilter.mesh = mesh;
            
            if(!useDebugGizmos) return;
            foreach (var pt in mesh.vertices) { 
                gizmosDrawList.Add((DrawPoint, new object[]{pt, Color.blue}));
            }

            for (int i = 0; i < mesh.triangles.Length; i += 3) {
                var num1 = mesh.triangles[i];
                var num2 = mesh.triangles[i + 1];
                var num3 = mesh.triangles[i + 2];

                var point1 = mesh.vertices[num1];
                var point2 = mesh.vertices[num2];
                var point3 = mesh.vertices[num3];
                
                gizmosDrawList.Add((DrawLine, new object[]{point1, point2}));
                gizmosDrawList.Add((DrawLine, new object[]{point2, point3}));
                gizmosDrawList.Add((DrawLine, new object[]{point1, point3}));
            }
            modelMesh = mesh;
        }
        
        //print(Utility.EnumerableElementsToString(mesh.vertices));
        //Utility.EnumerableElementsToString(mesh.vertices);
        /* for (int i = 0; i < mesh.vertices.Length; i++) {
             GizmosDrawList.Add((DrawLabel, new object[] { mesh.vertices[i], i.ToString() }));
         }
         var tris = new List<Tri>();
         for (int i = 0; i < mesh.triangles.Length; i += 3) {
             tris.Add(new Tri(mesh.triangles[i], mesh.triangles[i + 1], mesh.triangles[i + 2]));
             //Debug.Log($"({mesh.triangles[i]}, {mesh.triangles[i+1]}, {mesh.triangles[i+2]}), ");
         }
         */
            /* foreach (var tri in tris) {
                         int isAscending = IsAscending(new[] { tri.x, tri.y, tri.z });
                         Color color;
                         if (isAscending == 1) color = Color.green;
                         else if (isAscending == -1) color = Color.blue;
                         else color = Color.red;

                         var point = GetCenter(new[] { mesh.vertices[tri.x], mesh.vertices[tri.y], mesh.vertices[tri.z] });
                         GizmosDrawList.Add((DrawPoint, new object[] { point, color }));
                     }*/
            /*
                    print(Utility.EnumerableElementsToString(tris));*/

        #region gizmoCallFunctions
        
        private void DrawPoint(object[] parameters) {
            if (parameters.Length > 2) throw new Exception("too many parameters");
            var point = (Vector3)parameters[0];
            var color = (Color)parameters[1];
            Gizmos.color = color;
            Gizmos.DrawSphere(point, 0.1f);
        }

        private void DrawLabel(object[] parameters) {
            if (parameters.Length > 2) throw new Exception("too many parameters");
            var point = (Vector3)parameters[0];
            var text = (string)parameters[1];
            Handles.Label((Vector3)point, (string)text);
        }

        private void DrawLine(object[] parameters) {
            if (parameters.Length > 2) throw new Exception("too many parameters");
            var from = (Vector3)parameters[0];
            var to = (Vector3)parameters[1];
            Gizmos.DrawLine(from, to);
        }
        #endregion

        private void OnDrawGizmos() {
            if (modelMesh == null) return;
            if (!useDebugGizmos) return;
            foreach (var action in gizmosDrawList) {
                action.Item1?.Invoke(action.Item2);
            }
        }

        Vector3 GetCenter(Vector3[] graph) {
            float x = 0, y = 0, z = 0;
            foreach (Vector3 v in graph) {
                x += v.x;
                y += v.y;
                z += v.z;
            }
            return new Vector3(x, y, z) / graph.Length;
        }

        Ray GetTriangleNormal(Vector3 p1, Vector3 p2, Vector3 p3) {
            float dirX = p1.y * p2.z - p1.z * p2.y;
            float dirY = p1.z * p2.x - p1.x * p2.z;
            float dirZ = p1.x * p2.y - p1.y * p2.z;
            Vector3 direction = new Vector3(dirX, dirY, dirZ);

            Vector3 pos = GetCenter(new[] { p1, p2, p3 });
            return new Ray(pos, direction.normalized);
        }

        List<Vector3[]> CreateSquarePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            List<Vector3[]> squarePoints = new List<Vector3[]>();
            int mid = mainGraphPoints.Count / 2;
            Vector3[] endPoints = new Vector3[2];
            Vector3[] botRight = new Vector3[mid];
            Vector3[] topRight = new Vector3[mid];
            Vector3[] topLeft = new Vector3[mid];
            Vector3[] botLeft = new Vector3[mid];
            for (int i = 0; i < mid; i++) {
                Vector2 point1 = mainGraphPoints[i];
                Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];
                float distance = Vector2.Distance(point1, point2);
                
                botRight[i] = new Vector3(point1.x, 0, point1.y);
                topRight[i] = new Vector3(point1.x, distance, point1.y);
                topLeft[i] = new Vector3(point2.x, distance, point2.y);
                botLeft[i] = new Vector3(point2.x, 0, point2.y);
            }
            var midPt = mainGraphPoints[mid];
            //squarePoints.Add(new Vector3(midPt.x, 0, midPt.y));
            squarePoints.Add(endPoints);
            squarePoints.Add(botRight);
            squarePoints.Add(topRight);
            squarePoints.Add(topLeft);
            squarePoints.Add(botLeft);
            return squarePoints;
        }

      //  need to make return Vector3[]
        List<Vector3[]> CreateHemispherePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain, int subDivisions) {//1 is a triangle
            List<Vector3[]> hemispherePoints = new List<Vector3[]>();
            int mid = mainGraphPoints.Count / 2;
            
            var temp = mainGraphPoints[mid];
            var startPoint = new Vector3(temp.x, 0, temp.y);
            var middleEndPointV2 = (mainGraphPoints[0] + mainGraphPoints[mainGraphPoints.Count - 1]) / 2;
            var middleEndPointV3 = new Vector3(middleEndPointV2.x, 0, middleEndPointV2.y);
            hemispherePoints.Add(new[] {middleEndPointV3, startPoint});
            
            hemispherePoints.Add(new Vector3[mid]);//right
            for(int i = 0; i < subDivisions; i++) {
                hemispherePoints.Add(new Vector3[mid]);//subdivisions
            }
            hemispherePoints.Add(new Vector3[mid]);//left
            subDivisions++;//deals with vertex starting at 1
            for (int i = 0; i < mid; i++) {
                Vector2 point1 = mainGraphPoints[i];
                Vector2 point2 = mainGraphPoints[^(i + 1)];
                float radius = Vector2.Distance(point1, point2) / 2;

                hemispherePoints[^1][i] = new Vector3(point1.x, 0, point1.y);
                hemispherePoints[1][i] = new Vector3(point2.x, 0, point2.y);
                for (int vertex = 1; vertex < subDivisions; vertex++) {
                    float angle = ((Mathf.PI) / subDivisions) * vertex;
                    float cartesianX = radius * Mathf.Cos(angle);
                    float cartesianY = radius * Mathf.Sin(angle);
                    var hemiPtVert = hemispherePoints[vertex + 1];
                    hemiPtVert[i] = new Vector3(cartesianX, cartesianY, point1.y);
                }
            }

            return hemispherePoints;
        }


        #region triangle
        Mesh CreateMesh(List<Vector3[]> points) {
            Mesh mesh = new Mesh();
            var vertices2D = points; // CreateTriangleVertices(mainGraphPoints, boundingLinePoints, graphRange);
            mesh.vertices = FlattenVertices(vertices2D);
            var tris = CreateTris(vertices2D);
            mesh.triangles = ConvertTrisToInt(tris);
            // var temp = CreateTriangleNormals(tris, mesh.vertices);

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }



        List<Vector3[]> CreateTriangleVertices(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            List<Vector3[]> trianglePoints = new List<Vector3[]>();
            int mid = mainGraphPoints.Count / 2;
            var temp = mainGraphPoints[mid];
            var startPoint = new Vector3(temp.x, 0, temp.y);
            var middleEndPointV2 = (mainGraphPoints[0] + mainGraphPoints[mainGraphPoints.Count - 1]) / 2;
            var middleEndPointV3 = new Vector3(middleEndPointV2.x, 0, middleEndPointV2.y);
            trianglePoints.Add(new[] {startPoint, middleEndPointV3});

            Vector3[] leftArr = new Vector3[mid];
            Vector3[] rightArr = new Vector3[mid];
            Vector3[] middleArr = new Vector3[mid];
            for (int i = 0; i < mid; i++) {
                Vector2 point1 = mainGraphPoints[i];
                Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];

                float length = Vector2.Distance(point1, point2);
                float x = (point1.x + point2.x) / 2;
                float y = length * Mathf.Sin(1 / 3f * Mathf.PI);
                float z = (point1.y + point2.y) / 2;

                middleArr[mid - 1 - i] = new Vector3(x, y, z);
                leftArr[mid - 1 - i] = new Vector3(point1.x, 0, point1.y);
                rightArr[mid - 1 - i] = new Vector3(point2.x, 0, point2.y);
            }

            trianglePoints.Add(leftArr);
            trianglePoints.Add(middleArr);
            trianglePoints.Add(rightArr);

            return trianglePoints;
        }

        private int WrappingInt(int num, int len) {
            return (num + len) % len;
        }

        //        tris need to be clockwise order
        List<Tri> CreateTris(List<Vector3[]> vertices) {
            List<Tri> triangles = new List<Tri>();
            int[] sideStarts = new int[vertices.Count - 1];
            int origin = 0;
            int backOrigin = 1;
            sideStarts[0] = vertices[0].Length;
            for (int i = 1; i < sideStarts.Length; i++) {
                sideStarts[i] = sideStarts[i - 1] + vertices[i + 1].Length;
            }

            for (int i = 0; i < sideStarts.Length; i++) {
                triangles.Add(new Tri(origin, sideStarts[i], sideStarts[WrappingInt(i + 1, sideStarts.Length)]));
            }

            for (int side = 0; side < sideStarts.Length; side++) {
                int rightStart = sideStarts[WrappingInt(side + 1, sideStarts.Length)];
                int leftStart = sideStarts[side];
                for (int i = 0; i < vertices[1].Length - 1; i++) {
                    var tris = GenerateQuadTris(leftStart, rightStart);
                    triangles.Add(tris.Item1);
                    triangles.Add(tris.Item2);
                    rightStart++;
                    leftStart++;
                }
            }

            for (int i = 0; i < sideStarts.Length - 1; i++) {
                int sides2 = sideStarts[i + 1];
                int verticies2 = vertices[i + 2].Length;
                int right = sides2 + verticies2 - 1;
                int sides1 = sideStarts[i];
                int verticies1 = vertices[i + 1].Length;
                int left = sides1 + verticies1 - 1;
                triangles.Add(new Tri(backOrigin, right, left));
            }

            return triangles;
        }


        //https://answers.unity.com/questions/187637/procedural-meshs-normals-are-reversed.html
        Vector3[] CreateTriangleNormals(List<Tri> tris, Vector3[] verticies) {
            Vector3[] triangles = new Vector3[tris.Count];
            var origin = GetCenter(verticies);
            for(int i = 0; i < tris.Count; i++) {
                Vector3[] faceVerticies = new[] { verticies[tris[i].x], verticies[tris[i].y], verticies[tris[i].z] };
                var normalRay = GetTriangleNormal(faceVerticies[0], faceVerticies[1], faceVerticies[2]);
                var normal = normalRay.direction;
                Vector3 vectorToCenter = origin - normalRay.origin;
                normal = Mathf.Sign(Vector3.Dot(normal, vectorToCenter.normalized)) * -1 * normal;//normal is already normalized
                triangles[i] = normal;
            }
            return triangles;
        }

        //different start indexes for different arrays
        (Tri, Tri) GenerateQuadTris(int firstStart, int secondStart) {
            return (new Tri(secondStart, firstStart, firstStart+1), new Tri(firstStart+1, secondStart+1, secondStart));
        }

        Vector3[] FlattenVertices(List<Vector3[]> vertices2D)
        {
            int len = 0;
            foreach(var arr in vertices2D)
            {
                len += arr.Length;
            }
            Vector3[] vertices = new Vector3[len];
            int index = 0;
            foreach(var arr in vertices2D) {
                foreach(var v3 in arr){
                    vertices[index] = v3;
                    index++;
                }
            }
            return vertices;
        }

        int[] ConvertTrisToInt(List<Tri> tris) {
            int[] output = new int[tris.Count * 3];
            int index = 0;
            foreach (Tri tr in tris) {
                output[index] = tr.x;
                output[index + 1] = tr.y;
                output[index + 2] = tr.z;
                index += 3;
            }
            return output;
        }
        #endregion
        

        private class Tri {
            public int x, y, z;
            public Tri(int x, int y, int z) {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public override string ToString() => $"tri ({x}, {y}, {z})";
        }
    }



    public enum MeshType {
            square,
            hemisphere,
            triangle
        }

    } 