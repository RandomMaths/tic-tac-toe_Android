using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkContoller : MonoBehaviour
{
    [SerializeField] private GameObject game;
    [SerializeField] private GameObject networking;

    [SerializeField] private Button hostButton, clientButton;
    [SerializeField] private GameObject placeholderPanel;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private GameObject codeArea;
    [SerializeField] private Button joinButton;

    private void Start()
    {
        game.SetActive(false);
        placeholderPanel.SetActive(false);
    }

    public void StartHost()
    {
        ActivatePlaceholderPanel(true);
    }

    public void StartClient()
    {
        ActivatePlaceholderPanel(false);   
    }

    private void ActivatePlaceholderPanel(bool isHost)
    {
        placeholderPanel.SetActive(true);
        if (isHost)
        {
            labelText.text = "Your Code:";
            codeArea.GetComponent<TMP_InputField>().interactable = false;
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "Your MOM";
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            joinButton.interactable = false;
            joinButton.GetComponent<Image>().sprite = null;
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "waiting for client...";
        } 
        else if (!isHost)
        {
            labelText.text = "Enter Code:";
            codeArea.GetComponent<TMP_InputField>().interactable = true;
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            codeArea.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = new Color(152, 152, 152);
            joinButton.interactable = true;
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Join Game";
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 32;
            joinButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.black;
        }
    }
}
