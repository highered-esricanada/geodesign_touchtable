using System;
using System.Collections;
using System.Collections.Generic;
using TouchScript.Behaviors;
using TouchScript.Gestures;
using TouchScript.Gestures.TransformGestures;
using TouchScript.Layers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnitySlippyMap.Map;
using UnitySlippyMap.Markers;



namespace Esri
{
    public class ItemDragHandler : MonoBehaviour, IDragHandler, IDropHandler, IPointerDownHandler
    {
        private GameObject canvas;
        private GameObject container;
        private MapBehaviour map;
        private const int EarthRadius = 6378137; //no seams with globe example
        private const double OriginShift = 2 * Math.PI * EarthRadius / 2;

        private GameObject clone;



        public Transform[] allChildren;
        public TransformGesture transformGesture;


        private TestMap testMap;


        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log(this.gameObject.GetType().ToString());
            clone = Instantiate(this.gameObject, transform.parent, false);
            clone.name = this.name;

            var eventDataNew = new PointerEventData(EventSystem.current);
            EventSystem.current.SetSelectedGameObject(clone, eventDataNew);
            OnDrag(eventDataNew);
        }

        public void OnDrag(PointerEventData eventData)
        {

            canvas = GameObject.Find("Canvas");
            transform.position = Input.mousePosition;
            this.transform.SetParent(canvas.transform, false);

        }

        public void OnDrop(PointerEventData eventData)
        {
            RectTransform invPanel = GameObject.Find("Menu_Case").GetComponent<RectTransform>();



            //check to see if the mouse pointer is out of the Menu box
            if (!RectTransformUtility.RectangleContainsScreenPoint(invPanel, Input.mousePosition))
            {
                //hide the Image of 3d model
                this.gameObject.SetActive(false);
                string fileName = this.name;

                //figure out where to drop the modelin world space.
                RaycastHit hit = new RaycastHit();
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 1000))
                {
                    GameObject building = Resources.Load<GameObject>("SKPFiles/" + fileName);
                    building.transform.position = hit.point;

                    //add components
                    var prefab = Instantiate(building, building.transform.position, Quaternion.identity);
                    Transform[] allChildren = prefab.GetComponentsInChildren<Transform>();
                    foreach (Transform child in allChildren)
                    {
                        child.tag = "Selectable";
                    }

                    prefab.AddComponent<BoxCollider>();

                    //adjusting scale
                    Vector3 scale = prefab.transform.localScale;
                    scale.Set(0.0005f, 0.0005f, 0.0005f);
                    prefab.transform.localScale = scale;

                    double[] centerMeters = new double[2] {
                    map.CenterEPSG900913 [0],
                    map.CenterEPSG900913 [1]
                    };

                    Debug.Log("Centre of Map in Metres: " + centerMeters[0] + ", " + centerMeters[1]);

                    double[] displacementMeters = new double[2] {
                    hit.point.x / map.RoundedScaleMultiplier,
                    hit.point.z / map.RoundedScaleMultiplier
                    };

                    Debug.Log("displacement (hit point), in Metres: " +
                        displacementMeters[0] + ", " + displacementMeters[1]);

                    double[] buildingPosition = new double[2] {
                    centerMeters[0] += displacementMeters[0],
                    centerMeters[1] += displacementMeters[1]
                    };
                    Debug.Log("Building position in Metres, EPSG900913: " + centerMeters[0] + ", " + centerMeters[1]);

                    //call the latlon conversion method

                    var worldPos = new Vector2((float)buildingPosition[0], (float)buildingPosition[1]);

                    worldPos = ConvertDropPositionToLatLon(worldPos);

                    double[] buildingWorldPos = new double[2]
                    {
                    (double)worldPos.x,
                    (double)worldPos.y
                    };


                    Debug.Log("Position: " + worldPos.x + ", " + worldPos.y);

                    map.CreateMarker<MarkerBehaviour>(fileName, buildingWorldPos, prefab);
                    prefab.AddComponent<TransformGesture>();

                    transformGesture = prefab.GetComponent<TransformGesture>();
                    prefab.AddComponent<Transformer>();
                    prefab.AddComponent<TapGesture>();
                    prefab.AddComponent<LoadCaseMenu>();
                    prefab.AddComponent<CaseDragHandler>();

                    prefab.gameObject.tag = "Selectable";

                }


            }


        }

        


        // Start is called before the first frame update
        void Start()
        {
            map = FindObjectOfType(typeof(MapBehaviour)) as MapBehaviour;

            testMap = GameObject.Find("Test").GetComponent<TestMap>();



        }




        //takes touch position and converts it to lat/long,
        //in order for Marker Behaviour to be used in TestMap.cs
        private Vector2 ConvertDropPositionToLatLon(Vector2 m)
        {
            var vx = (float)(m.x / OriginShift) * 180;
            var vy = (float)(m.y / OriginShift) * 180;
            vy = (float)(180 / Math.PI * (2 * Math.Atan(Math.Exp(vy * Math.PI / 180)) - Math.PI / 2));

            //Debug.Log("Position: " + vx + ", " + vy);
            return new Vector2(vx, vy);

        }


    }
}