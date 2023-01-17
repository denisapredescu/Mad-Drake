using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.IO;
using System.Linq;

public class EndGameScript : MonoBehaviour
{
    public string SaveFilePath => Application.persistentDataPath + "/highscore.json";
    private string newName;
    private float newScore = 0.0f;
    private static TextMeshProUGUI scoreGUI;
    public GameObject scoreCard;
    private bool isTaken = false;

    [Serializable]
    public struct PlayerInfo
    {
        public string name;
        public float score;

        public PlayerInfo(string name, float score)
        {
            this.name = name;
            this.score = score;
        }
    }

    [System.Serializable]
    public struct Highscore
    {
        public List<PlayerInfo> Players;
    }


    [SerializeField]
    private Canvas _endGameCanvas;
    [SerializeField]
    private TMP_InputField _inputField;

    private Highscore highscore;

    private void Start()
    {
        _inputField.onEndEdit.AddListener(ReadInput);
        scoreGUI = scoreCard.GetComponent<TextMeshProUGUI>();
        scoreGUI.text = $"Your score: {newScore}";
    }

    private void Update()
    {
        if (GoToNextLevel.isEnded && !isTaken) // s-a terminat jocul
        {
            isTaken = true;
            if (SceneManager.GetActiveScene().name.Equals("BossLevel")) // aici va trebui modificat cu ultimul nivel
            {
                Time.timeScale = 0;
                _endGameCanvas.gameObject.SetActive(true);

                float gold = (float)HUDController.getGold();
                float lives = (float)HUDController.getLives();
                float damageTaken = (float)HUDController.getDamageTaken();
                newScore = ((gold + lives) + 1) / (damageTaken + 1) * 100;
                newScore = (float)(Mathf.Round(newScore * 100) / 100.0);
                scoreGUI.text = $"Your score: {newScore}";

                string _highscore;
                if (!File.Exists(SaveFilePath))
                {
                    highscore.Players = new List<PlayerInfo> { new PlayerInfo("Player#" + UnityEngine.Random.Range(1000, 10000), newScore) };
                    _highscore = JsonUtility.ToJson(highscore);
                    File.WriteAllText(SaveFilePath, _highscore);
                    return;
                }
                else
                {
                    _highscore = File.ReadAllText(SaveFilePath);
                    highscore = JsonUtility.FromJson<Highscore>(_highscore);

                    highscore.Players.Add(new PlayerInfo("Player#" + UnityEngine.Random.Range(1000, 10000), newScore));
                    highscore.Players = highscore.Players.OrderByDescending(x => x.score).ToList();

                    _highscore = JsonUtility.ToJson(highscore);
                    File.WriteAllText(SaveFilePath, _highscore);
                }
            }
        }
    }

    public void OnMainMenuButtonPressed()
    {
        _endGameCanvas.gameObject.SetActive(false);
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ReadInput(string input)
    {
        newName = input;
        _inputField.interactable = false;
    }
}
