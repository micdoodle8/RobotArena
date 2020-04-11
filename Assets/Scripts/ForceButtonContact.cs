#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

public class ForceButtonContact : MonoBehaviour
{
    void Start() {
        
    }

    void Update() {
        
    }

    private void ModifyChildrenRecursive(Transform transform, bool state) {
        InteractionBehaviour behaviour = transform.gameObject.GetComponent<InteractionBehaviour>();
        if (behaviour != null) {
            behaviour.enabled = state;
        } else {
            for (int i = 0; i < transform.childCount; ++i) {
                ModifyChildrenRecursive(transform.GetChild(i), state);
            }
        }
    }

    public void OnAnimForwardFinished() {
        ModifyChildrenRecursive(transform, true);
    }

    public void OnAnimBackwardStart() {
        ModifyChildrenRecursive(transform, false);
    }
}
