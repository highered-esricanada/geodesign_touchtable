using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySlippyMap.Markers;
using UnitySlippyMap.Map;
using System;
using UnityEngine.UI;

namespace Esri
{
    [RequireComponent(typeof(MeshFilter))]
    public class DrawLine : MonoBehaviour
    {
        public GameObject linePrefab;
        public GameObject currLine;
        public LineRenderer lineRenderer;
        public List<Vector3> fingerPos;

        public int triangleCounter = 1;
        public GameObject triangleObj;
        public MapBehaviour map;



        private const int EarthRadius = 6378137; //no seams with globe example
        private const double OriginShift = 2 * Math.PI * EarthRadius / 2;



        // Start is called before the first frame update
        void Start()
        {
           // numOfShapes = 1;


        }

        /*
        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {

                CreateLine();
            }

            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float planeHeight = 0.01f;
                float distToDrawPlane = (planeHeight - ray.origin.y) / ray.direction.y;
                //gets the current finger position after moving finger while mouse button is still down
                Vector3 tempFingerPos = ray.GetPoint(distToDrawPlane);

                if (Vector3.Distance(tempFingerPos, fingerPos[fingerPos.Count - 1]) > 0.01f)
                {
                    UpdateLine(tempFingerPos);
                }


            }

            if (Input.GetMouseButtonUp(0))
            {
                CloseLine();
                //int triCount = 1;
                TriangulatePoints(fingerPos);

                int triCount = 1;
                foreach (Triangle t in TriangulatePoints(fingerPos))
                {


                    //Debug.Log("Triangle " + triCount +": vertex 1 = " + t.v1.position.ToString() + "\n"
                    // + "vertex 2 = " + t.v2.position.ToString() + "\n"
                    // + "vertex 3 = " + t.v3.position.ToString() + "\n");

                    triCount++;
                    GameObject newTriangle = Object.Instantiate(Resources.Load<GameObject>("TrianglePrefab"));
                    newTriangle.name = "Triangle #: " + triangleCounter;
                    triangleCounter++;



                    //Add everything to a mesh
                    Mesh newMesh = new Mesh();
                    newTriangle.GetComponent<MeshFilter>().mesh = newMesh;

                    newTriangle.GetComponent<MeshRenderer>().material = Resources.Load<Material>("HighlightArea");


                    Vector3[] vertices = new Vector3[3];
                    int[] triangles = new int[] { 0, 1, 2 };

                    vertices[0] = t.v1.position;
                    vertices[1] = t.v2.position;
                    vertices[2] = t.v3.position;

                    newMesh.vertices = vertices;
                    newMesh.triangles = triangles;


                    
                }



            }


        }

    */

        public void FetchMap()
        {
            map = FindObjectOfType(typeof(MapBehaviour)) as MapBehaviour;

        }


        public void CreateLine()
        {
            FetchMap();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float planeHeight = 0.0f;
            float distToDrawPlane = (planeHeight - ray.origin.y) / ray.direction.y;
            Vector3 mousePosition = ray.GetPoint(distToDrawPlane);

            currLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            //currLine.name = "Shape # " + numOfShapes;

            


            //numOfShapes++;
            lineRenderer = currLine.GetComponent<LineRenderer>();
            fingerPos.Clear();
            fingerPos.Add(mousePosition);
            fingerPos.Add(mousePosition);

            var vertWorldPt = ShapeVertexToWorldPoint(mousePosition);

            map.CreateMarker<MarkerBehaviour>(currLine.name, vertWorldPt, currLine);

            lineRenderer.SetPosition(0, fingerPos[0]);
            lineRenderer.SetPosition(1, fingerPos[1]);

        }

        public void UpdateLine(Vector3 newFingerPos)
        {
            fingerPos.Add(newFingerPos);
            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, newFingerPos);
        }

        public void CloseLine()
        {

            currLine.GetComponent<LineRenderer>().enabled = false;
            lineRenderer.SetPosition(0, fingerPos[fingerPos.Count - 1]);
            lineRenderer.SetPosition(1, fingerPos[0]);

            /*
            var meshFilter = currLine.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            lineRenderer.BakeMesh(mesh);
            meshFilter.sharedMesh = mesh;

            var meshRenderer = currLine.AddComponent<MeshRenderer>();
            //newTriangle.GetComponent<MeshRenderer>().material = Resources.Load<Material>("HighlightArea");
            meshRenderer.sharedMaterial = Resources.Load<Material>("LineMaterial 1");
            GameObject.Destroy(lineRenderer);
            */

        }

        public double[] ShapeVertexToWorldPoint(Vector3 v) 
        {

            double[] centerMeters = new double[2] 
            {
                    map.CenterEPSG900913 [0],
                    map.CenterEPSG900913 [1]
            };

            double[] displacementMeters = new double[2] {
                    v.x / map.RoundedScaleMultiplier,
                    v.z / map.RoundedScaleMultiplier
            };

            double[] vertexPosition = new double[2] {
                    centerMeters[0] += displacementMeters[0],
                    centerMeters[1] += displacementMeters[1]
            };

            var worldPos = new Vector2((float)vertexPosition[0], (float)vertexPosition[1]);

            worldPos = ConvertPositionToLatLon(worldPos);

            double[] vertexWorldPos = new double[2]
            {
                    (double)worldPos.x,
                    (double)worldPos.y
            };

            Debug.Log("vertex wp: lat = " + vertexWorldPos[0] + ", lon: " + vertexWorldPos[1]);
            return vertexWorldPos;
        }

