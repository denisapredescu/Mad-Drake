using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> breakableParts;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    public int nrOfBulletsToDestroy = 3;
    private int bulletCounter;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        bulletCounter = 0;
        for(int i = 0; i < breakableParts.Count; i++)
        {
            breakableParts[i].GetComponent<Rigidbody2D>().simulated = false;
        }
    }

    private void BreakObject()
    {
        for (int i = 0; i < breakableParts.Count; i++)
        {
            breakableParts[i].GetComponent<Rigidbody2D>().simulated = true;
            spriteRenderer.enabled = false;
            boxCollider2D.enabled = false;
        }
    }

    public void HitBox()
    {
        Debug.Log("bang");
        bulletCounter++;
        if (bulletCounter == nrOfBulletsToDestroy)
        {
            BreakObject();
            Debug.Log("puff");
        }
    }

}
