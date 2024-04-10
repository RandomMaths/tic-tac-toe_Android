using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSpace : MonoBehaviour
{

    public Button button;
    public TextMeshProUGUI buttonText;

    private LocalGameController gameController;

    public void SetSpace()
    {
        buttonText.text = gameController.GetPlayerSide();
        button.interactable = false;
        gameController.EndTurn();   
    }

    public void SetGameControllerReference(LocalGameController controller)
    {
        gameController = controller;
    }

}
