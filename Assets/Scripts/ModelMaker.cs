using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;
using System.Runtime.CompilerServices;

namespace Create_Shape {
    public class ModelMaker : MonoBehaviour {
        //[SerializeField] List<Graph.EquationInput> equationInputs;
        [SerializeField] Graph.EquationInput mainEquation;
        [SerializeField] Graph.EquationInput yBound;//axis of rotation in rotate meshes
        // [SerializeField] Graph.EquationInput optionalXbound;
        [SerializeField] Vector2 graphRange;
        [SerializeField] MeshType meshType;
        [SerializeField] int numSteps = 5;
        [SerializeField] private bool useDebugGizmos = true;
        [SerializeField] private bool drawGrid = true;

        private delegate List<(Vector3, string)> Create3DPoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain);
        private readonly List<(Action<object[]>, object[])> gizmosDrawList = new List<(Action<object[]>, object[])>();
        private Create3DPoints create3DPoints;
        private int gridSize = 50;

        public void Generate(Graph.EquationInput mainEQ, Graph.EquationInput newYBound, Vector2 newDomain, MeshType type) {
            mainEquation = mainEQ;
            yBound = newYBound;
            graphRange = newDomain;
            meshType = type;
            
            List<Graph.EquationInput> equationInputs = new List<Graph.EquationInput>(){mainEquation, yBound};
            // if (optionalXbound != null) equationInputs.Add(optionalXbound);
            Graph graph = new Graph(equationInputs, graphRange, numSteps);
            var p1 = graph.points[0];
            var p2 = graph.points[1];
            var domain = new Vector2(graph.intersections[0], graph.intersections[1]);
            List<Vector3[]> points = new List<Vector3[]>();

            switch (meshType) {
                case MeshType.Square:
                    points = CreateSquarePoints(p1, p2, domain);
                    break;
                case MeshType.Hemisphere:
                    points = CreateHemispherePoints(p1, p2, domain, numSteps);
                    break;
                case MeshType.Triangle:
                    points = CreateHemispherePoints(p1, p2, domain, 1);
                    break;
            }

            MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
            var mesh = CreateMesh(points);
            meshFilter.mesh = mesh;
            
            #if UNITY_EDITOR
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
                
                gizmosDrawList.Add((DrawLine, new object[]{point1, point2, Color.blue}));
                gizmosDrawList.Add((DrawLine, new object[]{point2, point3, Color.blue}));
                gizmosDrawList.Add((DrawLine, new object[]{point1, point3, Color.blue}));
            }

            var offset = gridSize / 2;
            for (int row = -offset; row < offset; row++) {
                var point1 = new Vector3(-offset, 0, row);
                var point2 = new Vector3(offset, 0, row);
                gizmosDrawList.Add((DrawLine, new object[]{point1, point2}));
                gizmosDrawList.Add((DrawLabel, new object[]{new Vector3(0,0,row), row.ToString()}));
            }
            
            for (int column = -offset; column < offset; column++) {
                var point1 = new Vector3(column, 0, -offset);
                var point2 = new Vector3(column, 0, offset);
                gizmosDrawList.Add((DrawLine, new object[] {point1, point2}));
                gizmosDrawList.Add((DrawLabel, new object[] {new Vector3(column, 0, 0), column.ToString()}));
            }
            #endif
        }
        
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
            if (parameters.Length > 3) throw new Exception("too many parameters");
            var from = (Vector3)parameters[0];
            var to = (Vector3)parameters[1];
            Color color = Color.black;
            if(parameters.Length > 2)  color = (Color)parameters[2];
            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
        }
        #endregion

        private void OnDrawGizmos() {
            if (!useDebugGizmos) return;
            foreach (var action in gizmosDrawList) {
                action.Item1?.Invoke(action.Item2);
            }
        }

        internal Vector3 GetCenter(Vector3[] graph) {
            float x = 0, y = 0, z = 0;
            foreach (Vector3 v in graph) {
                x += v.x;
                y += v.y;
                z += v.z;
            }
            return new Vector3(x, y, z) / graph.Length;
        }
        
        internal List<Vector3[]> CreateSquarePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            List<Vector3[]> squarePoints = new List<Vector3[]>();
            int mid = mainGraphPoints.Count / 2;
            
            var temp = mainGraphPoints[mid];
            var startPoint = new Vector3(temp.x, 0, temp.y);
            var middleEndPointV2 = (mainGraphPoints[0] + mainGraphPoints[mainGraphPoints.Count - 1]) / 2;
            var middleEndPointV3 = new Vector3(middleEndPointV2.x, 0, middleEndPointV2.y);
            
            Vector3[] endPoints = {middleEndPointV3, startPoint};
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
            squarePoints.Add(endPoints);
            squarePoints.Add(botLeft);
            squarePoints.Add(topLeft);
            squarePoints.Add(topRight);
            squarePoints.Add(botRight);
            return squarePoints;
        }

        internal List<Vector3[]> CreateHemispherePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain, int subDivisions) {//1 is a triangle
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


        internal Mesh CreateMesh(List<Vector3[]> points) {
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
        
        private int WrappingInt(int num, int len) {
            return (num + len) % len;
        }

        internal List<Tri> CreateTris(List<Vector3[]> vertices) {
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

        internal (Tri, Tri) GenerateQuadTris(int firstStart, int secondStart) {
            return (new Tri(secondStart, firstStart, firstStart+1), new Tri(firstStart+1, secondStart+1, secondStart));
        }

        internal T[] FlattenVertices<T>(List<T[]> vertices2D) {
            int len = 0;
            foreach(var arr in vertices2D)
            {
                len += arr.Length;
            }
            T[] vertices = new T[len];
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
                output[index] = tr.X;
                output[index + 1] = tr.Y;
                output[index + 2] = tr.Z;
                index += 3;
            }
            return output;
        }

        public class Tri {
            public readonly int X;
            public readonly int Y;
            public readonly int Z;

            public Tri(int x, int y, int z) {
                this.X = x;
                this.Y = y;
                this.Z = z;
            }

            public override int GetHashCode() {
                int output = (91 * X * X - 28 * Y * Y * Y) * (3+Z);
                return output;
            }

            public override string ToString() => $"tri ({X}, {Y}, {Z})";
        }
        
        //this class should only be used in testing
        public class TestModelMaker {
            private readonly ModelMaker maker;
            public TestModelMaker(ModelMaker mm) {
                maker = mm;
            }

            public List<Vector3[]> TestCreateSquarePoints(List<Vector2> mainGraph, List<Vector2> bounding, Vector2 domain) {
                return maker.CreateSquarePoints(mainGraph, bounding, domain);
            }

            public List<Vector3[]> TestCreateHemispherePoints(List<Vector2> mainGraph, List<Vector2> bound, Vector2 domain) {
                return maker.CreateHemispherePoints(mainGraph, bound, domain, 3);
            }
            
            public List<Vector3[]> TestCreateTrianglePoints(List<Vector2> mainGraph, List<Vector2> bound, Vector2 domain) {
                return maker.CreateHemispherePoints(mainGraph, bound, domain, 1);
            }

            public List<Tri> TestCreateTris(List<Vector3[]> pts) {
                return maker.CreateTris(pts);
            }

            public T[] TestFlattenVertices<T>(List<T[]> nums) {
                return maker.FlattenVertices(nums);
            }
            public (Tri, Tri) TestGenerateQuadTris(int num1, int num2) {
                return maker.GenerateQuadTris(num1, num2);
            }
        }
    }



    public enum MeshType {
            Square,
            Hemisphere,
            Triangle
        }

    } 
    
    