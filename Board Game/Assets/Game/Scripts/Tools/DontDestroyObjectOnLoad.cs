using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyObjectOnLoad : MonoBehaviour
{
    protected static List<DontDestroyObjectOnLoad> dontDestroyObjectOnLoads = new List<DontDestroyObjectOnLoad>();

    private void Awake()
    {
        if(dontDestroyObjectOnLoads.Contains(this))
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

}
