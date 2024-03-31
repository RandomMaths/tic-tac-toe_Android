using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadLocal()
    {
        SceneManager.LoadScene("Local");
    }
    
    public void LoadOnline()
    {
        Debug.Log("Implement Networking");
        //SceneManager.LoadScene("Local");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
