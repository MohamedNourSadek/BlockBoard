using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Through_Scenes : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DebugManager.Instance.Log(this.gameObject.name);
    }
}
