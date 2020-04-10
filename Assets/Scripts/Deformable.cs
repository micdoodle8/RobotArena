#pragma warning disable 0618
using UnityEngine;
using UnityEngine.Networking;

public class Deformable : NetworkBehaviour
{
    private Vector3[] vertsIn;
    private Vector3[] vertsOut;

    private GenerateMesh generateMesh; 

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
        for (int i = 0; i < vertsOut.Length; ++i) {
            float euclideanDist = (localPoint - (vertsOut[i])).magnitude;
            
            if (euclideanDist < rad) {
                float depthMult = ((rad - euclideanDist) / rad);
                depthMult += 0.1F;
                depthMult = Mathf.Clamp(depthMult, 0, 1);
                vertsOut[i] = vertsIn[i] + Vector3.down * amount * depthMult;
            }
        }
        generateMesh.UpdateMesh(vertsOut);
    }

    [ClientRpc]
    void RpcUpdateDeformation(Vector3 worldPoint, float rad, float amount) {
        if (!isServer) {
            AddDeformation(worldPoint, rad, amount);
        }
    }
}
