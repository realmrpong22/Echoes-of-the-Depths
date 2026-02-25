using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float flashSpeed = 8f;
    [SerializeField] private float maxAlpha = 0.6f;

    public IEnumerator Flash()
    {
        // Fade in quickly
        while (flashImage.color.a < maxAlpha)
        {
            Color c = flashImage.color;
            c.a += Time.deltaTime * flashSpeed;
            flashImage.color = c;
            yield return null;
        }

        // Fade out quickly
        while (flashImage.color.a > 0f)
        {
            Color c = flashImage.color;
            c.a -= Time.deltaTime * flashSpeed;
            flashImage.color = c;
            yield return null;
        }
    }
}