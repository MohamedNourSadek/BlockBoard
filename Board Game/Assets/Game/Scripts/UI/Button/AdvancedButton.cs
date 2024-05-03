using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedButton : Button
{
    protected override void Awake()
    {
        base.Awake();
        onClick.AddListener(OnButtonPressed);
    }

    public void OnButtonPressed()
    {
        Manager.GetManager<SoundManager>().PlayButtonClick();
    }
}
