using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.IO;

public class GoToNextLevel : MonoBehaviour
{
    [SerializeField]
    private GameObject textInfoNextLevel;
    private bool inTrigger;
    [SerializeField]
    private string nextSceneName;

    public static bool isEnded = false;

    private void Start()
    {
        textInfoNextLevel = GameObject.Find("InfoNextLevel");
        textInfoNextLevel.SetActive(false);
        inTrigger = false;
        isEnded = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && inTrigger)
        {

            if (SceneManager.GetActiveScene().name.Equals("SampleScene")) // aici va trebui modificat cu ultimul nivel
                isEnded = true;
            else 
            {
                //SceneManager.LoadScene(nextSceneName);    // nu mai functioneaza trecerea la nivel 2 pentru ca hudul nu mai se potriveste
                Start();
            }
            
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Player")
        {
            textInfoNextLevel.SetActive(true);
            inTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Player")
        {
            textInfoNextLevel.SetActive(false);
            inTrigger = false;
        }
    }
}