        private Vector2 ConvertPositionToLatLon(Vector2 m)
        {
            var vx = (float)(m.x / OriginShift) * 180;
            var vy = (float)(m.y / OriginShift) * 180;
            vy = (float)(180 / Math.PI * (2 * Math.Atan(Math.Exp(vy * Math.PI / 180)) - Math.PI / 2));

            //Debug.Log("Position: " + vx + ", " + vy);
            return new Vector2(vx, vy);
        }



        //This assumes that we have a polygon and now we want to triangulate it
        //The points on the polygon should be ordered counter-clockwise
        //This alorithm is called ear clipping and it's O(n*n) Another common algorithm is dividing it into trapezoids and it's O(n log n)
        //One can maybe do it in O(n) time but no such version is known
        //Assumes we have at least 3 points
        public  List<Triangle> TriangulatePoints(List<Vector3> points)
        {
            //The list with triangles the method returns
            List<Triangle> triangles = new List<Triangle>();

            //If we just have three points, then we dont have to do all calculations
            if (points.Count == 3)
            {

                //no it doesn't
                //Debug.Log("does this if statement get called? ");
                triangles.Add(new Triangle(points[0], points[1], points[2]));

                return triangles;
            }

            //Step 1. Store the vertices in a list and we also need to know the next and prev vertex
            List<Vertex> vertices = new List<Vertex>();

            for (int i = 0; i < points.Count; i++)
            {
                vertices.Add(new Vertex(points[i]));
                //vertices successfully added to vertices List<>
                //Debug.Log(vertices[i].position);
            }

            //Find the next and previous vertex
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextPos = ClampListIndex(i + 1, vertices.Count);

                int prevPos = ClampListIndex(i - 1, vertices.Count);

                vertices[i].prevVertex = vertices[prevPos];

                vertices[i].nextVertex = vertices[nextPos];
            }

            //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                CheckIfReflexOrConvex(vertices[i]);
            }


            //Have to find the ears after we have found if the vertex is reflex or convex
            List<Vertex> earVertices = new List<Vertex>();

            for (int i = 0; i < vertices.Count; i++)
            {
                IsVertexEar(vertices[i], vertices, earVertices);

            }

            //Step 3. Triangulate!
            while (true)
            {
                //This means we have just one triangle left
                if (vertices.Count == 3)
                {
                    //The final triangle
                    triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

                    break;
                }

                //Make a triangle of the first ear
                //Debug.Log("Ear vertices.Count: " +earVertices.Count);
                Vertex earVertex = earVertices[0];

                Vertex earVertexPrev = earVertex.prevVertex;
                Vertex earVertexNext = earVertex.nextVertex;

                Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);
                //Debug.Log("newTriangle: curr, prev, next vertex " + earVertex.position +
                //earVertexPrev.position + earVertexNext.position);


                triangles.Add(newTriangle);

                //Remove the vertex from the lists
                earVertices.Remove(earVertex);

                //Debug.Log("vertices.Count: " + vertices.Count);
                vertices.Remove(earVertex);


                //Update the previous vertex and next vertex
                earVertexPrev.nextVertex = earVertexNext;
                earVertexNext.prevVertex = earVertexPrev;

                //...see if we have found a new ear by investigating the two vertices that was part of the ear
                CheckIfReflexOrConvex(earVertexPrev);
                CheckIfReflexOrConvex(earVertexNext);

                earVertices.Remove(earVertexPrev);
                earVertices.Remove(earVertexNext);

                IsVertexEar(earVertexPrev, vertices, earVertices);
                IsVertexEar(earVertexNext, vertices, earVertices);
            }

            //Debug.Log(triangles.Count);

            return triangles;

        }

        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }


        //Check if a vertex if reflex or convex, and add to appropriate list
        private static void CheckIfReflexOrConvex(Vertex v)
        {
            v.isReflex = false;
            v.isConvex = false;

            //This is a reflex vertex if its triangle is oriented clockwise
            Vector2 a = v.prevVertex.GetPos2D_XZ();
            Vector2 b = v.GetPos2D_XZ();
            Vector2 c = v.nextVertex.GetPos2D_XZ();

            if (IsTriangleOrientedClockwise(a, b, c))
            {
                v.isReflex = true;
            }
            else
            {
                v.isConvex = true;
            }
        }

        //Is a triangle in 2d space oriented clockwise or counter-clockwise
        public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        //Check if a vertex is an ear
        private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
        {
            //A reflex vertex cant be an ear!
            if (v.isReflex)
            {
                return;
            }

            //This triangle to check point in triangle
            Vector2 a = v.prevVertex.GetPos2D_XZ();
            //Debug.Log("Previous vertex: " + a.ToString());
            Vector2 b = v.GetPos2D_XZ();
            //Debug.Log("curr vertex: " + b.ToString());

            Vector2 c = v.nextVertex.GetPos2D_XZ();
            //Debug.Log("next vertex: " + c.ToString());


            bool hasPointInside = false;

            for (int i = 0; i < vertices.Count; i++)
            {
                //We only need to check if a reflex vertex is inside of the triangle
                if (vertices[i].isReflex)
                {
                    Vector2 p = vertices[i].GetPos2D_XZ();

                    //This means inside and not on the hull
                    if (IsPointInTriangle(a, b, c, p))
                    {
                        hasPointInside = true;

                        break;
                    }
                }
            }

            if (!hasPointInside)
            {
                earVertices.Add(v);
            }
        }


        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        //p is the testpoint, and the other points are corners in the triangle
        public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

            float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
            float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
            //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
            //{
            //    isWithinTriangle = true;
            //}

            //The point is within the triangle
            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }

    }

}
