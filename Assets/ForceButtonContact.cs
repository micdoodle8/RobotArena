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

    private void DisableChildren(Transform transform, bool state) {
        InteractionBehaviour behaviour = transform.gameObject.GetComponent<InteractionBehaviour>();
        if (behaviour != null) {
            behaviour.enabled = state;
        } else {
            for (int i = 0; i < transform.childCount; ++i) {
                DisableChildren(transform.GetChild(i), state);
            }
        }
    }

    public void OnAnimForwardFinished() {
        DisableChildren(transform, true);
    }

    public void OnAnimBackwardStart() {
        DisableChildren(transform, false);
    }
}
