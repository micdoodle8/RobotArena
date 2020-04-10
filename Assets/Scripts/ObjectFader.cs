#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFader : MonoBehaviour
{
    private MeshRenderer targetRenderer;
    private float fadeTime = 0.0F;
    private float startFadeTime = 1.0F;
    public AnimationCurve curveX = AnimationCurve.Linear(0, 1, 1, 0);
    public AnimationCurve curveY = AnimationCurve.Linear(0, 1, 1, 0);
    public AnimationCurve curveZ = AnimationCurve.Linear(0, 1, 1, 0);
    public Vector3 scaleMultiplier = new Vector3(1.0F, 1.0F, 1.0F);
    public bool fadeOut = true;
    public bool disappearAtEnd = false;

    // Start is called before the first frame update
    void Start() {
        targetRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update() {
        if (fadeTime > 0.0F) {
            fadeTime -= Time.deltaTime;
            if (fadeTime < 0.0F) {
                fadeTime = 0.0F;
                if (disappearAtEnd) {
                    targetRenderer.enabled = false;
                }
            }
            float val = fadeOut ? 1.0F - fadeTime / startFadeTime : fadeTime / startFadeTime;
            transform.localScale = new Vector3(curveX.Evaluate(val) * scaleMultiplier.x, curveY.Evaluate(val) * scaleMultiplier.y, curveZ.Evaluate(val) * scaleMultiplier.z);
            // renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, fadeTime / startFadeTime);
        }
    }

    public void StartFade(float seconds) {
        targetRenderer.enabled = true;
        startFadeTime = seconds;
        fadeTime = seconds;
    }

    public void ForceDisable() {
        targetRenderer.enabled = false;
    }
}
