using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimController : MonoBehaviour
{
    [Range(0.0F, 1.0F)]
    public float lerpSpeed = 1.0F;
    public float speedThres = 0.1F;
    private Animator animator;
    private Vector3 prevPos;
    //private VRRig vrRig;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (gameObject.tag.Equals("Player")) {
            SetNotHanging();
        }
        //vrRig = GetComponent<VRRig>();
        //prevPos = vrRig.head.vrTarget.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 headSetSpeed = (transform.position - prevPos) / Time.deltaTime;
        headSetSpeed.y = 0.0F;
        Vector3 headSetLocalSpeed = transform.InverseTransformDirection(headSetSpeed);
        prevPos = transform.position;
        
        float prevDirX = animator.GetFloat("dirX");
        float prevDirY = animator.GetFloat("dirY");

        animator.SetBool("isMoving", headSetLocalSpeed.magnitude > speedThres);
        animator.SetFloat("dirX", Mathf.Lerp(prevDirX, Mathf.Clamp(headSetLocalSpeed.x, -1.0F, 1.0F), lerpSpeed));
        animator.SetFloat("dirY", Mathf.Lerp(prevDirY, Mathf.Clamp(headSetLocalSpeed.z, -1.0F, 1.0F), lerpSpeed));
    }

    public void SetNotHanging() {
        Debug.Log("Not hanging " + animator);
        animator.SetBool("isHanging", false);
    }
}
