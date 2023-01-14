using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using static EndGameScript;

public class ShowHighscoreScript : MonoBehaviour
{
    public TextMeshProUGUI name1;
    public TextMeshProUGUI score1;
    public TextMeshProUGUI name2;
    public TextMeshProUGUI score2;
    public TextMeshProUGUI name3;
    public TextMeshProUGUI score3;
    public TextMeshProUGUI name4;
    public TextMeshProUGUI score4;
    public TextMeshProUGUI name5;
    public TextMeshProUGUI score5;

    public string saveFilePath => Application.persistentDataPath + "/highscore.json";

    void Start()
    {

        var pullData = File.ReadAllText(saveFilePath);
        var highscore = JsonUtility.FromJson<Highscore>(pullData);


        if (highscore.players.Count >= 1)
        {
            name1.text = highscore.players[0].name;
            score1.text = highscore.players[0].score.ToString();
        }
        else
        {
            name1.text = "";
            score1.text = "";
        }

        if (highscore.players.Count >= 2)
        {
            name2.text = highscore.players[1].name;
            score2.text = highscore.players[1].score.ToString();
        }
        else
        {
            name2.text = "";
            score2.text = "";
        }

        if (highscore.players.Count >= 3)
        {
            name3.text = highscore.players[2].name;
            score3.text = highscore.players[2].score.ToString();
        }
        else
        {
            name3.text = "";
            score3.text = "";
        }

        if (highscore.players.Count >= 4)
        {
            name4.text = highscore.players[3].name;
            score4.text = highscore.players[3].score.ToString();
        }
        else
        {
            name4.text = "";
            score4.text = "";
        }

        if (highscore.players.Count == 5)
        {
            name5.text = highscore.players[4].name;
            score5.text = highscore.players[4].score.ToString();
        }
        else
        {
            name5.text = "";
            score5.text = "";
        }
    }
}

