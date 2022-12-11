using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum tile_name { blank, one, two, three, four, five, six}

[System.Serializable]
public class Tile : MonoBehaviour
{
    [SerializeField] public bool Double;
    [SerializeField] public tile_name Up;
    [SerializeField] public tile_name Down;
    [SerializeField] public Vector3 FinalPosition;
}
