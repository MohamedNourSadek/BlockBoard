using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicker_light : MonoBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] float Amplitude;
    [SerializeField] float Frequency = 0.5f;
    [SerializeField] Light l;

    private void Awake()
    {
        l = GetComponent<Light>();
    }
    void FixedUpdate()
    {
        l.intensity = Amplitude * curve.Evaluate(Mathf.Abs(Mathf.Sin(Frequency*Time.timeSinceLevelLoad)));
    }
}
