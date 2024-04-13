using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System;


public class OnlineGameController : NetworkBehaviour
{
    private int moveCount;
    public NetworkVariable<int> currentTurn;
    public NetworkVariable<int> hostSide;
    public NetworkList<int> integerList;

    [SerializeField] private Button[] buttonList;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject restartButton;

    [SerializeField] private Player playerX;
    [SerializeField] private Player playerO;
    [SerializeField] private PlayerColor activePlayerColor;
    [SerializeField] private PlayerColor inactivePlayerColor;
    [SerializeField] private GameObject startInfo;

    public override void OnNetworkSpawn()
    {   
        //cells = new int[9];
        AddButtonListener();
        SetBoardInteractable(false);

        if (NetworkManager.Singleton.IsHost)
        {
            playerX.button.interactable = true;
            playerO.button.interactable = true;
            InitialiseNetworkList();
        } else if(!NetworkManager.Singleton.IsHost)
        {
            playerX.button.interactable = false;
            playerO.button.interactable = false ;
            startInfo.SetActive(false);
        } 

        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
    }

    private void Awake()
    {
        integerList = new NetworkList<int>();
        hostSide = new NetworkVariable<int>(0);
        currentTurn = new NetworkVariable<int>(0);
    }

    private void InitialiseNetworkList()
    {
        for(int i = 0;i < 9; i++)
        {
            integerList.Add(0);
        }
    }

    public void StartGame()
    {
        SetBoardInteractable(true);
        SetPlayerButtons(false);
        startInfo.SetActive(false);
    }

    private void SetStartingSide(int startingSide)
    {
        hostSide.Value = startingSide;
        currentTurn.Value = startingSide;
        
        if(currentTurn.Value == 1)
        {
            SetPlayerColor(playerX, playerO);
        }
        else if(currentTurn.Value == 2)
        {
            SetPlayerColor(playerO, playerX);
        }

        StartGame();
        StartClientSideClientRpc(startingSide);
    }

    public void UpdateMade(string index)
    {
        int grid = int.Parse(index.Substring(index.Length - 1)) - 1;
        
        if (NetworkManager.Singleton.IsHost)
        {
            integerList[grid] = 1;
            //Disableing button for host
           
            for (int i = 0; i < integerList.Count; i++)
            {
                if (integerList[i] == 0)
                {
                    buttonList[i].interactable = true;
                }
                else
                {
                    buttonList[i].interactable = false;
                }
            }
            UpdateButtonsClientRpc(grid);
            
            //Also disable for client
            UpdatePlayerClientRpc();
        } else if (!NetworkManager.Singleton.IsHost)
        {
            //Disbaling button for client side
            
            for (int i = 0; i < integerList.Count; i++)
            {
                if (integerList[i] == 0)
                {
                    buttonList[i].interactable = true;
                }
                else
                {
                    buttonList[i].interactable = false;
                }
            }
            //Also disable for host side
            UpdateButtonsServerRpc(grid);
            
            if (playerX.panel.color == activePlayerColor.panelColor)
            {
                SetPlayerColor(playerO, playerX);
            }
            else if (playerO.panel.color == activePlayerColor.panelColor)
            {
                SetPlayerColor(playerX, playerO);
            }
            UpdatePlayerClientRpc();
        }
        ToggleSideServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleSideServerRpc()
    {
        currentTurn.Value = (currentTurn.Value == 1) ? 2 : 1;
        Debug.Log(currentTurn.Value);
    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdateButtonsServerRpc(int grid)
    {
        //cells[grid] = 1;
        buttonList[grid].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentTurn.Value.ToString();
        integerList[grid] = 1;

        for (int i = 0; i < integerList.Count; i++)
        {
            if (integerList[i] == 0)
            {
                buttonList[i].interactable = true;
            }
            else
            {
                buttonList[i].interactable = false;
            }
        }
       

        if (playerX.panel.color == activePlayerColor.panelColor)
        {
            SetPlayerColor(playerO, playerX);
        }
        else if (playerO.panel.color == activePlayerColor.panelColor)
        {
            SetPlayerColor(playerX, playerO);
        }

        buttonList[grid].interactable = false;
        UpdateButtonsClientRpc(grid);
    }

    [ClientRpc]
    private void UpdateButtonsClientRpc(int grid)
    {
        //cells[grid] = 1;
        buttonList[grid].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = currentTurn.Value.ToString();

        for (int i = 0; i < integerList.Count; i++)
        {
            if (integerList[i] == 0)
            {
                buttonList[i].interactable = true;
            }
            else
            {
                buttonList[i].interactable = false;
            }
        }

        buttonList[grid].interactable = false;
    }

    [ClientRpc]
    private void UpdatePlayerClientRpc()
    {
        if (playerX.panel.color == activePlayerColor.panelColor)
        {
            SetPlayerColor(playerO, playerX);
        }
        else if (playerO.panel.color == activePlayerColor.panelColor)
        {
            SetPlayerColor(playerX, playerO);
        }
    }

    [ClientRpc]
    private void StartClientSideClientRpc(int startingSide)
    {
        StartGame();

        if (startingSide == 1)
        {
            SetPlayerColor(playerX, playerO);
        }
        else if(startingSide == 2)
        {
            SetPlayerColor(playerO, playerX);
        }
    }

    private void SetPlayerColor(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    private void SetPlayerColorInactive()
    {
        playerX.panel.color = inactivePlayerColor.panelColor;
        playerX.text.color = inactivePlayerColor.textColor;
        playerO.panel.color = inactivePlayerColor.panelColor;
        playerO.text.color = inactivePlayerColor.textColor;
    }
    
    private void AddButtonListener()
    {
        foreach (Button button in buttonList){
            button.onClick.AddListener(delegate{
                UpdateMade(button.name);
            });
            button.interactable = false;
        }

        playerX.button.onClick.AddListener(delegate
        {
            SetStartingSide(1);
        });

        playerO.button.onClick.AddListener(delegate
        {
            SetStartingSide(2);
        });
    }

    private void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    private void SetPlayerButtons(bool toggle)
    {
        playerX.button.interactable = toggle;
        playerO.button.interactable = toggle;
    }

}
