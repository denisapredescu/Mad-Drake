using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLevelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject boss;
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
    [SerializeField]
    private Animator bossRevealCameraAnimator;
    [SerializeField]
    private Animator bossHealthShowAnimator;
    [SerializeField]
    private Transform bossPhase1Position;
    [SerializeField]
    private Transform bossPhase2Position;
    [SerializeField]
    private float bossMoveSpeed;
    [SerializeField]
    private ParticleSystem rain1;
    [SerializeField]
    private ParticleSystem rain2;
    [SerializeField]
    private ParticleSystem rain3;
    [SerializeField]
    private GameObject warning1;
    [SerializeField]
    private GameObject warning2;
    [SerializeField]
    private GameObject warning3;
    [SerializeField]
    private float rainDuration;
    [SerializeField]
    private float delayBetweenRain;
    [SerializeField]
    private float rainWarningsDuration;

    private int fireballsInOneSetCopy;
    private bool bossRevealed;
    private int bossMaxHealth;
    private int bossCurrentHealth;

    private bool canGoToPhase2;
    private bool bossArrivedAtPhase2;
    private bool noFireRainChosen;
    private int noFireRainIndex;

    private void Start()
    {
        bossRevealed = false;
        canGoToPhase2 = false;
        bossArrivedAtPhase2 = false;
        noFireRainChosen = false;
        fireballsInOneSetCopy = fireballsInOneSet;
        rain1.Stop();
        rain2.Stop();
        rain3.Stop();
        warning1.SetActive(false);
        warning2.SetActive(false);
        warning3.SetActive(false);
        bossMaxHealth = boss.GetComponent<ShadowBehaviour>().GetHealth();
    }

    private void Update()
    {
        bossCurrentHealth = boss.GetComponent<ShadowBehaviour>().GetHealth();
        if (!bossRevealed)
        {
            BossReveal();
        }
        if(canGoToPhase2 && !bossArrivedAtPhase2)
        {
            if (boss.transform.position != bossPhase2Position.position)
            { 
                boss.transform.position = Vector2.MoveTowards(boss.transform.position, bossPhase2Position.position, bossMoveSpeed * Time.deltaTime); 
            }
            // just first time will enter in this else
            else
            {
                Debug.Log("Boss arrived at phase2");
                ChooseFireRainIndex();
                bossArrivedAtPhase2 = true;
            }
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
        if(boss.activeInHierarchy)
        {
            if(bossCurrentHealth > bossMaxHealth / 2)
            {
                StartCoroutine(ThrowFireballs());
            }
            else
            {
                canGoToPhase2 = true;
            }
            
        }
        
    }

    private IEnumerator ThrowFireballs()
    {
        yield return new WaitForSeconds(delayBetweenFireballs);
        Instantiate(fireballPrefab, new Vector3(fireballSpawn.position.x, fireballSpawn.position.y, fireballSpawn.position.z), Quaternion.identity);
        if(boss.activeInHierarchy)
        {
            if(bossCurrentHealth > bossMaxHealth / 2)
            {
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
            else
            {
                canGoToPhase2 = true;
            }
            
        }
    }
    private void ChooseFireRainIndex()
    {
        noFireRainIndex = Random.Range(1, 4);
        Debug.Log("Chosen fire rain index with no rain: " + noFireRainIndex);
        /*noFireRainChosen = true;*/
        StartCoroutine(ShowRainWarnings());

    }

    private IEnumerator ShowRainWarnings()
    {
        if (noFireRainIndex != 1)
            warning1.SetActive(true);
        if (noFireRainIndex != 2)
            warning2.SetActive(true);
        if (noFireRainIndex != 3)
            warning3.SetActive(true);
        yield return new WaitForSeconds(rainWarningsDuration);
        warning1.SetActive(false);
        warning2.SetActive(false);
        warning3.SetActive(false);
        StartCoroutine(StartRain());

    }

    // HERE STARTS/STOPS THE RAIN
    private IEnumerator StartRain()
    {
        if (noFireRainIndex != 1)
            rain1.Play();
        if (noFireRainIndex != 2)
            rain2.Play();
        if (noFireRainIndex != 3)
            rain3.Play();
        yield return new WaitForSeconds(rainDuration);
        rain1.Stop();
        rain2.Stop();
        rain3.Stop();
        StartCoroutine(WaitBetweenRain());
    }

    private IEnumerator WaitBetweenRain()
    {
        yield return new WaitForSeconds(delayBetweenRain);
        /*noFireRainChosen = false;*/
        ChooseFireRainIndex();
    }

}
