using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connection_Animation : MonoBehaviour
{
    [SerializeField] Text myText;
    [SerializeField] List<string> keys = new List<string>();
    [SerializeField] float Speed = 10;

    int currentKey = 0;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Animation());
    }

    IEnumerator Animation()
    {
        while (true)
        {
            if (currentKey < keys.Count - 1)
                currentKey++;
            else
                currentKey = 0;

            myText.text = keys[currentKey];
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime / Speed);
        }
    }
}
