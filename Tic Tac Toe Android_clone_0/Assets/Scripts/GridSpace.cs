using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSpace : MonoBehaviour
{

    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    private LocalGameController localGameController;

    public void SetSpace()
    {
        buttonText.text = localGameController.GetPlayerSide();
        button.interactable = false;
        localGameController.EndTurn();
    }

    public void SetLocalGameControllerReference(LocalGameController controller)
    {
        localGameController = controller;
    }
}
