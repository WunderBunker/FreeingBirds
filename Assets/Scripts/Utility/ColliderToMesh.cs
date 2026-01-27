using UnityEngine;
using System.Linq;


[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class ColliderToMesh : MonoBehaviour
{
    private PolygonCollider2D gPolygon;
    Mesh gMesh;

    void Awake()
    {
        gPolygon = gameObject.GetComponent<PolygonCollider2D>();

        MeshFilter vMeshFilter = GetComponent<MeshFilter>();
        gMesh = new Mesh();
        vMeshFilter.mesh = gMesh;
        
        MajMesh();
    }

    public void  MajMesh()
    {
        Vector2[] vMeshUV = new Vector2[gPolygon.points.Count()];
        Vector3[] vMeshVertices = new Vector3[gPolygon.points.Count()];

        int[] vMeshTriangles;

        //On  récupère les points et les UV directements à partir des points du polygon
        for (int lCptPoints = 0; lCptPoints < gPolygon.points.Count(); lCptPoints++)
        {
            vMeshUV[lCptPoints] = gPolygon.points[lCptPoints] + gPolygon.offset;
            vMeshVertices[lCptPoints] = new Vector3(gPolygon.points[lCptPoints].x + gPolygon.offset.x, gPolygon.points[lCptPoints].y + gPolygon.offset.y, 0);
        }

        //On récupère les triangles en triangulant les UV
        vMeshTriangles = TriangulatorBis.Triangulate( ref vMeshUV);

        gMesh.Clear();

        //On maj les données dans la mesh
        gMesh.triangles = null;
        gMesh.vertices = vMeshVertices;
        gMesh.uv = vMeshUV;
        gMesh.triangles = vMeshTriangles;
        gMesh.RecalculateBounds();
        //gMesh.RecalculateNormals();
        gMesh.normals = Enumerable.Repeat(Vector3.back, vMeshVertices.Length).ToArray();
    }

     #if UNITY_EDITOR

   /*  private void OnDrawGizmos()
    {
        Gizmos.DrawMesh(gMesh);
    } */

    #endif
}