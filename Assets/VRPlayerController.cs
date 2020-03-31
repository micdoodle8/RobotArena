using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VRPlayerController : NetworkBehaviour
{
    private Transform vrTargetHead;
    private Transform vrTargetLeftHand;
    private Transform vrTargetRightHand;
    private GameObject cameraRig;
    [SyncVar]
    public GameObject teamContainer;
    public Transform hipsTransform;
    public float legLength = 0.94F;
    public bool onGround = true;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetPlayerControlled() {

        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in otherPlayers) {
            p.GetComponent<VRPlayerController>().SetPlayerNotControlled();
        }

        VRRig rigComp = GetComponent<VRRig>();
        // if (isClient) {
            vrTargetHead = GameObject.Find("CenterEyeAnchor").transform;
            vrTargetLeftHand = GameObject.Find("LeftHandAnchor").transform;
            vrTargetRightHand = GameObject.Find("RightHandAnchor").transform;
            cameraRig = GameObject.Find("OVRCameraRig");

            GetComponent<VRRig>().enabled = true;
            //sdkManager.SetActive(true);
            cameraRig.transform.position = rigComp.head.rigTarget.position;
            cameraRig.transform.rotation = transform.rotation;
            rigComp.head.vrTarget = vrTargetHead;
            rigComp.leftHand.vrTarget = vrTargetLeftHand;
            rigComp.rightHand.vrTarget = vrTargetRightHand;
        // }
    }

    public void SetPlayerNotControlled() {
        GetComponent<VRRig>().enabled = false;
    }

    private float yVel = 0.0F;

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        Ray ray = new Ray(hipsTransform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, legLength)) {
            onGround = true;
        } else {
            onGround = false;
        }
        ray = new Ray(hipsTransform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, legLength - 0.2F)) {
            Vector3 diff = hipsTransform.position - hit.point;
            float springForce = (4.0F / diff.magnitude);
            if (yVel > 0.0F) {
                springForce /= 3.0F;
            }
            yVel += springForce * Time.deltaTime;
        } else if (!onGround) {
            yVel -= 0.981F * Time.deltaTime;
        } else {
            yVel = 0.0F;
        }
        Transform targetTransform = transform;
        if (vrTargetHead != null) { 
            targetTransform = vrTargetHead;
        }
        targetTransform.position += new Vector3(0.0F, yVel, 0.0F);
    }
}
