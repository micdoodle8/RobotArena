using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Leap.Unity;

public class VRPlayerController : NetworkBehaviour
{
    private Transform vrTargetHead;
    private Transform vrTargetLeftHand;
    private Transform vrTargetRightHand;
    private GameObject cameraRig;
    [SyncVar]
    public GameObject teamContainer;
    public Transform hipsTransformWithHands;
    public Transform hipsTransformWithoutHands;
    private Transform hipsTransform;
    public float legLength = 0.94F;
    public bool onGround = true;
    private GameObject scene;
    public GameObject withHands;
    public GameObject withoutHands;

    // Start is called before the first frame update
    void Start()
    {
        scene = GameObject.Find("Scene");
        hipsTransform = hipsTransformWithHands;
    }

    public void SetPlayerControlled() {

        GameObject[] otherPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject p in otherPlayers) {
            p.GetComponent<VRPlayerController>().SetPlayerNotControlled();
        }

        withHands.active = false;
        withoutHands.active = true;
        VRRig rigComp = GetComponentInChildren<VRRig>();
        vrTargetHead = GameObject.Find("Main Camera").transform;
        GameObject handModels = GameObject.Find("Hand Models");
        var fingers = Resources.FindObjectsOfTypeAll<RiggedFinger>();
        foreach (RiggedFinger finger in fingers) {
            if (finger.transform.parent.gameObject.name.Equals("left_palm")) {
                vrTargetLeftHand = finger.transform.parent;
            } else if (finger.transform.parent.gameObject.name.Equals("right_palm")) {
                vrTargetRightHand = finger.transform.parent;
            }
        }
        cameraRig = GameObject.Find("Leap Rig");

        rigComp.enabled = true;
        //sdkManager.SetActive(true);
        cameraRig.transform.position = rigComp.head.rigTarget.position;
        cameraRig.transform.rotation = transform.rotation;
        cameraRig.transform.localScale = new Vector3(1.0F, 1.0F, 1.0F);
        rigComp.head.vrTarget = vrTargetHead;
        rigComp.leftHand.vrTarget = vrTargetLeftHand;
        rigComp.rightHand.vrTarget = vrTargetRightHand;
        hipsTransform = hipsTransformWithoutHands;
        withHands.active = false;
        withoutHands.active = true;
    }

    public void SetPlayerNotControlled() {
        VRRig rig = GetComponentInChildren<VRRig>();
        if (rig != null) {
            rig.enabled = false;
        }
        hipsTransform = hipsTransformWithHands;
        withHands.active = true;
        withoutHands.active = false;
    }

    private float yVel = 0.0F;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (withHands.active) { // If not controlled
            RaycastHit hit;
            Ray ray = new Ray(hipsTransform.position, Vector3.down);
            if (Physics.Raycast(ray, out hit, legLength * scene.transform.localScale.y)) {
                onGround = true;
            } else {
                onGround = false;
            }
            ray = new Ray(hipsTransform.position, Vector3.down);
            if (Physics.Raycast(ray, out hit, (legLength - 0.2F) * scene.transform.localScale.y)) {
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
            targetTransform.position += new Vector3(0.0F, yVel * scene.transform.localScale.y, 0.0F);
        }
    }
}
