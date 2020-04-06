using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class ButtonColourUpdater : MonoBehaviour
{
    private InteractionBehaviour theButton;
    private Renderer renderer;
    public Color defaultColor = Color.Lerp(Color.black, Color.white, 0.1F);
    public Color suspendedColor = Color.red;
    public Color hoverColor = Color.Lerp(Color.black, Color.white, 0.7F);
    public Color primaryHoverColor = Color.Lerp(Color.black, Color.white, 0.8F);
    public bool usePrimaryHover = true;
    public bool useHover = false;
    public Color pressedColor = Color.white;

    void Start() {
        theButton = GetComponent<InteractionBehaviour>();
        renderer = GetComponentInChildren<Renderer>();
    }

    void Update() {
        Color targetColor = defaultColor;

        if (theButton.isPrimaryHovered && usePrimaryHover) {
            targetColor = primaryHoverColor;
        } else {
            if (theButton.isHovered && useHover) {
                float glow = theButton.closestHoveringControllerDistance.Map(0F, 0.2F, 1F, 0.0F);
                targetColor = Color.Lerp(defaultColor, hoverColor, glow);
            }
        }

        if (theButton.isSuspended) {
            targetColor = suspendedColor;
        }

        if (theButton is InteractionButton && (theButton as InteractionButton).isPressed) {
            targetColor = pressedColor;
        }

        renderer.material.color = Color.Lerp(renderer.material.color, targetColor, 30F * Time.deltaTime);
    }
}
