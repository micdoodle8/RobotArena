using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Deformable : NetworkBehaviour
{
    private Vector3[] vertsIn;
    private Vector3[] vertsOut;

    private GenerateMesh generateMesh; 
    private List<GameObject> recentlyCollided = new List<GameObject>();

    public void OnMeshGenerate() {
        generateMesh = GetComponent<GenerateMesh>();
        vertsIn = generateMesh.GetVerts();
        vertsOut = generateMesh.GetVerts();
    }

    public void AddDeformation(Vector3 worldPoint, float rad, float amount) {
        if (isServer) {
            RpcUpdateDeformation(worldPoint, rad, amount);
        }
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        // Vector3 localPoint = new Vector3(localPoint4.x, localPoint4.y, localPoint4.z);
        // Debug.Log(worldPoint + " " + localPoint);
        for (int i = 0; i < vertsOut.Length; ++i) {
            float euclideanDist = (localPoint - (vertsOut[i])).magnitude;
            
            if (euclideanDist < rad) {
                float depthMult = ((rad - euclideanDist) / rad);
                depthMult += 0.1F;
                // depthMult += Random.value * 0.1F;
                depthMult = Mathf.Clamp(depthMult, 0, 1);
                vertsOut[i] = vertsIn[i] + Vector3.down * amount * depthMult;
            }
        }
        generateMesh.UpdateMesh(vertsOut);
        Debug.Log("Terraformable::AddDeformation -- Done Deforming on " + (isServer ? "Server" : "Client"));
    }

    [ClientRpc]
    void RpcUpdateDeformation(Vector3 worldPoint, float rad, float amount) {
        if (!isServer) {
            AddDeformation(worldPoint, rad, amount);
        }
    }

    private void OnCollisionEnter(Collision coll) {
        // if (!recentlyCollided.Contains(coll.gameObject)) {
            Debug.Log("Coll1");
        if (isServer && coll.relativeVelocity.magnitude > 5.0F) {
            Debug.Log("Coll2");
            foreach (ContactPoint point in coll.contacts) {
                // AddDeformation(point.point, 1.4F, Mathf.Clamp((coll.relativeVelocity.magnitude * coll.rigidbody.mass) / 500.0F, 0.5F, 1.5F));
                recentlyCollided.Add(coll.gameObject);
            }
        }
    }
}
