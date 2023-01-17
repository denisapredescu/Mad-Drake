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
        SceneManager.LoadScene("L1");
    }

    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

    public void OnShowHighscoreButtonPressed()
    {
        _mainMenu.gameObject.SetActive(false);
        _highscore.gameObject.SetActive(true);
    }

    public void OnGoBackToMainMenuButtonPressed()
    {
        _mainMenu.gameObject.SetActive(true);
        _highscore.gameObject.SetActive(false);
    }


}
