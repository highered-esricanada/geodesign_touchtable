using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class ExtrudeBuilding : MonoBehaviour
{
    public Transform floor;
    public Transform roof;
    public Transform level;

     float resPercent;
     float commPercent;
     float indusPercent;

    public Image fillI;
    public Image fillR;
    public Image fillC;


    private List<GameObject> levels;
    private List<GameObject> commercialList;
    private List<GameObject> residentialList;
    private List<GameObject> industrialList;

    private double resCircleFill;
    private double commCircleFill;
    private double indusCircleFill;


    public Material[] residentialMaterials;
    public Material[] commercialMaterials;
    public Material[] industrialMaterials;
    public Material[] originalMaterials;

    public Button residentUp;
    public Button residentDown;
    public Button commercialUp;
    public Button commericalDown;
    public Button industrialUp;
    public Button industrialDown;

    TextMeshProUGUI commercialValue;
    TextMeshProUGUI residentialValue;
    TextMeshProUGUI industrialValue;

    int commercialFloors;
    int residentialFloors;
    int industrialFloors;

    bool isGrey = true;

    Vector3 RoofHeight;
    Vector3 LevelHeight;

    GameObject topLevel;
    private int value;
    private int currValue;
    int numberOfFloors = 1;

    
    public IEnumerator AddFloors(int num)
    {
        roof.position += LevelHeight;

        GameObject newLevel = Instantiate(topLevel);
        levels.Add(newLevel);

        if(num == 0)
        {
            residentialList.Add(newLevel);
        }

        else if(num == 1)
        {
            commercialList.Add(newLevel);
        }

        else
        {
            industrialList.Add(newLevel);
        }

        newLevel.AddComponent<MeshFilter>();
        newLevel.AddComponent<MeshRenderer>();

        Mesh newMesh = new Mesh();
        newLevel.GetComponent<MeshFilter>().mesh = newMesh;
        newLevel.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Commercial");


        newLevel.transform.parent = GameObject.Find("Construct").transform;
        newLevel.transform.position += (LevelHeight*numberOfFloors);
        numberOfFloors++;
        newLevel.name = "Level #" + numberOfFloors;

        yield return null;


    }

    public IEnumerator RemoveFloors(int num)
    {
        if(numberOfFloors <= 1)
        {
            yield return null;
        }
        GameObject levelToDelete = GameObject.Find("Level #" + numberOfFloors);
        levels.Remove(levelToDelete);

        if (num == 0)
        {
            residentialList.Remove(levelToDelete);
        }

        else if (num == 1)
        {
            commercialList.Remove(levelToDelete);
        }

        else if(num ==2)
        {
            industrialList.Remove(levelToDelete);
        }
        Destroy(levelToDelete);
        roof.position -= LevelHeight;
        numberOfFloors--;

        yield return null;
    }


    // Start is called before the first frame update
    void Start()
    {
        

        //starting number of floors
        residentialFloors = 1;
        commercialFloors = 1;
        industrialFloors = 0;
        

        //setting up numbers corr5esponding to type f level added
        commercialValue = GameObject.Find("Commerical_Value").GetComponent<TextMeshProUGUI>();
        commercialValue.text = commercialFloors.ToString();
        residentialValue = GameObject.Find("Residential_Value").GetComponent<TextMeshProUGUI>();
        residentialValue.text = residentialFloors.ToString(); 
        industrialValue = GameObject.Find("Industrial_Value").GetComponent<TextMeshProUGUI>();
        industrialValue.text = industrialFloors.ToString();


       //instantiate the lists to store different floors in
        levels = new List<GameObject>();
        residentialList = new List<GameObject>();
        commercialList = new List<GameObject>();
        industrialList = new List<GameObject>();
        

        var groundFloor = GameObject.Find("Floor");
        groundFloor.name = "Level #0";
        //keep track of level closest to roof
        topLevel = GameObject.Find("Level");
        topLevel.name = "Level #1";

        //get a vector to add onto/subtract from current position;
        LevelHeight = new Vector3(0, (roof.transform.position.y-level.transform.position.y), 0);

        //groundfloor will be at index(0)
        levels.Add(groundFloor);
        commercialList.Add(groundFloor);

        //toplevel will be at index(1);
        levels.Add(topLevel);
        residentialList.Add(topLevel);

        GetCurrentFill();

    }

    // Update is called once per frame
    void Update()
    {
        commercialValue.text = commercialFloors.ToString();
        residentialValue.text = residentialFloors.ToString();
        industrialValue.text = industrialFloors.ToString();

    }

    public void IncreaseR()
    {
        residentialFloors++;
        StartCoroutine(AddFloors(0));

        GetCurrentFill();
    }
    public void DecreaseR()
    {
        if(residentialList.Count == 1)
        {
            return;
        }
        
        if(residentialFloors > 1)
        {
            residentialFloors--;
            StartCoroutine(RemoveFloors(0));
        }

        GetCurrentFill();

    }
    public void IncreaseC()
    {
        commercialFloors++;
        StartCoroutine(AddFloors(1));

        GetCurrentFill();
    }
    public void DecreaseC()
    {
        if (commercialList.Count == 1)
        {
            return;
        }

        if (commercialFloors > 1)
        {
            commercialFloors--;
            StartCoroutine(RemoveFloors(1));
        }

        GetCurrentFill();
    }
    public void IncreaseI()
    {
        industrialFloors++;
        StartCoroutine(AddFloors(2));

        GetCurrentFill();
    }
    public void DecreaseI()
    {
        if (industrialList.Count == 0)
        {
            return;
        }

        if (industrialFloors > 0)
        {
            industrialFloors--;
            StartCoroutine(RemoveFloors(2));
        }

        GetCurrentFill();
    }

    public void GetCurrentFill()
    {
        //residential amount will always be 1 for the FILL
        fillR.fillAmount = 1.0f;
        resPercent = residentialList.Count *1.0f / levels.Count *1.0f;

        //industrial is the top layer so it's calculation is based on its representative size
        fillI.fillAmount = industrialList.Count * 1.0f / levels.Count * 1.0f;
        
        
        //commerical amount is...
        commPercent = commercialList.Count / levels.Count;
        fillC.fillAmount = 1.0f - resPercent;


        Debug.Log("Res floors (" + residentialList.Count + ")" + "/" + "total floors (" + levels.Count + ") = " + (residentialList.Count * 1.0f / levels.Count * 1.0f));
        Debug.Log("Comm floors (" + commercialList.Count + ")" + "/" + "total floors (" + levels.Count + ") = " + (commercialList.Count * 1.0f / levels.Count * 1.0f));
        Debug.Log("Indus floors (" + industrialList.Count + ")" + "/" + "total floors (" + levels.Count + ") = " + (industrialList.Count * 1.0f / levels.Count * 1.0f));

        Debug.Log("Res Fill: " + fillR.fillAmount);
        Debug.Log("Indust Fill: " + fillI.fillAmount);
        Debug.Log("Comm Fill: " + fillC.fillAmount);

        Debug.Log("Pie Recalculation: " + "R(" + resPercent + ")" + " + "
            + "C(" + fillC.fillAmount + ")" + " = " + (resPercent + fillC.fillAmount));
    }

    public void WhichMaterial()
    {
        isGrey = !isGrey;
        Debug.Log("isGrey = " + isGrey);
        if (!isGrey)
        {
            ChangeFloorMaterialToColor();
        }

        else
        {
            ChangeFloorToMaterialGrey();
        }
    }
    
    
    public void ChangeFloorMaterialToColor()
    {
        for(int i = 0; i < residentialList.Count; i++)
        {
            MeshRenderer meshRenderer = GameObject.Find("Level #" + (levels.Count - 1 - i) + "/Group1/Group2/Mesh1").GetComponent<MeshRenderer>();
            Material[] oldMaterial = meshRenderer.sharedMaterials;
            oldMaterial = residentialMaterials;
            meshRenderer.sharedMaterials = oldMaterial;


        }

        for (int i = 0; i < industrialList.Count; i++)
        {
            var floorsLeft = levels.Count - residentialList.Count;
            MeshRenderer meshRenderer = GameObject.Find("Level #" + (floorsLeft - 1 - i) + "/Group1/Group2/Mesh1")
                .GetComponent<MeshRenderer>();
            Material[] oldMaterial = meshRenderer.sharedMaterials;
            oldMaterial = industrialMaterials;
            meshRenderer.sharedMaterials = oldMaterial;

        }

        for (int i = 0; i < (levels.Count - residentialList.Count - industrialList.Count); i++)
        {
            var floorLevel = levels.Count - residentialList.Count - industrialList.Count - 1 - i;


            if(floorLevel > 0)
            {
                MeshRenderer meshRenderer = GameObject.Find("Level #" + (floorLevel) + "/Group1/Group2/Mesh1")
                    .GetComponent<MeshRenderer>();
                Material[] oldMaterial = meshRenderer.sharedMaterials;
                oldMaterial = commercialMaterials;
                meshRenderer.sharedMaterials = oldMaterial;


            }

            else
            {

                MeshRenderer meshRenderer = GameObject.Find("Level #" + (floorLevel) + "/Mesh2")
                    .GetComponent<MeshRenderer>();
                Material[] oldMaterial = meshRenderer.sharedMaterials;
                oldMaterial = commercialMaterials;
                meshRenderer.sharedMaterials = oldMaterial;
            }
        }
    }

    public void ChangeFloorToMaterialGrey()
    {
        for(int i = 0; i < levels.Count; i++)
        {
            var levelNum = levels.Count - 1 - i;

            if (levelNum > 0)
            {
                MeshRenderer meshRenderer = GameObject.Find("Level #" + (levelNum) + "/Group1/Group2/Mesh1")
                    .GetComponent<MeshRenderer>();
                Material[] oldMaterial = meshRenderer.sharedMaterials;
                oldMaterial = originalMaterials;
                meshRenderer.sharedMaterials = oldMaterial;
            }

            else
            {
                MeshRenderer meshRenderer = GameObject.Find("Level #" + (levelNum) + "/Mesh2")
                    .GetComponent<MeshRenderer>();
                Material[] oldMaterial = meshRenderer.sharedMaterials;
                oldMaterial = originalMaterials;
                meshRenderer.sharedMaterials = oldMaterial;
            }
        }
    }
}
