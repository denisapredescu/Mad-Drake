using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GoToNextLevel : MonoBehaviour
{

    [SerializeField]
    private GameObject textInfoNextLevel;
    private bool inTrigger;
    [SerializeField]
    private string nextSceneName;


    private void Start()
    {
        textInfoNextLevel = GameObject.Find("InfoNextLevel");
        textInfoNextLevel.SetActive(false);
        inTrigger = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && inTrigger)
        {
            SceneManager.LoadScene(nextSceneName);
            Start();
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
