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

    public void Map() {
        Vector3 target = vrTarget.TransformPoint(trackingPositionOffset);
        if (maxDistSource != null) {
            Vector3 diff = target - maxDistSource.position;
            if (diff.magnitude > maxDist) {
                target = maxDistSource.position + diff.normalized * maxDist;
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

    // Start is called before the first frame update
    void Start()
    {
        headBodyOffset = transform.position - headConstraint.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = headConstraint.position + headBodyOffset;
        transform.forward = Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized;

        // if (GameObject.Find("Team" + GetComponent<VRPlayerController>().teamID).GetComponent<Team>().currentPlayer == gameObject)
        {
            head.Map();
            leftHand.Map();
            rightHand.Map();
        }
    }
}
