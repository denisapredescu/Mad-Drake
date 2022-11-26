using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static bool GameRunning { get; private set; } = true;
 
    [SerializeField]
    private Canvas _menuCanvas;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && GameRunning)
        {
            Time.timeScale = 0;
            _menuCanvas.gameObject.SetActive(true);
            GameRunning = false;
        }

        // necessary to enable movement when a new game starts
        if (GameRunning)
        {
            Time.timeScale = 1;
        } 
    }

    public void OnReturnToGameButtonPressed()
    {
        _menuCanvas.gameObject.SetActive(false);
        GameRunning = true;
    }

    public void OnMainMenuButtonPressed()
    {
        GameRunning = true;
        SceneManager.LoadScene("MainMenuScene");
    }
}
