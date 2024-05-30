using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextPopUpsPanel : Panel
{
    [SerializeField] GameObject UiParent;
    [SerializeField] GameObject PopupPrefab;
    [SerializeField] float AnimationSpeed = 1.5f;
    [SerializeField] float InitialScale = 0f;
    [SerializeField] float Final_Scale = 2f;
    [SerializeField] Color InitialColor = Color.white;
    [SerializeField] Vector2 InitialPosition;


    public void PlayText(string text)
    {
        PlayText(text, InitialColor, AnimationSpeed, InitialScale, Final_Scale , InitialPosition);
    }
    public void PlayText(string text, float speed)
    {
        PlayText(text, InitialColor, speed, InitialScale, Final_Scale, InitialPosition);
    }
    public void PlayText(string text, float speed, Vector3 InitialPosition)
    {
        PlayText(text, InitialColor, speed, InitialScale, Final_Scale, InitialPosition);
    }
    public void PlayText(string text, Vector3 InitialPosition)
    {
        PlayText(text, InitialColor, AnimationSpeed, InitialScale, Final_Scale, InitialPosition);
    }
    public void PlayText(string text, Color startingColor, float speed, float initialScale, float finalScale)
    {
        PlayText(text, InitialColor, AnimationSpeed, InitialScale, Final_Scale, InitialPosition);
    }
    public void PlayText(string text,Color startingColor,  float speed, float initialScale, float finalScale, Vector2 InitialPosition)
    {
        Text newInstance = Instantiate(PopupPrefab.gameObject, UiParent.transform).GetComponent<Text>();
        newInstance.rectTransform.anchoredPosition = InitialPosition;
        newInstance.text = text;
        newInstance.color = startingColor;

        newInstance.gameObject.transform.localScale = new Vector3(1f, 1f, 1f) * initialScale;
        StartCoroutine(textAnimationCoroutine(newInstance, speed, finalScale));
    }

    private IEnumerator textAnimationCoroutine(Text text, float speed, float finalScale)
    {
        float speedMode = (Application.platform == RuntimePlatform.Android) ? speed * 2f : (speed * 0.5f);

        while (text.transform.lossyScale.magnitude < finalScale)
        {
            text.transform.localScale += (new Vector3(speedMode, speedMode, speedMode) * 0.02f);
            text.color -= new Color(0f, 0f, 0f, speedMode * 0.02f);
            yield return new WaitForSecondsRealtime(0.02f);
        }
    }
}
