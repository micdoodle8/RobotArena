#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMove : NetworkBehaviour
{
    public bool isLerpingPos;
    public bool isLerpingRot;
    public Quaternion lastRealRot;
    public Quaternion realRot;
    public Vector3 lastRealPos;
    public Vector3 realPos;
    public float lerpStartTime;
    public float timeToLerp;

    public bool shouldSendMovement;
    public float speed;
    public float networkSendRate = 5;
    public float timeBetweenMovementStart;
    public float timeBetweenMovementEnd;

    private Vector3 posAtLastSend;
    private Quaternion rotAtLastSend;
    public float moveThreshold = 0.001F;
    public float rotThreshold = 0.001F;

    void Update() {
        if (hasAuthority) {
            if (shouldSendMovement
                        && (posAtLastSend == null || (posAtLastSend - transform.position).magnitude >= moveThreshold) 
                        && (rotAtLastSend == null || (rotAtLastSend.eulerAngles - transform.rotation.eulerAngles).magnitude >= rotThreshold)) {
                shouldSendMovement = false;
                StartCoroutine(StartNetworkSendCooldown());
            }            
        }
    }

    private void FixedUpdate() {
        if (!hasAuthority) {
            NetworkLerp();
        }
    }

    public override void OnStartAuthority() {
        if (hasAuthority) {
            // RegisterMessages();
            shouldSendMovement = true;
        }
    }

    private IEnumerator StartNetworkSendCooldown() {
        timeBetweenMovementStart = Time.time;
        yield return new WaitForSeconds((1 / networkSendRate));
        SendNetworkMovement();
    }

    private void SendNetworkMovement() {
        timeBetweenMovementEnd = Time.time;
        posAtLastSend = transform.position;
        rotAtLastSend = transform.rotation;
        CmdUpdateState(transform.position, transform.rotation, timeBetweenMovementEnd - timeBetweenMovementStart);
        shouldSendMovement = true;
    }

    [Command]
    private void CmdUpdateState(Vector3 pos, Quaternion rot, float time) {
        RpcUpdateState(pos, rot, time);
    }

    [ClientRpc]
    private void RpcUpdateState(Vector3 pos, Quaternion rot, float time) {
        if (!hasAuthority) {
            lastRealPos = realPos;
            lastRealRot = realRot;
            realRot = rot;
            realPos = pos;
            timeToLerp = time;

            if (realPos != transform.position) {
                isLerpingPos = true;
            }

            if (realRot.eulerAngles != transform.rotation.eulerAngles) {
                isLerpingRot = true;
            }

            lerpStartTime = Time.time;
        }
    }

    private void NetworkLerp() {
        if (isLerpingPos) {
            float lerpP = (Time.time - lerpStartTime) / timeToLerp;
            transform.position = Vector3.Lerp(lastRealPos, realPos, lerpP);
        }

        if (isLerpingRot) {
            float lerpP = (Time.time - lerpStartTime) / timeToLerp;
            transform.rotation = Quaternion.Lerp(lastRealRot, realRot, lerpP);
        }
    }
}
