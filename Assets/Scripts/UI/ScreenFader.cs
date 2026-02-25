using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeSpeed = 2f;

    public IEnumerator FadeOut()
    {
        while (fadeImage.color.a < 1f)
        {
            Color c = fadeImage.color;
            c.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }
    }

    public IEnumerator FadeIn()
    {
        while (fadeImage.color.a > 0f)
        {
            Color c = fadeImage.color;
            c.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = c;
            yield return null;
        }
    }
}