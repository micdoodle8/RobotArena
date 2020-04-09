using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Leap.Unity.Interaction;

public class FacingCamera : MonoBehaviour
{
    private Camera camera;
    public Transform target;
    [Range(-1.0F, 1.0F)]
    public float minDot = 0.8F;
    private bool lastFacing;
    public UnityEvent OnBeginFacingCamera;
    public UnityEvent OnEndFacingCamera;
    public bool isForcedOff = true;
    private InteractionController leftController;
    private bool lastControllerTracked = false;

    // Start is called before the first frame update
    void Start() {
        camera = Camera.main;
        lastFacing = isFacingCam();
        InteractionManager manager = InteractionManager.instance;
        HashSet<InteractionController>.Enumerator e = manager.interactionControllers.GetEnumerator();
        while (e.MoveNext()) {
            InteractionController controller = e.Current;
            if (controller.isLeft) {
                leftController = controller;
            }
        }
    }

    // Update is called once per frame
    void Update() {
        bool tracked = leftController.isTracked;
        if (tracked) {
            bool facing = isFacingCam();
            if (facing != lastFacing) {
                if (facing) {
                    OnBeginFacingCamera.Invoke();
                } else {
                    OnEndFacingCamera.Invoke();
                }
            }
            lastFacing = facing;
        } else {
            if (lastControllerTracked) {
                OnEndFacingCamera.Invoke();
            }
            lastFacing = false;
        }
        lastControllerTracked = tracked;
    }

    private bool isFacingCam() {
        if (isForcedOff) {
            return false;
        }
        Vector3 diff = camera.transform.position - target.transform.position;
        return Vector3.Dot(diff.normalized, target.forward) > minDot;
    }

    public void Disable(bool state) {
        isForcedOff = state;
    }
}
