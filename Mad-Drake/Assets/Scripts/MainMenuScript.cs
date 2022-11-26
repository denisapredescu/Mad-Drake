using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }
}
