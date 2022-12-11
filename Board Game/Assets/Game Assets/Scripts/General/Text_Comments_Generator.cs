using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Text_Comments_Generator : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] GameObject Text_Prefab;
    [SerializeField] public float text_Speed = 1.5f;
    [SerializeField] public float text_initialScale = 0f;
    [SerializeField] public Color text_initial_Color = Color.white;
    [SerializeField] public float text_final_Scale = 2f;
    [SerializeField] public Vector2 Initial_Position;
    public void PlayText(string text)
    {
        PlayText(text, text_initial_Color, text_Speed, text_initialScale, text_final_Scale , Initial_Position);
    }

    public void PlayText(string text, float speed)
    {
        PlayText(text, text_initial_Color, speed, text_initialScale, text_final_Scale, Initial_Position);
    }

    public void PlayText(string text, float speed, Vector3 InitialPosition)
    {
        PlayText(text, text_initial_Color, speed, text_initialScale, text_final_Scale, InitialPosition);
    }

    public void PlayText(string text, Vector3 InitialPosition)
    {
        PlayText(text, text_initial_Color, text_Speed, text_initialScale, text_final_Scale, InitialPosition);
    }

    public void PlayText(string text, Color starting_Color, float speed, float initialScale, float finalScale)
    {
        PlayText(text, text_initial_Color, text_Speed, text_initialScale, text_final_Scale, Initial_Position);
    }
    
    public void PlayText(string text,Color starting_Color,  float speed, float initialScale, float finalScale, Vector2 InitialPosition)
    {
        Text t = Instantiate(Text_Prefab.gameObject, canvas.transform).GetComponent<Text>();
        t.rectTransform.anchoredPosition = InitialPosition;
        t.text = text;
        t.color = starting_Color;

        t.gameObject.transform.localScale = new Vector3(1f, 1f, 1f) * initialScale;
        StartCoroutine(text_Animation(t, speed, finalScale));
    }

    IEnumerator text_Animation(Text text, float speed, float finalScale)
    {
        float speed_mod = (Application.platform == RuntimePlatform.Android) ? speed * 2f : (speed * 0.5f);

        while (text.transform.lossyScale.magnitude < finalScale)
        {
            text.transform.localScale += (new Vector3(speed_mod, speed_mod, speed_mod) * 0.02f);
            text.color -= new Color(0f, 0f, 0f, speed_mod * 0.02f);
            yield return new WaitForSecondsRealtime(0.02f);
        }
    }
}
