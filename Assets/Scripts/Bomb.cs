using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Bomb : NetworkBehaviour
{
    private float countdown = 0.0F;
    public float timeToExplode = 1.0F;
    private GameObject lastHit;
    public GameObject destroyedBombPrefab;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;
    }

    // Update is called once per frame
    void Update() {
        if (countdown > 0.0F) {
            countdown -= Time.deltaTime;
            if (countdown <= 0.0F && lastHit != null) {
                CmdExplode(lastHit);
            }
        }
    }

    [Command]
    private void CmdExplode(GameObject ground) {
        if (ground.GetComponent<Deformable>() != null) {
            ground.GetComponent<Deformable>().AddDeformation(transform.position, 3.0F, 3.0F);
            Destroy(gameObject, 0.1F);
        }
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
