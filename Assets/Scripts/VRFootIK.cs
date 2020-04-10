#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRFootIK : MonoBehaviour
{
    private Animator animator;
    public Vector3 footOffset;
    [Range(0.0F, 1.0F)]
    public float rightFootPosWeight = 1.0F;
    [Range(0.0F, 1.0F)]
    public float leftFootPosWeight = 1.0F;
    [Range(0.0F, 1.0F)]
    public float rightFootRotWeight = 1.0F;
    [Range(0.0F, 1.0F)]
    public float leftFootRotWeight = 1.0F;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void DoIKFoot(AvatarIKGoal foot, float posWeight, float rotWeight) {
        Vector3 footPos = animator.GetIKPosition(foot);
        RaycastHit hit;

        bool didHit = Physics.Raycast(footPos + Vector3.up, Vector3.down, out hit);
        if (didHit) {
            animator.SetIKPositionWeight(foot, posWeight);
            animator.SetIKPosition(foot, hit.point + footOffset);
            Quaternion footRot = Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal);
            animator.SetIKRotationWeight(foot, rotWeight);
            animator.SetIKRotation(foot, footRot);
        } else {
            animator.SetIKPositionWeight(foot, 0.0F);
        }
    }

    private void OnAnimatorIK(int layerIndex) {
        DoIKFoot(AvatarIKGoal.RightFoot, rightFootPosWeight, rightFootRotWeight);
        DoIKFoot(AvatarIKGoal.LeftFoot, leftFootPosWeight, leftFootRotWeight);
    }
}
