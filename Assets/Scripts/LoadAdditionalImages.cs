using System.Collections;
using System.Collections.Generic;
using TablePlus.ElementsDB.DBBridge;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoadAdditionalImages : MonoBehaviour
{

    public GameObject prefab;
    public GameObject container;
    // Start is called before the first frame update
    public void loadAdditionalImage(DBCaseResource dbcaseresource)
    {
        StartCoroutine(loadAdditionalImageFromElementDB(dbcaseresource));
    }

    private IEnumerator loadAdditionalImageFromElementDB(DBCaseResource dbcaseresource)
    {
        var createModel = Instantiate(prefab) as GameObject;
        createModel.transform.SetParent(container.transform, false);
        createModel.name = dbcaseresource._MediaTitle;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(Elements.WebServer + dbcaseresource._PathWeb);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Sprite webSprite = SpriteFromTexture2D(myTexture);
            createModel.GetComponent<Image>().sprite = webSprite;
        }


            
        yield return null;
    }

        Sprite SpriteFromTexture2D(Texture2D texture)
        {

            return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }
