#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemBomb : NetworkBehaviour
{
    private float countdown = 0.0F;
    public float timeToExplode = 1.0F;
    private GameObject lastHit;
    public GameObject destroyedBombPrefab;
    public float radius = 3.25F;
    public float damage = 100.0F;


    // Start is called before the first frame update
    void Start() {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    void Update() {
        if (countdown > 0.0F) {
            countdown -= Time.deltaTime;
            if (countdown <= 0.0F && lastHit != null && lastHit.GetComponent<Deformable>() != null) {
                CmdExplode(lastHit);
                Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
                foreach (Object o in connectedPlayers) {
                    ((ConnectedPlayerManager) o).EndTurn(true, true);
                }
            }
        }
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
        ground.GetComponent<Deformable>().AddDeformation(transform.position, radius - 0.25F, 1.0F);
        Destroy(gameObject, 0.1F);
    }

    private void OnDestroy() {
        Instantiate(destroyedBombPrefab, transform.position, transform.rotation, GameObject.Find("Scene").transform);
    }

    private void OnCollisionEnter(Collision coll) {
        if (hasAuthority) {
            if ((lastHit != null && lastHit.GetComponent<Deformable>() == null) || countdown <= 0.0F) {
                lastHit = coll.gameObject;
                countdown = timeToExplode;
            }
        }
    }
}
