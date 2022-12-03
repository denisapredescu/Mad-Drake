using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetMaxBullets(int bullets)
    {
        slider.maxValue = bullets;
        slider.value = bullets;

        fill.color = gradient.Evaluate(1f);
    }

    public void SetBullets(int bullets)
    {
        slider.value = bullets;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }
}
