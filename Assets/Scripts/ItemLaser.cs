#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Leap.Unity;

public class ItemLaser : NetworkBehaviour
{
    private Renderer[] cylinderRenderers;
    public float lifespan = 2.0F;
    private float timeToLive;
    private List<GameObject> hitEntities = new List<GameObject>();
    public float damageMultiplier = 20.0F;
    public GameObject firingPlayer = null;

    void Start() {
        cylinderRenderers = GetComponentsInChildren<Renderer>();
        timeToLive = lifespan;
    }

    [Command]
    private void CmdDestroy() {
        Destroy(gameObject);
    }

    void Update() {
        timeToLive -= Time.deltaTime;
        if (hasAuthority) {
            if (timeToLive <= 0.0F) {
                CmdDestroy();
            } else {
                float scale = Mathf.Sin(Mathf.PI * (timeToLive / lifespan));
                gameObject.transform.localScale = new Vector3(scale, scale, 1.0F);
                int numActiveHands = 0;
                Vector3 avgPos = Vector3.zero;
                Vector3 rightHand = Vector3.right;
                Vector3 leftHand = Vector3.zero;
                GameObject handModels = GameObject.Find("Hand Models");
                for (int i = 0; i < handModels.transform.childCount; ++i) {
                    GameObject hand = handModels.transform.GetChild(i).gameObject;
                    if (hand.active) {
                        RiggedHand rh = hand.GetComponent<RiggedHand>();
                        if (rh != null) {
                            numActiveHands++;
                            avgPos += rh.GetLeapHand().PalmPosition.ToVector3();
                            if (rh.Handedness == Chirality.Right) {
                                rightHand = rh.GetLeapHand().PalmPosition.ToVector3();
                            } else {
                                leftHand = rh.GetLeapHand().PalmPosition.ToVector3();
                            }
                        }
                    }
                }
                if (numActiveHands != 2) {
                    CmdDestroy();
                } else {
                    transform.position = avgPos / 2.0F;
                    Vector3 rightVec = (rightHand - leftHand).normalized;
                    Vector3 forwardVec = Vector3.Cross(rightVec, Vector3.up);

                    // https://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
                    Vector3 startPoint = transform.position;
                    
                    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                    GameObject closestPlayer = null;
                    float lowestDist = float.MaxValue;
                    foreach (GameObject player in players) {
                        if (player.GetComponent<ControllableRobot>().teamContainer == firingPlayer.GetComponent<ControllableRobot>().teamContainer) {
                            continue;
                        }
                        Vector3 pos = player.transform.position;
                        float t = ((pos.x - startPoint.x) * forwardVec.x + (pos.z - startPoint.z) * forwardVec.z) / (forwardVec.x * forwardVec.x + forwardVec.z * forwardVec.z);
                        Vector3 pointToLine = pos - (startPoint + t * forwardVec);
                        pointToLine.y = 0.0F;
                        float mag = pointToLine.magnitude;
                        if (mag < lowestDist) {
                            lowestDist = mag;
                            closestPlayer = player;
                        }
                    }

                    if (closestPlayer != null) {
                        Vector3 toPlayer = ((closestPlayer.transform.position + new Vector3(0.0F, 1.0F, 0.0F)) - startPoint);
                        forwardVec = new Vector3(forwardVec.x * toPlayer.magnitude, toPlayer.y, forwardVec.z * toPlayer.magnitude);
                    }

                    transform.rotation = Quaternion.LookRotation(forwardVec.normalized);
                    foreach (Renderer renderer in cylinderRenderers) {
                        renderer.enabled = true;
                    }
                }
            }
        }

        if (isServer) {
            foreach (GameObject hitObj in hitEntities) {
                Health health = hitObj.GetComponent<Health>();
                if (health != null) {
                    health.DoDamage(Time.deltaTime * damageMultiplier);
                }
            }
        }
    }

    void OnDestroy() {
        if (hasAuthority) {
            Object[] connectedPlayers = GameObject.FindObjectsOfType(typeof(ConnectedPlayerManager));
            foreach (Object o in connectedPlayers) {
                ((ConnectedPlayerManager) o).EndTurn(true, true);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        Transform hitTransform = other.gameObject.transform.parent;
        if (hitTransform.tag.Equals("Player") && hitTransform.gameObject != firingPlayer) {
            if (!hitEntities.Contains(hitTransform.gameObject)) {
                hitEntities.Add(hitTransform.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        Transform hitTransform = other.gameObject.transform.parent;
        if (hitTransform.tag.Equals("Player")) {
            if (hitEntities.Contains(hitTransform.gameObject)) {
                hitEntities.Remove(hitTransform.gameObject);
            }
        }
    }
}
