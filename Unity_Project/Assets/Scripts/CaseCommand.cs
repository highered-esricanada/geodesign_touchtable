using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CaseCommand : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Button btn = this.GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {

        //CaseControl og = this.transform.parent.parent.gameObject.GetComponent<CaseControl>();
        //og.CaseCommand(this.name);
        SendMessageUpwards("CaseCommand", this.name);
    }
}
