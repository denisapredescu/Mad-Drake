using UnityEngine;
using UnityEngine.UI;

public class TextCenterer : MonoBehaviour
{
    void Start()
    {
        // Find the "Health Score" GameObject
        Transform healthScore = transform.Find("Health Score");

        // Get the Text component of the "Health Score" GameObject
        Text text = healthScore.GetComponent<Text>();

        // Get the RectTransform component of the "Health Score" GameObject
        RectTransform textRect = healthScore.GetComponent<RectTransform>();

        // Set the anchor of the text to the center of the parent
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);

        // Set the pivot of the text to the center
        textRect.pivot = new Vector2(0.5f, 0.5f);

        // Set the position of the text to the center of the parent
        textRect.localPosition = textRect.rect.center;
    }
}
