using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Through_Scenes : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DebugManager.Log(this.gameObject.name);
    }
}
