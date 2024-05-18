using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connection_Animation : MonoBehaviour
{
    [SerializeField] private List<string> keys = new List<string>();
    [SerializeField] private float speed = 0.2f;

    private Text myText;
    private int currentKey = 0;

    private void Awake()
    {
        myText = GetComponent<Text>();
    }
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Animation());
    }
    private void OnDisable()
    {
        StopAllCoroutines();
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
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime / speed);
        }
    }
}
