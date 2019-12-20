using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Load3DModels : MonoBehaviour
{
    public Object[] models3D;



    public IEnumerator LoadModels3D()
    {
        models3D = Resources.LoadAll<GameObject>("SKPFiles");

        foreach(var m_3d in models3D)
        {
            Debug.Log(m_3d.name);
        }


        yield return null;
    }

   


    // Start is called before the first frame update
    void Start()
    {
     
        StartCoroutine(LoadModels3D());

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
