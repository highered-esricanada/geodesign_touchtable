// 
//  TestMap.cs
//  
//  Author:
//       Jonathan Derrough <jonathan.derrough@gmail.com>
//  
//  Copyright (c) 2012 Jonathan Derrough
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

// #define DEBUG_LOG

using UnityEngine;

using TouchScript.Gestures.TransformGestures;

using System;

using UnitySlippyMap.Map;
using UnitySlippyMap.Markers;
using UnitySlippyMap.Layers;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using TouchScript;
using TouchScript.Gestures;
using TouchScript.Layers;

using TouchScript.Gestures.TransformGestures.Base;






using UnityEngine.UI;
using TouchScript.Pointers;
using static TouchScript.Gestures.Gesture;

namespace Esri
{
    public class TestMap : MonoBehaviour
    {

        public DrawLine drawLine;
        public bool isDrawing = false;
        public int numOfShapes;

        public MapBehaviour map;


        public Texture LocationTexture;
        public Texture MarkerTexture;


        public float CameraZoom;
        private Transform camPivot;

        private bool isPerspectiveView = false;
        private float perspectiveAngle = 30.0f;
        private float destinationAngle = 0.0f;
        private float currentAngle = 0.0f;
        private float animationDuration = 0.5f;
        private float animationModelDuration = 1.5f;
        private float animationStartTime = 0.0f;
        private float modelSpeed = 2.0f;

        //touchscript variables
        public ScreenTransformGesture TwoFingerMoveGesture;
        public ScreenTransformGesture ManipulationGesture;
        public float PanSpeed = 1f;
        public float RotationSpeed = 200f;
        public float ZoomSpeed = .25f;

        //panning variable(s)
        public bool drag = false;
        public bool panningStop = false;
        public Vector3 screenPosition = Vector3.zero;
        private Plane plane;
        private ProjectionParams projection;
        private Vector3 startPosition;
        private Pointer primaryPointer;
        private static Vector3 lastHitPosition = Vector3.zero;
        private Vector3 newPosition;

        private Transform cam;
        private Transform esriMap;

        private Vector3 maxscale = new Vector3(0.0005f, 0.0005f, 0.0005f);
        private Vector3 minscale = new Vector3(0.0005f, 0.00001f, 0.0005f);


        private List<LayerBehaviour> layers;
        private int currentLayerIndex = 0;

        public StandardLayer standardLayer;

        public Button button;
        bool buttonToggle = false;

        private void Awake()
        {
            cam = GameObject.Find("Camera").transform;

            //camPivot = transform.Find("Camera");

        }

        private void OnEnable()
        {
            TwoFingerMoveGesture.Transformed += twoFingerTransformHandler;
            ManipulationGesture.Transformed += manipulationTransformedHandler;

            if (TouchManager.Instance != null)
            {
                //update handler
                TouchManager.Instance.PointersUpdated += pointersUpdateHandler;
                //release handler
                TouchManager.Instance.PointersReleased += pointersReleaseHandler;
                //press handler
                TouchManager.Instance.PointersPressed += pointersPressedHandler;

            }
        }

        private void ManageLayers()
        {

        }

        private void OnDisable()
        {
            if (TouchManager.Instance != null)
            {
                //update handler
                TouchManager.Instance.PointersUpdated -= pointersUpdateHandler;
                //release handler
                TouchManager.Instance.PointersReleased -= pointersReleaseHandler;
                //press handler
                TouchManager.Instance.PointersPressed -= pointersPressedHandler;
            }
            TwoFingerMoveGesture.Transformed -= twoFingerTransformHandler;
            ManipulationGesture.Transformed -= manipulationTransformedHandler;
        }



