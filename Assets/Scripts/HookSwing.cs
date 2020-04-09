using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookSwing : MonoBehaviour
{
    [Range(0.0F, 1.0F)]
    public float lerpSpeed = 0.3F;
    private Vector3 lastPos;
    private Quaternion startRot;

    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
        startRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 diff = (transform.position - lastPos) / Time.deltaTime;
        Vector3 eulers = transform.rotation.eulerAngles;
        // while (eulers.y < 0) eulers.y += 360; 
        float targetYaw = diff.magnitude > 0.01F ? Mathf.Rad2Deg * Mathf.Atan2(diff.x, diff.z) : eulers.y;
        // while (targetYaw < 0) targetYaw += 360; 
        // float actualYaw = Mathf.Lerp(eulers.y, targetYaw, lerpSpeed);
        float targetPitch = diff.magnitude > 0.01F ? diff.magnitude * 5.0F : 0.0F;
        // float actualPitch = Mathf.Lerp(eulers.x, targetPitch, lerpSpeed);
        // // eulers.y = actualYaw;
        // // eulers.x = actualPitch;
        // // transform.rotation = startRot;
        // // transform.rotation = Quaternion.Euler(eulers);
        // transform.rotation = Quaternion.Euler(new Vector3(actualPitch, actualYaw, eulers.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(targetPitch, targetYaw, eulers.z)), lerpSpeed);
        lastPos = transform.position;
    }
}
