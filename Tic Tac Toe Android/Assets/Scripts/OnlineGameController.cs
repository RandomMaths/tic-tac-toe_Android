using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System;


public class OnlineGameController : NetworkBehaviour
{
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

    [Rpc(SendTo.Everyone)]
    public void StartGameRpc()
    {
        if(NetworkManager.Singleton.IsHost)
            SetBoardInteractable(true);

        SetPlayerButtons(false);
        startInfo.SetActive(false);
    }

    [Rpc(SendTo.Everyone)]
    private void SetStartingSideRpc(int startingSide)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            hostSide.Value = startingSide;
            currentTurn.Value = startingSide;
        }

        SetPlayerSideRpc(startingSide);
        StartGameRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void SetPlayerSideRpc(int side)
    {
        if(gameOverText.text == "")
        {
            if (side == 1)
            {
                SetPlayerColor(playerX, playerO);
            }
            else if (side == 2)
            {
                SetPlayerColor(playerO, playerX);
            }
        } else
        {
            SetPlayerColorInactive();
        }
    }

    public void UpdateMade(string index)
    {
        int grid = int.Parse(index.Substring(index.Length - 1)) - 1;

        UpdateGridRpc(grid);
        UpdateButtonsRpc(grid);

        CheckBoardRpc();
  
        //Switch Sides
        ToggleSideRpc();
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    private void UpdateGridRpc(int grid)
    {
        integerList[grid] = currentTurn.Value;

    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    private void ToggleSideRpc()
    {
        currentTurn.Value = (currentTurn.Value == 1) ? 2 : 1;
        SetPlayerSideRpc(currentTurn.Value);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void UpdateButtonsRpc(int grid)
    {
        SetBoardInteractable(false);
        buttonList[grid].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (currentTurn.Value == 1) ? "X" : "O";
        if(NetworkManager.Singleton.IsHost && currentTurn.Value != hostSide.Value)
        {
            UpdateButtons();
        } else if(!NetworkManager.Singleton.IsHost && currentTurn.Value == hostSide.Value)
        {
            UpdateButtons();
        }
        
        buttonList[grid].interactable = false;
    }

    [Rpc(SendTo.Everyone)]
    private void CheckBoardRpc()
    {
        for (int i = 0; i <= 2; i++)
        {
            if (integerList[i * 3 + 0] == currentTurn.Value && integerList[i * 3 + 1] == currentTurn.Value && integerList[i * 3 + 2] == currentTurn.Value)
            {
                GameOverRpc(false);
            }

            if (integerList[i] == currentTurn.Value && integerList[1 * 3 + i] == currentTurn.Value && integerList[3 * 2 + i] == currentTurn.Value)
            {
                GameOverRpc(false);
            }
        }

        if (integerList[0] == currentTurn.Value && integerList[4] == currentTurn.Value && integerList[8] == currentTurn.Value)
        {
            GameOverRpc(false);
        }

        if (integerList[2] == currentTurn.Value && integerList[4] == currentTurn.Value && integerList[6] == currentTurn.Value)
        {
            GameOverRpc(false);
        }

        if (GridComplete())
        {
            GameOverRpc(true);
        }
    }

    [Rpc(SendTo.Everyone)]
    private void GameOverRpc(bool draw)
    {
        SetBoardInteractable(false);
        SetPlayerColorInactive();
        restartButton.SetActive(true);
        gameOverPanel.SetActive(true);

        if (draw){
            gameOverText.text = "DRAW";
        } else if (!draw)
        {
            gameOverText.text = "You Lost";
            if (NetworkManager.Singleton.IsHost && currentTurn.Value == hostSide.Value)
            {
                gameOverText.text = "You Won";
            }
            else if (!NetworkManager.Singleton.IsHost && currentTurn.Value != hostSide.Value)
            {
                gameOverText.text = "You Won";
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void RestartGameRpc()
    {
        restartButton.SetActive(false);
        gameOverPanel.SetActive(false);
        gameOverText.text = "";
        if (NetworkManager.Singleton.IsHost)
        {
            playerX.button.interactable = true;
            playerO.button.interactable = true;
            startInfo.SetActive(true);
        }
        SetPlayerColorInactive();
        ResetButtons();
        ResetNetworkVariablesRpc();
    }

    [Rpc(SendTo.Owner)]
    private void ResetNetworkVariablesRpc()
    {
        currentTurn.Value = 0;
        hostSide.Value = 0;
        for(int i = 0;i < 9;i++)
        {
            integerList[i] = 0;
        }
    }

    private bool GridComplete()
    {
        for(int i = 0;i < 9; i++)
        {
            if(integerList[i] == 0)
                return false;
        }
        return true;
    }

    private void ResetButtons()
    {
        foreach(Button button in buttonList)
        {
            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    private void UpdateButtons()
    {
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
            SetStartingSideRpc(1);
        });

        playerO.button.onClick.AddListener(delegate
        {
            SetStartingSideRpc(2);
        });

        restartButton.GetComponent<Button>().onClick.AddListener(delegate
        {
            RestartGameRpc();
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
