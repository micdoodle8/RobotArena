using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class RocketScript : NetworkBehaviour
{
    private Rigidbody rigidbody;
    public float dropRate = 2.5F;
    public GameObject destroyedRocketPrefab;
    private Vector3 steerDir;
    private bool fingerExtended;
    [Range(0.0F, 1.0F)]
    public float lerpRate = 0.1F;
    public float radius = 1.5F;
    public float damage = 50.0F;
    private bool hasHit = false;

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
        if (hasAuthority && !hasHit) {
            hasHit = true;
            CmdExplode(coll.gameObject);
            Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
            foreach (Object o in connectedPlayers) {
                ((ConnectedPlayerManager) o).EndTurn(true, false);
            }
        }
    }

    private void OnDestroy() {
        Instantiate(destroyedRocketPrefab, transform.position, transform.rotation, GameObject.Find("Scene").transform);
    }

    [Command]
    private void CmdExplode(GameObject ground) {
        Health[] healthObjs = GameObject.FindObjectsOfType<Health>();
        foreach (Health health in healthObjs) {
            Vector3 delta = health.gameObject.transform.position - transform.position;
            if (delta.magnitude < radius) {
                float depthMult = ((radius - delta.magnitude) / radius);
                health.DoDamage(damage * depthMult);
            }
        }
        ground.GetComponent<Deformable>().AddDeformation(transform.position, radius - 0.25F, 0.5F);
        Destroy(gameObject, 0.1F);
    }
}
