using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintTextFader : MonoBehaviour
{
    private Text hintText;
    private float fadeTime = 0.0F;
    private float startFadeTime = 1.0F;

    // Start is called before the first frame update
    void Start() {
        hintText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        if (fadeTime > 0.0F) {
            fadeTime -= Time.deltaTime;
            if (fadeTime < 0.0F) {
                fadeTime = 0.0F;
                hintText.enabled = false;
            }
            hintText.color = new Color(hintText.color.r, hintText.color.g, hintText.color.b, fadeTime / startFadeTime);
        }
    }

    public void StartFade(float seconds) {
        hintText.enabled = true;
        startFadeTime = seconds;
        fadeTime = seconds;
    }
}
