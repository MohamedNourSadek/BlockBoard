using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdditionalMath : MonoBehaviour
{
    public static float Remap(float Value, Vector2 Range1, Vector2 Range2)
    {
        float perten = Mathf.Clamp((Value - Range1.x) / (Range1.y - Range1.x),0f,1f);
        return (perten * (Range2.y - Range2.x));
    }
}
