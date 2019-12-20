using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{

    public Sprite tabIdle;
    public Sprite tabHover;
    public Sprite tabActive;
    public TabButton selectedTab;
    public int index;

    public List<GameObject> objectsToSwap;
    public List<TabButton> tabButtons;

    public void Subscribe(TabButton button)
    {
        if(tabButtons == null)
        {
            tabButtons = new List<TabButton>();
            
        }

        tabButtons.Add(button);

    }

    public void OnTabEnter(TabButton button)
    {
        ResetTabs();
        if(selectedTab == null || button != selectedTab)
        {
            button.background.sprite = tabHover;
        }
    }

    public void OnTabExit(TabButton button)
    {

        ResetTabs();
    }

    public void OnTabSelected(TabButton button)
    {

        if(selectedTab != null)
        {
            selectedTab.Deselect();
        }

        selectedTab = button;

        selectedTab.Select();


        ResetTabs();

        button.background.sprite = tabActive;

        //gets the index for each tab button, where 0 = case menu, 1 = data menu

        index = button.transform.GetSiblingIndex();
        Debug.Log("Sibling Index: " + index);


        for(int i = 0; i < objectsToSwap.Count; i++)
        {
           
            if (i == index)
            {
                objectsToSwap[i].SetActive(true);
            }

            
            else
            {
                objectsToSwap[i].SetActive(false);
            }
        
        }

 }

    public void ResetTabs()
    {
        foreach(TabButton button in tabButtons)
        {
            if(selectedTab != null &&  button == selectedTab) { continue; }
            button.background.sprite = tabIdle;
        }
    }
}
