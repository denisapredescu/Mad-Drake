using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLevelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private new GameObject camera;
    [SerializeField]
    private Transform fireballSpawn;
    [SerializeField]
    private GameObject fireballPrefab;
    [SerializeField]
    private float delayBetweenFireballs;
    [SerializeField]
    private float delayBetweenFireballsSets;
    [SerializeField]
    private int fireballsInOneSet;

    private int fireballsInOneSetCopy;

    [SerializeField]
    private Animator bossRevealCameraAnimator;
    [SerializeField]
    private Animator bossHealthShowAnimator;

    private bool bossRevealed;



    private void Start()
    {
        bossRevealed = false;
        fireballsInOneSetCopy = fireballsInOneSet;
    }

    private void Update()
    {
        if(!bossRevealed)
        {
            BossReveal();
        }
    }
    private void BossReveal()
    {
        if (player.transform.position.x > 9.2f)
        {
            camera.transform.position = new Vector3(camera.transform.position.x + 18, camera.transform.position.y, camera.transform.position.z);
            bossRevealCameraAnimator.Play("camera_reveal_boss");
            bossHealthShowAnimator.Play("boss_health_show");
            bossRevealed = true;
            StartCoroutine(WaitBetweenFireballSets());
        }

    }

    private IEnumerator WaitBetweenFireballSets()
    {
        yield return new WaitForSeconds(delayBetweenFireballsSets);
        fireballsInOneSet = fireballsInOneSetCopy;
        StartCoroutine(ThrowFireballs());
    }

    private IEnumerator ThrowFireballs()
    {
        yield return new WaitForSeconds(delayBetweenFireballs);
        Instantiate(fireballPrefab, new Vector3(fireballSpawn.position.x, fireballSpawn.position.y, fireballSpawn.position.z), Quaternion.identity);
        if (fireballsInOneSet > 0)
        {
            fireballsInOneSet--;
            StartCoroutine(ThrowFireballs());
        }
        else
        {
            StartCoroutine(WaitBetweenFireballSets());
        }
    }
}
