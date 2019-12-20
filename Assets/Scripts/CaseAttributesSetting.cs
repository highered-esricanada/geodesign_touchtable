using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseAttributesSetting : MonoBehaviour
{
    // Start is called before the first frame update

    private bool isShowingCaseAttributes;
    void Start()
    {
        isShowingCaseAttributes = false;
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        if (!isShowingCaseAttributes)
        {
            //showing CaseAttributes
            //disable GeneralDescribe;3DModelView;Viewport
            this.transform.parent.Find("GeneralDescribe").gameObject.SetActive(false);
            this.transform.parent.Find("3DModelView").gameObject.SetActive(false);
            this.transform.parent.Find("Viewport").gameObject.SetActive(false);

            //this.transform.parent.Find("Scrollbar").gameObject.SetActive(true);
            this.transform.parent.Find("CaseAttributes").gameObject.SetActive(true);

            this.transform.Find("Text").GetComponent<Text>().color = Color.red;

            isShowingCaseAttributes = true;
        }
        else
        {
            //showing Mainpage
            //disable Scrollbar;Image;
            this.transform.parent.Find("GeneralDescribe").gameObject.SetActive(true);
            this.transform.parent.Find("3DModelView").gameObject.SetActive(true);
            this.transform.parent.Find("Viewport").gameObject.SetActive(true);

            //this.transform.parent.Find("Scrollbar").gameObject.SetActive(false);
            this.transform.parent.Find("CaseAttributes").gameObject.SetActive(false);


            this.transform.Find("Text").GetComponent<Text>().color = Color.white;
            isShowingCaseAttributes = false;
        }
    }


}
