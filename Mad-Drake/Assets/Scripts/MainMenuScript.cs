using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField]
    private Canvas _mainMenu;
    [SerializeField]
    private Canvas _highscore;

    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

    public void OnShowHighscoreButtonPressed()
    {
        Debug.Log("highscore");
        _mainMenu.gameObject.SetActive(false);
        _highscore.gameObject.SetActive(true);
    }

    public void OnGoBackToMainMenuButtonPressed()
    {
        Debug.Log("go back");
        _mainMenu.gameObject.SetActive(true);
        _highscore.gameObject.SetActive(false);
    }


}
