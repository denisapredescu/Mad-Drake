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
            Debug.Log("bv ai apasAT PE E");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
