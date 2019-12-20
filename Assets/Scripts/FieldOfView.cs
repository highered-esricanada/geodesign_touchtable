using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class FieldOfView : MonoBehaviour
{

    private Slider angle;
    private Slider radius;
    private Slider height;
    private Slider rotate;

    private float viewHeight;
    private float viewRotation;


    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public MeshFilter noViewMeshFilter;
    public MeshFilter viewMeshFilter;
    //public GameObject ViewMeshObject;

    public Material POVGood;
    public Material POVBad;

    Mesh viewMesh;
    Mesh noViewMesh;
    MeshRenderer viewMeshRenderer;

    void Start()
    {
        viewHeight = this.transform.position.y;
        viewRotation = this.transform.rotation.y;

        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;

        //viewMeshRenderer = ViewMeshObject.GetComponent<MeshRenderer>();

        noViewMesh = new Mesh();
        noViewMesh.name = "No View Mesh";
        noViewMeshFilter.mesh = noViewMesh;



        //StartCoroutine("FindTargetsWithDelay", .2f);


    }

    private void Update()
    {
        if (GameObject.Find("Page 2 - FOV") != null)
        {
            angle = GameObject.Find("View Angle S").GetComponent<Slider>();
            radius = GameObject.Find("View Radius S").GetComponent<Slider>();
            height = GameObject.Find("Height S").GetComponent<Slider>();
            rotate = GameObject.Find("Rotation S").GetComponent<Slider>();

            viewAngle = angle.value;
            viewRadius = radius.value;
        }

        DrawFieldOfView(true);
        viewMeshRenderer.material = POVBad;

    }


    void LateUpdate()
    {
        DrawFieldOfView(false);
        viewMeshRenderer.material = POVGood;
    }



    void DrawFieldOfView(bool firstTimeDraw)
    {
        //step count is number of rays
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            //current angle, incrementing stepangles for each multiple of i
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle, firstTimeDraw);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast, firstTimeDraw);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }

            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        if (firstTimeDraw)
        {
            noViewMesh.Clear();
            noViewMesh.vertices = vertices;
            noViewMesh.triangles = triangles;
            noViewMesh.RecalculateNormals();
        }
        else
        {
            viewMesh.Clear();
            viewMesh.vertices = vertices;
            viewMesh.triangles = triangles;
            viewMesh.RecalculateNormals();
        }
    }


    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast, bool firstTimeDraw)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle, firstTimeDraw);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }


    ViewCastInfo ViewCast(float globalAngle, bool firstTimeDraw)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius) && !firstTimeDraw)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);

        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}







