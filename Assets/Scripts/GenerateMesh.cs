using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateMesh : MonoBehaviour
{
    public int xSize = 16;
    public int ySize = 16;
    public float scale = 0.1F;
    // public int gridSize = 16;

    private MeshFilter filter;
    private MeshCollider collider;
    private Mesh mesh;

    public Texture2D image;

    void Awake()
    {
        filter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<MeshCollider>();
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
        for (int i = 0, y = 0; y <= ySize; ++y) {
            for (int x = 0; x <= xSize; ++x, ++i) {

                Color c = image.GetPixel((int)((x / (float)xSize) * (image.width - 6)) + 3, (int)((y /  (float)ySize) * (image.height - 6)) + 3);
                yVal = c.r * 5.0F;
                if (yVal > 0.3F) {
                    yVal = 0.3F + yVal * 0.3F;
                }
                verts.Add(new Vector3(x * scale + xSize * scale * -0.5F, yVal, y * scale + ySize * scale * -0.5F));
                norms.Add(new Vector3(0, 1, 0));
            }
        }

        // transform.position = new Vector3(xSize * scale * -0.5F * transform.parent.localScale.x, 0.0F, ySize * scale * -0.5F * transform.parent.localScale.z);

        int[] tris = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; ++y, ++vi) {
            for (int x = 0; x < xSize; ++x, ti += 6, vi++) {
                tris[ti] = vi;
                tris[ti + 3] = vi + 1;
                tris[ti + 2] = vi + 1;
                tris[ti + 4] = vi + xSize + 1;
                tris[ti + 1] = vi + xSize + 1;
                tris[ti + 5] = vi + xSize + 2;
            }
        }
        
        mesh.SetVertices(verts);
        mesh.SetNormals(norms);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;

        Deformable deformable = GetComponent<Deformable>();
        if (deformable != null) {
            deformable.OnMeshGenerate();
        }

        return mesh;
    }

    public void UpdateMesh(Vector3[] verts) {
        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;
    }

    public Vector3[] GetVerts() {
        return mesh.vertices;
    }
}
