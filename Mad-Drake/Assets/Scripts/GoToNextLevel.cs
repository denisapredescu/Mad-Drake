using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToNextLevel : MonoBehaviour
{
    [SerializeField]
    private GameObject textInfoNextLevel;
    private bool inTrigger;

    public static bool isEnded = false;

    private void Start()
    {
        inTrigger = false;
        isEnded = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && inTrigger)
        {

            if (SceneManager.GetActiveScene().name.Equals("BossLevel")) // aici va trebui modificat cu ultimul nivel
            {

                isEnded = true;
            }
            else 
            {
                Scene scene = SceneManager.GetActiveScene();
                switch(scene.name)
                {
                    case "L1": SceneManager.LoadScene("L2"); break;
                    case "L2": SceneManager.LoadScene("L3"); break;
                    case "L3": SceneManager.LoadScene("BossLevel"); break;
                    default: break;
                }
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
