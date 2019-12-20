using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Esri
{
    public class LoadModels : MonoBehaviour
    {
        public Sprite[] models;
        public GameObject prefab;
        public GameObject container;


        // Start is called before the first frame update
        void Start()
        {

            StartCoroutine(LoadingModels());

        }

        private IEnumerator LoadingModels()
        {

            //loads all the sprite images od 3D models
            models = Resources.LoadAll<Sprite>("Images");

            foreach (var m in models)
            {
                //instantiate prefab image UI
                var createModel = Instantiate(prefab) as GameObject;
                createModel.transform.SetParent(container.transform, false);
                createModel.name = m.name;
                createModel.GetComponent<Image>().sprite = m;

                createModel.AddComponent<ItemDragHandler>();





            }

            yield return null;
        }


        public void OnClick()
        {
            Debug.Log("You clicked Model image: " + this.name);


        }

        // Update is called once per frame
        void Update()
        {


        }
    }

}