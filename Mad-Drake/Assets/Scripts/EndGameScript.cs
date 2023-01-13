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

    [Serializable]
    public struct Highscore
    {
        public List<PlayerInfo> players;
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
            if (SceneManager.GetActiveScene().name.Equals("SampleScene")) // aici va trebui modificat cu ultimul nivel
            {
                Time.timeScale = 0;
                _endGameCanvas.gameObject.SetActive(true);

                float gold = (float)HUDController.getGold();
                float lives = (float)HUDController.getLives();
                float damageTaken = (float)HUDController.getDamageTaken();
                newScore = gold / ((lives + 1) * damageTaken);
                newScore = (float)(Mathf.Round(newScore * 100) / 100.0);
                scoreGUI.text = $"Your score: {newScore}";

                if (!File.Exists(saveFilePath))
                    return;

                var pullData = File.ReadAllText(saveFilePath);
                highscore = JsonUtility.FromJson<Highscore>(pullData);

                if (highscore.players.Count == 0)
                {
                    _inputField.gameObject.SetActive(true);
                }
                else
                {
                    var index = highscore.players.Count - 1;

                    while (index != -1 && highscore.players[index].score < newScore)
                        index--;

                    if (index != highscore.players.Count - 1)
                       _inputField.gameObject.SetActive(true);  
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

        PlayerInfo newPlayer = new PlayerInfo();
        newPlayer.name = newName;
        newPlayer.score = newScore;

        if (highscore.players.Count == 0)
        {
            highscore.players.Add(newPlayer);
            var pushData = JsonUtility.ToJson(highscore);
            File.WriteAllText(saveFilePath, pushData);
        }
        else
        {
            var index = highscore.players.Count - 1;

            while (index != -1 && highscore.players[index].score < newScore)
                index--;

            if (highscore.players.Count != 5)
                highscore.players.Add(highscore.players[highscore.players.Count - 1]);

            for (var j = highscore.players.Count - 1; j > index + 1; j--)
                highscore.players[j] = highscore.players[j - 1];


            if (index != highscore.players.Count - 1)
                highscore.players[index + 1] = newPlayer;

            var pushData = JsonUtility.ToJson(highscore);
            File.WriteAllText(saveFilePath, pushData);
        }
    }
}
