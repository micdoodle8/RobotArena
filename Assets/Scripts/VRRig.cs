#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap {
    public Transform vrTarget;
    public Transform rigTarget;
    public Transform maxDistSource;
    public float maxDist;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    public bool handAttached = false;

    public void Map() {
        Vector3 target = vrTarget.TransformPoint(trackingPositionOffset);
        if (maxDistSource != null) {
            Vector3 diff = target - maxDistSource.position;
            if (diff.magnitude > maxDist) {
                target = maxDistSource.position + diff.normalized * maxDist;
                handAttached = false;
            } else {
                handAttached = true;
            }
        }
        rigTarget.position = target;
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}

public class VRRig : MonoBehaviour
{
    public VRMap head;
    public VRMap leftHand;
    public VRMap rightHand;

    public Transform headConstraint;
    public Vector3 headBodyOffset;
    private SkinnedMeshRenderer meshRenderer;
    private bool lastHandsAttached;
    private float materialFade = 0.0F;
    private float materialFadeDir = 0.0F;
    public float fadeSpeed = 1.5F;

    // Start is called before the first frame update
    void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = headConstraint.position + headBodyOffset;
        transform.forward = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;

        head.Map();
        leftHand.Map();
        rightHand.Map();

        if (materialFadeDir != 0.0F) {
            materialFade += Time.deltaTime * materialFadeDir * fadeSpeed;
            materialFade = Mathf.Clamp(materialFade, 0.0F, 1.0F);
            meshRenderer.material.color = new Color(meshRenderer.material.color.r, meshRenderer.material.color.g, meshRenderer.material.color.b, materialFade);
            if (materialFade <= 0.0F || materialFade >= 1.0F) {
                materialFadeDir = 0.0F;
            }
        }

        bool handsAttached = leftHand.handAttached && rightHand.handAttached;
        if (handsAttached && !lastHandsAttached) {
            materialFadeDir = 1.0F;
            materialFade = 0.0F;
        } else if (!handsAttached && lastHandsAttached) {
            materialFadeDir = -1.0F;
            materialFade = 1.0F;
        }
        lastHandsAttached = handsAttached;
    }

    public void ForceOpaque() {
        meshRenderer.material.color = new Color(meshRenderer.material.color.r, meshRenderer.material.color.g, meshRenderer.material.color.b, 1.0F);
        materialFade = materialFadeDir = 0.0F;
    }
}
