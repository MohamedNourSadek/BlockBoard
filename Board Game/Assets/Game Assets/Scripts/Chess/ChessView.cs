using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessView : MonoBehaviour
{
    [SerializeField] MeshRenderer _3DView;
    [SerializeField] public GameObject _2DView;
    [SerializeField] public MeshRenderer _2DView_mesh;


    public void Turn_3D()
    {
        _3DView.enabled = true;
        _2DView.SetActive(false);
    }
    public void Turn2D()
    {
        _3DView.enabled = false;
        _2DView.SetActive(true);
    }
}
