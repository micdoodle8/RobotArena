using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketScript : MonoBehaviour
{
    private Rigidbody rigidbody;
    public float dropRate = 2.5F;
    public GameObject destroyedRocketPrefab;
    private Vector3 steerDir;
    private bool fingerExtended;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update() {
        
    }

    private void FixedUpdate() {
        if (fingerExtended) {
            if (transform.forward.y > -0.5F) {
                transform.Rotate(Vector3.right, dropRate * Time.deltaTime, Space.Self);
            }
            transform.Rotate(-steerDir.y * 0.5F, steerDir.x, 0.0F, Space.Self);
            rigidbody.velocity = steerDir;
        } else {
            rigidbody.velocity = Vector3.zero;
        }
    }

    public void Steer(Vector3 steerDir, bool fingerExtended) {
        this.steerDir = steerDir;
        this.fingerExtended = fingerExtended;
    }

    private void OnCollisionEnter(Collision coll) {
        if (coll.gameObject.GetComponent<Deformable>() != null) {
            coll.gameObject.GetComponent<Deformable>().AddDeformation(transform.position, 1.5F, 3.0F);
            Destroy(gameObject, 0.1F);
            Instantiate(destroyedRocketPrefab, transform.position, transform.rotation, GameObject.Find("Scene").transform);
        }
    }
}
