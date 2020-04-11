using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class HoldPosition : MonoBehaviour
{
    private Rigidbody attachedRB;
    private Vector3 startPos;
    private InteractionBehaviour attachedIB;

    void Start() {
        attachedRB = GetComponent<Rigidbody>();
        startPos = transform.position;
        attachedIB = GetComponent<InteractionBehaviour>();
    }

    void FixedUpdate() {
        if (attachedIB.enabled) {
            attachedRB.isKinematic = true;
        } else {
            attachedRB.isKinematic = true;
        }
    }
}
