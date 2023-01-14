using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [SerializeField]
    private GameObject bombPrefab;
    [SerializeField]
    private float timeUntilExplosion;
    [SerializeField]
    private int nrOfBombs;
    [SerializeField]
    private AudioSource bombExplosionAudio;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && nrOfBombs > 0)
        {
            GameObject bomb = Instantiate(bombPrefab, 
                new Vector3(transform.position.x, 
                            transform.position.y, 
                            transform.position.z), 
                Quaternion.identity);

            bomb.GetComponent<CircleCollider2D>().enabled = false;
            ParticleSystem bombExplosionPS = bomb.GetComponent<ParticleSystem>();
            nrOfBombs--;
            HUDController.ReleaseBomb();
            
            StartCoroutine(ExplodeBomb(bomb, bombExplosionPS));
        }

        
    }

    private IEnumerator ExplodeBomb(GameObject bomb, ParticleSystem bombExplosionPS)
    {
        yield return new WaitForSeconds(timeUntilExplosion);
        bomb.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        bomb.transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
        // Note: in particle system component Stop Action is set to Destroy, so the object will be destroyed after 
        // the effect is completed
        bombExplosionAudio.Play();
        bombExplosionPS.Play();
        // activate collider to call the OnTrigger event inside the bomb game object
        bomb.GetComponent<CircleCollider2D>().enabled = true;
    }

}
