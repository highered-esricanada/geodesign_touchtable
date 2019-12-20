using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public int minimum;
    public int current;
    public int maximum;
    public Image mask;
    public Image fillRes;
    public Color color;
    //public Image fillComm;
    //public Image fillIndust;




    // Update is called once per frame
    void Update()
    {
        GetCurrentFill();
    }

    public void GetCurrentFill()
    {
        float currentOffset = current - minimum;
        float maximumOffset = maximum - minimum;
        float fillAmount = currentOffset / maximumOffset;
        mask.fillAmount = fillAmount;
        fillRes.color = color;
    }
}
