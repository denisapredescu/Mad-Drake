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


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && nrOfBombs > 0)
        {
            GameObject bomb = Instantiate(bombPrefab, 
                new Vector3(transform.position.x, 
                            transform.position.y, 
                            transform.position.z), 
                Quaternion.identity);

            ParticleSystem bombExplosionPS = bomb.GetComponent<ParticleSystem>();
            nrOfBombs--;
            StartCoroutine(ExplodeBomb(bomb, bombExplosionPS));
        }
    }

    private IEnumerator ExplodeBomb(GameObject bomb, ParticleSystem bombExplosionPS)
    {
        yield return new WaitForSeconds(timeUntilExplosion);
        bomb.GetComponent<SpriteRenderer>().enabled = false;
        // Note: in particle system component Stop Action is set to Destroy, so the object will be destroyed after 
        // the effect is completed
        bombExplosionPS.Play(); 
    }
}
