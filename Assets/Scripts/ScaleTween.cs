using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScaleTween : MonoBehaviour
{

    public Button button;

    //when tab button "power off" is selected.
    public void OnOpen()
    {
        this.gameObject.SetActive(true);
        this.transform.localScale = new Vector3(0f, 0f, 0f);
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.3f).setDelay(0.5f);

        button.onClick.AddListener(OnQuit);
    }

    public void OnQuit()
    {
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.3f).setDelay(0.5f);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit ();
#endif
    }
}


