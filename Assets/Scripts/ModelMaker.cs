using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

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

            switch (meshType) {
                case MeshType.square:
                    create3DPoints = CreateSquarePoints;
                    break;
                case MeshType.hemisphere:
                    create3DPoints = CreateHemispherePoints;
                    break;
                case MeshType.triangle:
                    MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

                    MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
                    var mesh = CreateTriangleMesh(p1, p2, domain);
                    meshFilter.mesh = mesh;
                    modelMesh = mesh;
                    print(Utility.EnumerableElementsToString(mesh.vertices));
                    //Utility.EnumerableElementsToString(mesh.vertices);

                    var tris = new List<Tri>();
                    for(int i = 0; i < mesh.triangles.Length; i += 3) {
                        tris.Add(new Tri( mesh.triangles[i] ,  mesh.triangles[i + 1],  mesh.triangles[i + 2]));
                        //Debug.Log($"({mesh.triangles[i]}, {mesh.triangles[i+1]}, {mesh.triangles[i+2]}), ");
                    }
                    print(Utility.EnumerableElementsToString(tris));
                    return;
            }

            var modelPoints= create3DPoints?.Invoke(p1, p2, domain);
            var parent = Instantiate(modelPointsParent);

            foreach(var point in modelPoints) {//each point
                Point newPoint = Instantiate(pointPrefab, parent);
                newPoint.SetData(point.Item1, point.Item2);
            }

            
        }

        bool gizmoDrawn = false;
        private void OnDrawGizmos() {
            if (modelMesh == null) return;
            
           // if (gizmoDrawn) return;
            int index = 0;
            foreach(var pt in modelMesh.vertices) {
                //Gizmos.color = Color.red;
                //Gizmos.DrawSphere(pt, 0.1f);
                Handles.Label(pt , index.ToString());
                index++;
            }

            print($"normals length {modelMesh.normals.Length}");
            for (int i = 0; i < modelMesh.triangles.Length; i += 3) {
                Vector3[] g = { modelMesh.vertices[modelMesh.triangles[i]], modelMesh.vertices[modelMesh.triangles[i + 1]], modelMesh.vertices[modelMesh.triangles[i + 2]] };

                Gizmos.color = Color.cyan;
                int normalIndex = i / 3;
                print($"normal ind {normalIndex}");
                Gizmos.DrawRay(GetCenter(g), modelMesh.normals[i/3]);
                
            }

            Handles.Label(GetCenter(modelMesh.vertices), "center");
            gizmoDrawn = true;

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


        Vector3 RandomOffset() {
            float x = Random.value * 0.05f;
            float y = Random.value * 0.05f;
            float z = Random.value * 0.05f;
            return new Vector3(x, y, z);
        }

        Ray GetTriangleNormal(Vector3 p1, Vector3 p2, Vector3 p3) {
            float dirX = p1.y * p2.z - p1.z * p2.y;
            float dirY = p1.z * p2.x - p1.x * p2.z;
            float dirZ = p1.x * p2.y - p1.y * p2.z;
            Vector3 direction = new Vector3(dirX, dirY, dirZ);

            Vector3 pos = GetCenter(new[] { p1, p2, p3 });
            return new Ray(pos, direction.normalized);
        }

       // Ray GetTriangleNormal(Tri tri) => GetTriangleNormal(tri.)
       /* Nx = Ay* Bz - Az* By
        Ny = Az* Bx - Ax* Bz
        Nz = Ax* By - Ay* Bx*/

        List<(Vector3,string)> CreateSquarePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            List<(Vector3,string)> squarePoints = new List<(Vector3, string)>();
            int mid = mainGraphPoints.Count/2;

            for (int i = 0; i < mid; i++) {
                Vector2 point1 = mainGraphPoints[i];
                Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];
                float distance = Vector2.Distance(point1, point2);
                squarePoints.Add((new Vector3(point1.x, 0, point1.y),"left bot " + i));
                squarePoints.Add((new Vector3(point1.x, distance, point1.y),"left top " + i));
                squarePoints.Add((new Vector3(point2.x, distance, point2.y),"right top " + i));
                squarePoints.Add((new Vector3(point2.x, 0, point2.y),"right bot " + i));
            }
            var midPt = mainGraphPoints[mid];
            squarePoints.Add((new Vector3(midPt.x, 0, midPt.y), "mid point"));
            return squarePoints;
        }

        List<(Vector3, string)> CreateHemispherePoints(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            List< (Vector3,string)> hemispherePoints = new List<(Vector3, string)> ();
            int mid = mainGraphPoints.Count / 2 + 1;
            int numVertices = 2;
            for (int i = 0; i < mid; i++) {
                Vector2 point1 = mainGraphPoints[i];
                Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];
                float radius = Vector2.Distance(point1, point2)/2;

                hemispherePoints.Add((new Vector3(point1.x, 0, point1.y), "left bot " + i));
                for(int vertex = 1; vertex < numVertices; vertex++) {
                    float angle = ((Mathf.PI) / numVertices )* vertex;
                    float cartesianX = radius * Mathf.Cos(angle);
                    float cartesianY = radius * Mathf.Sin(angle);
                    hemispherePoints.Add((new Vector3(cartesianX, cartesianY, point1.y), "vertex " + vertex + " layer " + i));
                }
                hemispherePoints.Add((new Vector3(point2.x, 0, point2.y), "right bot " + i));
            }

            return hemispherePoints;
        }


        #region triangle
        Mesh CreateTriangleMesh(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            Mesh mesh = new Mesh();
            var vertices2D = CreateTriangleVertices(mainGraphPoints, boundingLinePoints, graphRange);
            mesh.vertices = FlattenVertices(vertices2D);
            var tris = CreateTriangleTris(vertices2D);
            mesh.triangles = ConvertTrisToInt(tris);
            var temp = CreateTriangleNormals(tris, mesh.vertices);

           // mesh.RecalculateNormals();
           // mesh.RecalculateTangents();
            return mesh;
        }



        List<Vector3[]> CreateTriangleVertices(List<Vector2> mainGraphPoints, List<Vector2> boundingLinePoints, Vector2 domain) {
            List<Vector3[]> trianglePoints = new List<Vector3[]> ();
            int mid = mainGraphPoints.Count / 2;
            var temp = mainGraphPoints[mid];
            trianglePoints.Add(new[] { new Vector3(temp.x, 0, temp.y) });

            Vector3[] leftArr = new Vector3[mid];
            Vector3[] rightArr = new Vector3[mid];
            Vector3[] middleArr = new Vector3[mid];
            for (int i = 0; i < mid; i++) {
                Vector2 point1 = mainGraphPoints[i];
                Vector2 point2 = mainGraphPoints[mainGraphPoints.Count - i - 1];

                //trianglePoints.Add((new Vector3(point1.x, 0, point1.y), "left bot " + i));
                //trianglePoints.Add((new Vector3(point2.x, 0, point2.y), "right bot " + i));

                float length = Vector2.Distance(point1, point2);
                float x = (point1.x+point2.x)/2;
                float y = length*Mathf.Sin(1/3f*Mathf.PI);
                float z = (point1.y + point2.y) / 2;
                //Debug.Log("p1 " + point1 + " p2 " + point2);
                // Debug.Log("triangle coords " + x + " " + y + " " + z);
                // trianglePoints.Add((new Vector3(x,y,z), "top " + i));
                middleArr[mid-1-i] = new Vector3(x, y, z);
                leftArr[mid - 1 - i] = new Vector3(point1.x, 0, point1.y);
                rightArr[mid - 1 - i] = new Vector3(point2.x, 0, point2.y);
            }
            //add normals, use dot product to tell which direction
            trianglePoints.Add(middleArr);
            trianglePoints.Add(leftArr);
            trianglePoints.Add(rightArr);

            return trianglePoints;
        }

        List<Tri> CreateTriangleTris(List<Vector3[]> vertices) {
            List<Tri> triangles = new List<Tri>();
            int origin = 0;
            int topStart = 1;
            int leftStart = topStart + vertices[1].Length;
            int rightStart = vertices[2].Length + leftStart;
            //top and right

            triangles.Add(new Tri(origin, leftStart, rightStart));
            triangles.Add(new Tri(origin, leftStart, topStart));
            triangles.Add(new Tri(origin, topStart, rightStart));

            for (int i = 0; i < vertices[1].Length - 1; i++) {
                var tris = GenerateQuadTris(topStart, rightStart);
                triangles.Add(tris.Item1);
                triangles.Add(tris.Item2);
            }

            //top and left
            for (int i = 0; i < vertices[1].Length - 1; i++) {
                var tris = GenerateQuadTris(topStart, leftStart);
                triangles.Add(tris.Item1);
                triangles.Add(tris.Item2);
            }

            //left and right
            for (int i = 0; i < vertices[1].Length - 1; i++) {
                var tris = GenerateQuadTris(leftStart, rightStart);
                triangles.Add(tris.Item1);
                triangles.Add(tris.Item2);
            }


            return triangles;
        }

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
            return (new Tri(firstStart,secondStart,firstStart+1), new Tri(secondStart, firstStart+1, secondStart+1));
        }

        Vector3[] FlattenVertices(List<Vector3[]> vertices2D) {
            Vector3[] vertices = new Vector3[vertices2D[0].Length + vertices2D[1].Length * 3];
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