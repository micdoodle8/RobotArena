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
    [Range(0.0F, 1.0F)]
    public float lerpRate = 0.1F;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update() {
        
    }

    private void FixedUpdate() {
        if (fingerExtended) {
            if (rigidbody.velocity.magnitude < 0.0001F) {
                rigidbody.velocity = steerDir * 4.0F;
            } else {
                rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, steerDir * 4.0F, lerpRate);
            }
            transform.rotation = Quaternion.LookRotation(rigidbody.velocity.normalized);
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
            Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
            foreach (Object o in connectedPlayers) {
                ((ConnectedPlayerManager) o).EndTurn();
            }
        }
    }
}