        private void pointersUpdateHandler(object sender, PointerEventArgs e)
        {
            if (isDrawing)
            {
                drag = false;
                Debug.Log("isDrawing update is working.");

                Ray drawRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float planeHeight = 0.01f;
                float distToDrawPlane = (planeHeight - drawRay.origin.y) / drawRay.direction.y;
                //gets the current finger position after moving finger while mouse button is still down
                Vector3 tempFingerPos = drawRay.GetPoint(distToDrawPlane);

                if (Vector3.Distance(tempFingerPos, drawLine.fingerPos[drawLine.fingerPos.Count - 1]) > 0.001f)
                {
                    drawLine.UpdateLine(tempFingerPos);
                }
            }
            
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log(hit.collider.name);
                var selection = hit.transform;


                if (selection.CompareTag("Selectable"))
                {
                    var standLayer = GameObject.Find("Camera").GetComponent<StandardLayer>().enabled = true;

                }


                else
                {
                    var standLayer = GameObject.Find("Camera").GetComponent<StandardLayer>().enabled = false;

                }


            }

            

        }

        //pressing handler
        private void pointersPressedHandler(object sender, PointerEventArgs e)
        {
            
            
            
            GameObject tabArea = GameObject.Find("Tab Area");
            TabGroup tabGroup = tabArea.GetComponent<TabGroup>();



            Vector2 pos = new Vector2();
            foreach (var pointer in e.Pointers)
            {
                pos += pointer.Position;
            }
            newPosition = pos / e.Pointers.Count;


            lastHitPosition = Vector3.zero;
            var activePointers = TwoFingerMoveGesture.ActivePointers;
            var activeCount = activePointers.Count;
            if (activeCount == 0)
            {
                if (isDrawing)
                {
                    drag = false;
                    Debug.Log("isdrawing pointer pressed");
                    drawLine.CreateLine();
                }

                else if (tabGroup.index == 0 && !isDrawing)
                {
                    RectTransform invPanel = GameObject.Find("Menu_Case").GetComponent<RectTransform>();

                    if (!RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition))
                    {
                        drag = true;
                        Debug.Log("Begin Drag");
                    }

                }

                else if (tabGroup.index == 1 && !isDrawing)
                {
                    RectTransform fovPanel = GameObject.Find("Menu_FOV").GetComponent<RectTransform>();

                    if (!RectTransformUtility.RectangleContainsScreenPoint(fovPanel, Input.mousePosition))
                    {
                        drag = true;
                        Debug.Log("Begin Drag");
                    }

                }

                else
                {
                    drag = true;

                }
            }
        }

        private void pointersReleaseHandler(object sender, PointerEventArgs e)
        {
            
            
            if (isDrawing)
            {
                StartCoroutine(CloseShape());
            }
            drag = false;

            //Debug.Log("Not panning");

            // reset the last hit position
            lastHitPosition = Vector3.zero;

            // trigger a tile update
            map.IsDirty = true;

        }

        void Update()
        {
            if (destinationAngle != 0.0f)
            {
                Vector3 cameraLeft = Quaternion.AngleAxis(-90.0f, Camera.main.transform.up) * Camera.main.transform.forward;
                if ((Time.time - animationStartTime) < animationDuration)
                {
                    float angle = Mathf.LerpAngle(0.0f, destinationAngle, (Time.time - animationStartTime) / animationDuration);
                    Camera.main.transform.RotateAround(Vector3.zero, cameraLeft, angle - currentAngle);
                    currentAngle = angle;

                    map.Zoom(ZoomSpeed);
                }
                else
                {
                    if ((Camera.main.transform.position.y) > CameraZoom)
                    {
                        map.Zoom(-ZoomSpeed);
                    }

                    Camera.main.transform.RotateAround(Vector3.zero, cameraLeft, destinationAngle - currentAngle);
                    destinationAngle = 0.0f;
                    currentAngle = 0.0f;
                    map.IsDirty = true;

                    


                }

                map.HasMoved = true;
            }

            if (GameObject.Find("ScrollInfoControl") != null)
            {
                RectTransform casePanel = GameObject.Find("Content_Cases").GetComponent<RectTransform>();
                if (!RectTransformUtility.RectangleContainsScreenPoint(casePanel, Input.mousePosition))
                {
                    var standLayer = GameObject.Find("Camera").GetComponent<StandardLayer>().enabled = true;
                }


            }


        }

        public IEnumerator CloseShape()
        {
            Debug.Log("drawing pointer released.");
            drawLine.CloseLine();

            GameObject shapeHolder = Instantiate(Resources.Load<GameObject>("ShapeHolder"));
            shapeHolder.name = "Shape #" + numOfShapes;

            //get current finger positionto put into vertex form, to be translated into lat/long
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float planeHeight = 0.0f;
            float distToDrawPlane = (planeHeight - ray.origin.y) / ray.direction.y;
            Vector3 mousePosition = ray.GetPoint(distToDrawPlane);

            var vertWorldPt = drawLine.ShapeVertexToWorldPoint(mousePosition);


            map.CreateMarker<MarkerBehaviour>(shapeHolder.name, vertWorldPt, shapeHolder);



            drawLine.TriangulatePoints(drawLine.fingerPos);

            int triCount = 1;

            foreach (Triangle t in drawLine.TriangulatePoints(drawLine.fingerPos))
            {


                triCount++;
                GameObject newTriangle = Instantiate(Resources.Load<GameObject>("TrianglePrefab"));
                newTriangle.name = "Triangle #: " + drawLine.triangleCounter;
                newTriangle.transform.parent = shapeHolder.transform;
                drawLine.triangleCounter++;



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
            numOfShapes++;

            yield return null;
                    
        }

        private void updateCamera()
        {

            screenPosition = new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 0);
            //if (drag)
            if (drag)
            {

                map.UpdatesCenterWithLocation = false;

                //  Debug.Log("Panning");

                // apply the movements
                Ray ray = map.CurrentCamera.ScreenPointToRay(screenPosition);
                //Debug.Log("1. INPUT - screenposition: " + screenPosition);
                //Debug.Log("2. INPUT - Touchpostion: " + new Vector3(Input.touches[0].position.x, Input.touches[0].position.y));
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    Vector3 displacement = Vector3.zero;
                    //Debug.Log("3. INPUT - Displacement instantiate: " + displacement);
                    if (lastHitPosition != Vector3.zero)
                    {
                        displacement = hitInfo.point - lastHitPosition;
                        //Debug.Log("4. TEST: displacement = hitInfo.point - lastHitPosition: " + displacement);
                    }
                    lastHitPosition = new Vector3(hitInfo.point.x, hitInfo.point.y, hitInfo.point.z);
                    //Debug.Log("5. INPUT - lasthitpos: " + lastHitPosition);
                    if (displacement != Vector3.zero)
                    {
                        // update the centerWGS84 property to the new centerWGS84 wgs84 coordinates of the map
                        double[] displacementMeters = new double[2] {
                            displacement.x / map.RoundedScaleMultiplier,
                            displacement.z / map.RoundedScaleMultiplier
                        };
                        //Debug.Log("6. TEST: Displacement Meters: " + displacementMeters[0]
                        // + ", " + displacementMeters[1]);


                        double[] centerMeters = new double[2] {
                            map.CenterEPSG900913 [0],
                            map.CenterEPSG900913 [1]
                        };
                        //Debug.Log("7. TEST: Center Meters: " + centerMeters[0] + ", " + centerMeters[1]);
                        centerMeters[0] -= displacementMeters[0];
                        centerMeters[1] -= displacementMeters[1];
                        map.CenterEPSG900913 = centerMeters;
                        //Debug.Log("8. TEST: map centre: " + map.CenterEPSG900913[0] + ", " + map.CenterEPSG900913[1]);

#if DEBUG_LOG
    					Debug.Log("DEBUG: Map.Update: new centerWGS84 wgs84: " + centerWGS84[0] + ", " + centerWGS84[1]);
#endif
                    }

                    map.HasMoved = true;
                }

            }


        }

        private void twoFingerTransformHandler(object sender, System.EventArgs e)
        {

            if (esriMap == null)
            {
                esriMap = GameObject.Find("[Map]").transform;
            }

            updateCamera();



        }

        public void manipulationTransformedHandler(object sender, System.EventArgs e)
        {


            updateCamera();

            cam.transform.localPosition -= Vector3.up * (ManipulationGesture.DeltaScale - 1f) * ZoomSpeed;

            if ((Camera.main.transform.position.y) > CameraZoom)
            {
                map.Zoom(-ZoomSpeed);
            }

            else
            {
                map.Zoom(ZoomSpeed);
            }

            CameraZoom = Camera.main.transform.position.y;
            var rotation = Quaternion.Euler(0,
                    0,
                    -ManipulationGesture.DeltaRotation);
            cam.transform.localRotation *= rotation;

            //cam.transform.rotation = ManipulationGesture.DeltaRotation;

            //Debug.Log(CameraZoom);



        }

        public float CameraAngleCurrent()
        {
            float theAngle;

            if (isPerspectiveView == false)
            {
                theAngle = 90.0f;
            }

            else //perspective view is true
            {
                theAngle = 60.0f;
            }


            return theAngle;

        }

        public void ChangePerspective()
        {
            GameObject[] models = GameObject.FindGameObjectsWithTag("Selectable");

            if (isPerspectiveView)
            {
                /*foreach (GameObject model in models)
                {
                    model.transform.localScale = new Vector3( model.transform.localScale.x, 0.005f, model.transform.localScale.z);
                }*/
                destinationAngle = -perspectiveAngle;
                
            }

            else
            {
                /*foreach (GameObject model in models)
                {
                    model.transform.localScale = new Vector3(model.transform.localScale.x, 0.03f, model.transform.localScale.z);
                }*/
                destinationAngle = perspectiveAngle;
            }

            animationStartTime = Time.time;
            isPerspectiveView = !isPerspectiveView;


        }


        public void DrawingTool()
        {
            isDrawing = !isDrawing;
            Debug.Log("isDrawing = " + isDrawing);
        }

        public void ChangeButtonColor()
        {

            buttonToggle = !buttonToggle;

            if (!buttonToggle)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = Color.green;
                button.colors = colors;
            }

            else
            {
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                button.colors = colors;
            }
        }

        private IEnumerator Start()
        {
            numOfShapes = 1;

            //Debug.Log("Screen position start: " + screenPosition);
            cam = GameObject.Find("Camera").transform;
            //Debug.Log("Cam local positioon: "+ cam.transform.localPosition);
            //Debug.Log("cam rotation: " + cam.rotation);
            CameraZoom = Camera.main.transform.position.y;

            var standLayer = GameObject.Find("Camera").GetComponent<StandardLayer>().enabled = false;



            // create the map singleton
            map = MapBehaviour.Instance;
            map.CurrentCamera = Camera.main;


            //map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;

            map.CurrentZoom = 16.0f;
            // UVic

            map.CenterWGS84 = new double[2] { -123.3117, 48.4634 };
            map.UsesLocation = true;
            map.InputsEnabled = true;

            //Debug.Log("Start CenterEPSG900913 [0]: " + map.CenterEPSG900913[0] + ", " +
            //map.CenterEPSG900913[1]);



            layers = new List<LayerBehaviour>();

            // create an Esri tile layer
            EsriTileLayer esriLayer = map.CreateLayer<EsriTileLayer>("Esri");
            esriLayer.BaseURL = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/";

            //Setting up the camera for TouchScript
            //esriLayer.transform.parent = GameObject.Find("Scene").transform;
            //Camera.main.transform.parent = GameObject.Find("Esri").transform;

            layers.Add(esriLayer);

            yield return null;

        }

        void OnApplicationQuit()
        {
            map = null;
        }



#if DEBUG_PROFILE
	void LateUpdate()
	{
		Debug.Log("PROFILE:\n" + UnitySlippyMap.Profiler.Dump());
		UnitySlippyMap.Profiler.Reset();
	}
#endif
    }

}