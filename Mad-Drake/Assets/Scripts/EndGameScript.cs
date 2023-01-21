using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.IO;

public class EndGameScript : MonoBehaviour
{
    public string saveFilePath => Application.persistentDataPath + "/highscore.json";
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
            // cod folosit doar la finalul jocului cand se ajunge la nivelul X
            Debug.Log(SceneManager.GetActiveScene().name);
            if (SceneManager.GetActiveScene().name.Equals("BossLevel")) // aici va trebui modificat cu ultimul nivel
            {
                Debug.Log("test");
                Time.timeScale = 0;
                _endGameCanvas.gameObject.SetActive(true);

                float gold = (float)HUDController.getGold();
                float lives = (float)HUDController.getLives();
                float damageTaken = (float)HUDController.getDamageTaken();
                newScore = ((gold + lives) + 1) / (damageTaken + 1) * 100;
                newScore = (float)(Mathf.Round(newScore * 100) / 100.0);
                scoreGUI.text = $"Your score: {newScore}";

                if (!File.Exists(saveFilePath))
                    return;

                var pullData = File.ReadAllText(saveFilePath);
                highscore = JsonUtility.FromJson<Highscore>(pullData);

                if (highscore.Players.Count == 0)
                {
                    _inputField.gameObject.SetActive(true);
                }
                else
                {
                    var index = highscore.Players.Count - 1;

                    while (index != -1 && highscore.Players[index].score < newScore)
                        index--;

                    if (index != highscore.Players.Count - 1)
                       _inputField.gameObject.SetActive(true);  
                }
            }
        }
    }

    public void OnMainMenuButtonPressed()
    {
        Debug.Log("merge");
        _endGameCanvas.gameObject.SetActive(false);
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ReadInput(string input)
    {
        newName = input;
        _inputField.interactable = false;

        PlayerInfo newPlayer = new PlayerInfo();
        newPlayer.name = newName;
        newPlayer.score = newScore;

        if (highscore.Players.Count == 0)
        {
            highscore.Players.Add(newPlayer);
            var pushData = JsonUtility.ToJson(highscore);
            File.WriteAllText(saveFilePath, pushData);
        }
        else
        {
            var index = highscore.Players.Count - 1;

            while (index != -1 && highscore.Players[index].score < newScore)
                index--;

            if (highscore.Players.Count != 5)
                highscore.Players.Add(highscore.Players[highscore.Players.Count - 1]);

            for (var j = highscore.Players.Count - 1; j > index + 1; j--)
                highscore.Players[j] = highscore.Players[j - 1];


            if (index != highscore.Players.Count - 1)
                highscore.Players[index + 1] = newPlayer;

            var pushData = JsonUtility.ToJson(highscore);
            File.WriteAllText(saveFilePath, pushData);
        }
    }
}