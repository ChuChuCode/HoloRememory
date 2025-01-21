using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Fog_Mask : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] int Split_Num;
    [SerializeField] Mask_Type Mask_Radius;
    [SerializeField] LayerMask LandMask;
    enum Mask_Type
    {
        Character = 7,
        Minion = 5,
        Tower = 10
    }
    Transform Origin;
    Mesh mesh;
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
        gameObject.layer = LayerMask.NameToLayer("Mask");
        Origin = transform.parent;
        // gameObject.transform.SetParent(null);
    }
    void LateUpdate()
    {
        Vector3[] vertices = new Vector3[Split_Num+1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[Split_Num*3];

        float step = 360f / Split_Num;
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f , 0.5f);
        int verexIndex = 1;
        int triangleIndex = 0 ;
        for (int i = 0 ; i < Split_Num ; i++)
        {
            Vector3 direction = new Vector3(Mathf.Cos( i * step * Mathf.Deg2Rad ), 0, Mathf.Sin( i * step * Mathf.Deg2Rad ));
            RaycastHit hit;
            if ( Physics.Raycast(transform.position, direction, out hit, (int)Mask_Radius,LandMask) )
            {
                // change to local space
                vertices[verexIndex] = hit.point - transform.position;
            }
            else
            {
                vertices[verexIndex] = direction * (int)Mask_Radius;
            }
            uv[verexIndex] = uv[0] + new Vector2 (direction.x , direction.z) * 0.5f;
            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = verexIndex;
                triangles[triangleIndex + 2] = verexIndex - 1;

                triangleIndex += 3;
            }
            verexIndex++;
        }
        triangles[triangleIndex + 0] = 0;
        triangles[triangleIndex + 1] = 1 ;
        triangles[triangleIndex + 2] = verexIndex - 1;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(transform.position, Vector3.one * (int)Mask_Radius);
    }
}
