using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoController : MonoBehaviour
{
    public static DominoController Instance;

    private void Awake()
    {
        Instance = this;
    }


    public void StartGame()
    {
        gameObject.SetActive(true);
        Panel.GetPanel<DominoPanel>().Show();
    }
}
