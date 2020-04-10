#pragma warning disable 0618
using UnityEngine;

public class HookSwing : MonoBehaviour
{
    [Range(0.0F, 1.0F)]
    public float lerpSpeed = 0.3F;
    private Vector3 lastPos;

    // Start is called before the first frame update
    void Start()
    {
        lastPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 diff = (transform.position - lastPos) / Time.deltaTime;
        Vector3 eulers = transform.rotation.eulerAngles;
        float targetYaw = diff.magnitude > 0.00001F ? Mathf.Rad2Deg * Mathf.Atan2(diff.x, diff.z) : eulers.y;
        float targetPitch = diff.magnitude > 0.00001F ? diff.magnitude * 5.0F : 0.0F;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(targetPitch, targetYaw, eulers.z)), lerpSpeed);
        lastPos = transform.position;
    }
}
