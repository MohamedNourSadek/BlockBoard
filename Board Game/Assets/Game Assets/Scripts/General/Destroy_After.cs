using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy_After : MonoBehaviour
{
    [SerializeField] float time = 5f;

    private void Start()
    {
        StartCoroutine(DestroyAfter());
    }
    
    IEnumerator DestroyAfter()
    {
        yield return new WaitForSeconds(time);
        DestroyImmediate(this.gameObject);
    }
}
