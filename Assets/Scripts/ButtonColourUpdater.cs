#pragma warning disable 0618
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

public class ButtonColourUpdater : MonoBehaviour
{
    private InteractionBehaviour theButton;
    private Renderer buttonRenderer;
    public Color defaultColor = new Color(0.1F, 0.1F, 0.1F, 1.0F);
    public Color hoverColor = new Color(0.7F, 0.7F, 0.7F, 1.0F);
    public Color primaryHoverColor = new Color(0.8F, 0.8F, 0.8F, 1.0F);
    public bool usePrimaryHover = true;
    public bool useHover = false;
    public Color pressedColor = Color.white;
    public int buttonIndex = -1;

    void Start() {
        theButton = GetComponent<InteractionBehaviour>();
        buttonRenderer = GetComponentInChildren<Renderer>();
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

        float pressedTime = 0.0F;
        Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
        foreach (Object o in connectedPlayers) {
            pressedTime = ((ConnectedPlayerManager) o).ButtonPressed(buttonIndex);
            if (pressedTime > 0.0F) {
                break;
            }
        }

        if (pressedTime > 0.0F) {
            targetColor = pressedColor * pressedTime;
        }

        buttonRenderer.material.color = Color.Lerp(buttonRenderer.material.color, targetColor, 30F * Time.deltaTime);
    }
}
