using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] GameObject Background;
    [SerializeField] GameObject Logo;
    [SerializeField] GameObject Window;
    float backgroundAlpha = 1f;
    Image BackgroundImg;
    Color BackgroundColor;

    private void Start()
    {
        BackgroundImg = Background.GetComponent<Image>();
        BackgroundColor = BackgroundImg.color;
    }

    public void CloseMenu()
    {
        Logo.SetActive(false);
        Window.SetActive(false);
        if (Background.activeInHierarchy)
        {
            StartCoroutine("FadeOutBackground");
        }
    }

    private IEnumerator FadeOutBackground()
    {
        while (backgroundAlpha > 0)
        {
            backgroundAlpha -= 0.75f * Time.deltaTime;
            BackgroundColor.a = backgroundAlpha;
            BackgroundImg.color = BackgroundColor;
            yield return null;
        }
        Background.SetActive(false);
        yield break;
    }
}