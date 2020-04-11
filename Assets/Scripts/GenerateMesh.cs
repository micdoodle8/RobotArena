#pragma warning disable 0618
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateMesh : MonoBehaviour
{
    public int xSize = 16;
    public int ySize = 16;
    public float scale = 0.1F;
    public int edgeTrim = 3; // Ignore these pixels on the edge of the image
    public float heightScale = 10.0F;

    private MeshFilter filter;
    private MeshCollider groundCollider;
    private Mesh mesh;

    public Texture2D image;

    void Awake() {
        filter = gameObject.AddComponent<MeshFilter>();
        groundCollider = gameObject.AddComponent<MeshCollider>();
        Generate();
    }

    Mesh Generate() {
        mesh = new Mesh();
        mesh.name = "TerrainMesh";
        mesh.MarkDynamic();
        filter.mesh = mesh;
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();

        float yVal;
        Color c;
        // Set positions and normals:
        for (int i = 0, y = 0; y <= ySize; ++y) {
            for (int x = 0; x <= xSize; ++x, ++i) {
                c = image.GetPixel((int)((x / (float)xSize) * (image.width - edgeTrim * 2)) + edgeTrim, 
                                            (int)((y /  (float)ySize) * (image.height - edgeTrim * 2)) + edgeTrim);
                yVal = c.r * 3.0F;
                verts.Add(new Vector3(x * scale + xSize * scale * -0.5F, yVal, y * scale + ySize * scale * -0.5F));
                norms.Add(new Vector3(0, 1, 0));
            }
        }

        // Set indices to render:
        int[] tris = new int[xSize * ySize * 6];
        for (int index = 0, vi = 0, y = 0; y < ySize; ++y, ++vi) {
            for (int x = 0; x < xSize; ++x, index += 6, vi++) {
                // First triangle:
                tris[index] = vi;
                tris[index + 1] = vi + xSize + 1;
                tris[index + 2] = vi + 1;
                // Second triangle:
                tris[index + 3] = vi + 1;
                tris[index + 4] = vi + xSize + 1;
                tris[index + 5] = vi + xSize + 2;
            }
        }
        
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        groundCollider.sharedMesh = null;
        groundCollider.sharedMesh = mesh;

        Deformable deformable = GetComponent<Deformable>();
        if (deformable != null) {
            deformable.OnMeshGenerate();
        }

        return mesh;
    }

    public void UpdateMesh(Vector3[] verts) {
        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
        groundCollider.sharedMesh = null;
        groundCollider.sharedMesh = mesh;
    }

    public Vector3[] GetVerts() {
        return mesh.vertices;
    }
}
