#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum FadeStatus {
    NONE,
    TO_BLACK,
    TO_CLEAR
}

public class VRFader : MonoBehaviour
{
    private FadeStatus status;
    private float progress = 0.0F;
    private float totalTime = 0.0F;
    private MeshRenderer overlayRenderer;
    public delegate void FadeCompleteCallback(int param);
    private FadeCompleteCallback callback;
    private int callbackParam;
    [Range(0.0F, 1.0F)]
    public float startAlpha = 0.0F;

    // Start is called before the first frame update
    void Start()
    {
        overlayRenderer = gameObject.GetComponent<MeshRenderer>();
        overlayRenderer.material.color = new Color(0.0F, 0.0F, 0.0F, startAlpha);
    }

    // Update is called once per frame
    void Update()
    {
        if (status != FadeStatus.NONE) {
            progress -= Time.deltaTime;
            if (progress <= 0.0F) {
                if (status == FadeStatus.TO_BLACK) {
                    overlayRenderer.material.color = new Color(0.0F, 0.0F, 0.0F, 1.0F);
                } else if (status == FadeStatus.TO_CLEAR) {
                    overlayRenderer.enabled = false;
                    overlayRenderer.material.color = new Color(0.0F, 0.0F, 0.0F, 0.0F);
                }
                status = FadeStatus.NONE;
                if (callback != null) {
                    callback(callbackParam);
                }
            } else {
                if (status == FadeStatus.TO_BLACK) {
                    overlayRenderer.material.color = new Color(0.0F, 0.0F, 0.0F, 1.0F - progress / totalTime);
                } else if (status == FadeStatus.TO_CLEAR) {
                    overlayRenderer.material.color = new Color(0.0F, 0.0F, 0.0F, progress / totalTime);
                }
            }
        }
    }

    public void FadeToBlack(float time, FadeCompleteCallback callback, int param) {
        overlayRenderer.enabled = true;
        status = FadeStatus.TO_BLACK;
        progress = time;
        totalTime = time;
        this.callback = callback;
        callbackParam = param;
    }
    
    public void FadeBackIn(float time, FadeCompleteCallback callback, int param) {
        status = FadeStatus.TO_CLEAR;
        progress = time;
        totalTime = time;
        this.callback = callback;
        callbackParam = param;
    }
}
