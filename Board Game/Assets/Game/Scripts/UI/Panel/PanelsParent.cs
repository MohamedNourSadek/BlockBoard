using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelsParent : MonoBehaviour
{
    private void Awake()
    {
        AddAllPanelsToPanelsList();
    }

    private void AddAllPanelsToPanelsList()
    {
        foreach (Panel panel in GetComponentsInChildren<Panel>(true))
        {
            panel.AddToPanelList();
        }

        foreach (Panel panel in GetComponentsInChildren<Panel>(true))
        {
            panel.AddToPanelList();
        }
    }

}
