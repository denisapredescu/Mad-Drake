using TMPro;
using UnityEngine;

public class TextCenterer : MonoBehaviour
{
    void Start()
    {
        TextMeshProUGUI childTextComponent = GetComponentInChildren<TextMeshProUGUI>();
        Transform score = childTextComponent.transform;
        RectTransform textRect = score.GetComponent<RectTransform>();

        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);

        textRect.pivot = new Vector2(0.5f, 0.5f);

        textRect.localPosition = textRect.rect.center;
    }
}
