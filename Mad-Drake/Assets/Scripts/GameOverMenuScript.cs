using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuScript : MonoBehaviour
{

    [SerializeField]
    private Canvas _gameOverCanvas;  // when the player is dead, the game is over

    public void Update()
    {
        if (HUDController.GetPlayerDeadStatus())
        {
            Time.timeScale = 0;
            _gameOverCanvas.gameObject.SetActive(true);
        }       
    }
    public void OnRestartButtonPressed()
    {
        SceneManager.LoadScene("L1");
    }

    public void OnMainMenuButtonPressed()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}
