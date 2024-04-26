using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player
{
    public Image panel;
    public TextMeshProUGUI text;
    public Button button;
}

[System.Serializable]
public class PlayerColor
{
    public Color textColor;
    public Color panelColor;
}

public class LocalGameController : MonoBehaviour
{
    private string playerSide;

    private int[] values;

    [SerializeField] private GameObject gameModePanel;

    [SerializeField] private TextMeshProUGUI[] buttonList;

    [SerializeField] private GameObject startInfo;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [SerializeField] private GameObject restartButton;

    [SerializeField] private Player playerX;
    [SerializeField] private Player playerO;
    [SerializeField] private PlayerColor activePlayerColor;
    [SerializeField] private PlayerColor inactivePlayerColor;

    //AI variables
    private bool computerPlays;
    private string computerSide;
    private bool computerTurn;
    private int score;
    private int bestScore = int.MinValue;
    private int bestMove;

    private void Awake()
    {
        values = new int[9];
        playerX.button.interactable = false;
        playerO.button.interactable = false;
        gameOverPanel.SetActive(false);
        SetGameControllerReferenceOnButtons();
        restartButton.SetActive(false);
    }

    public void Update()
    {
        bestMove = 0;
        score = 0;
        bestScore = int.MinValue;
        if(computerPlays && computerTurn)
        {
            for(int i = 0;i < 9; i++)
            {
                if (values[i] == 0)
                {
                    values[i] = GetPlayerValue(computerSide);
                    score = MiniMax(0, false);
                    values[i] = 0;
                    if(score >= bestScore)
                    {
                        bestScore = score;
                        bestMove = i;
                    }
                }
            }
            buttonList[bestMove].text = computerSide;
            buttonList[bestMove].GetComponentInParent<Button>().interactable = false;
            EndTurn();
        }
    }

    private int MiniMax(int depth, bool isMaximizing)
    {
        if(CheckWinner() != "N")
        {
            if (CheckWinner() == "D")
                return 0;
            else
                return CheckWinner() == computerSide ? 1 : -1;
        }

        if (isMaximizing)
        {
            int localBestScore = int.MinValue;
            for(int i = 0;i < 9; i++)
            {
                if(values[i] == 0)
                {
                    values[i] = GetPlayerValue(computerSide);
                    int localScore = MiniMax(depth + 1, false);
                    values[i] = 0;
                    localBestScore = Mathf.Max(localScore, localBestScore);
                }
            }
            return localBestScore;
        }
        else
        {
            int localBestScore = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (values[i] == 0)
                {
                    values[i] = GetPlayerValue(playerSide);
                    int localScore = MiniMax(depth + 1, true);
                    values[i] = 0;
                    localBestScore = Mathf.Min(localScore, localBestScore);
                }
            }
            return localBestScore;
        }
    }

    public void SetGameControllerReferenceOnButtons()
    {
        for(int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<GridSpace>().SetLocalGameControllerReference(this);
        }
    }

    public string GetPlayerSide()
    {
        return playerSide;
    }

    public void EndTurn()
    {
        SynchronizeValues();
        if(CheckWinner() != "N")
        {
            GameOver(CheckWinner());
        }
        else
        {
            ChangeSides();
        }
    }

    void SynchronizeValues()
    {
        for(int i = 0; i < values.Length; i++)
        {
            if (buttonList[i].text == "X")
                values[i] = 1;
            if (buttonList[i].text == "O")
                values[i] = 2;
            if (buttonList[i].text == "")
                values[i] = 0;
        }
    }

    string CheckWinner()
    {
        for(int side = 1;side <= 2; side++)
        {
            for (int i = 0; i <= 2; i++)
            {
                if (values[i * 3 + 0] == side && values[i * 3 + 1] == side && values[i * 3 + 2] == side)
                {
                    return (side == 1) ? "X" : "O";
                }

                if (values[i] == side && values[1 * 3 + i] == side && values[3 * 2 + i] == side)
                {
                    return (side == 1) ? "X" : "O";
                }
            }

            if (values[0] == side && values[4] == side && values[8] == side)
            {
                return (side == 1) ? "X" : "O";
            }

            if (values[2] == side && values[4] == side && values[6] == side)
            {
                return (side == 1) ? "X" : "O";
            }
        }

        for(int i = 0;i < values.Length; i++)
            if (values[i] == 0)
                return "N";
        
        return "D";
    }

void SetPlayerColor(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.text.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.text.color = inactivePlayerColor.textColor;
    }

    void SetPlayerColorInactive()
    {
        playerX.panel.color = inactivePlayerColor.panelColor;
        playerX.text.color = inactivePlayerColor.textColor;
        playerO.panel.color = inactivePlayerColor.panelColor;
        playerO.text.color = inactivePlayerColor.textColor;
    }

    void GameOver(string winningPlayer)
    {
        SetBoardInteractable(false);

        if (winningPlayer.Equals("D"))
        {
            SetGameOverText("DRAW");
            SetPlayerColorInactive();
        }
        else
            SetGameOverText(winningPlayer + " Wins!");

        restartButton.SetActive(true);
    }

    void ChangeSides()
    {
        if(!computerPlays)
            playerSide = (playerSide == "X") ? "O" : "X";
        if (computerPlays)
            computerTurn = !computerTurn;

        if(playerSide == "X")
        {
            SetPlayerColor(playerX, playerO);
        }
        else
        {
            SetPlayerColor(playerO, playerX);
        }
    }

    void SetGameOverText(string value)
    {
        gameOverPanel.SetActive(true);
        gameOverText.text = value;
    }

    public void SetGameMode(string mode)
    {
        playerX.button.interactable = true;
        playerO.button.interactable = true;
        if (mode == "Single")
            computerPlays = true;
        Debug.Log(computerPlays);
        gameModePanel.SetActive(false);
    }

    public void SetStartingSide(string startingSide)
    {
        playerSide = startingSide;
        if(playerSide == "X")
        {
            SetPlayerColor(playerX, playerO);
            if(computerPlays)
                computerSide = "O";
        }
        else
        {
            SetPlayerColor(playerO, playerX);
            if(computerPlays)
                computerSide = "X";
        }

        StartGame();
    }

    void StartGame()
    {
        SetBoardInteractable(true);
        SetPlayerButtons(false);
        startInfo.SetActive(false);
    }

    public void RestartGame()
    {
        computerTurn = false;
        computerPlays = true;
        gameOverPanel.SetActive(false);
        SetPlayerButtons(true);
        SetPlayerColorInactive();
        startInfo.SetActive(true);

        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].text = "";
            values[i] = 0;
        }

        restartButton.SetActive(false);
    }   

    void SetBoardInteractable(bool toggle)
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].GetComponentInParent<Button>().interactable = toggle;
        }
    }

    void SetPlayerButtons(bool toggle)
    {
        playerX.button.interactable = toggle;
        playerO.button.interactable = toggle;
    }

    private int GetPlayerValue(string player)
    {
        return (player == "X") ? 1 : 2;
    }
}
