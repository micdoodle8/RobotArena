using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        lastFacing = isFacingCam();
    }

    // Update is called once per frame
    void Update()
    {
        bool facing = isFacingCam();
        if (facing != lastFacing) {
            if (facing) {
                OnBeginFacingCamera.Invoke();
            } else {
                OnEndFacingCamera.Invoke();
            }
        }
        lastFacing = facing;
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
