using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoController : MonoBehaviour
{
    public static DominoController Instance;


    public DominoController()
    {
        Instance = this;
    }


    public void StartGame()
    {
        DominoGeometery.Instance.gameObject.SetActive(true);
        Panel.GetPanel<DominoPanel>().Show();
        gameObject.SetActive(true);
    }
}
