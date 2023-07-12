using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SlicibleMoveDown : MonoBehaviour
{
    private bool isActive;

    private Transform[] bones;
    private float speed = 1;

    private GameObject blockedMesh;

    private void Start()
    {
        CreateBones();
        isActive = true;
    }

    public void BlockedMesh(GameObject blocked = null)
    {
        blockedMesh = blocked;
    }


    public void CreateBones()
    {

        gameObject.AddComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer rend = GetComponent<SkinnedMeshRenderer>();

        Mesh mesh = rend.sharedMesh;

        Vector3[] verticles = mesh.vertices;
        Vector2[] uv = mesh.uv;
        int[] trignales = mesh.triangles;

        mesh = new Mesh();
        mesh.vertices = verticles;
        mesh.uv = uv;
        mesh.triangles = trignales;
        mesh.RecalculateNormals();

        // assign bone weights to mesh
        int vertex = mesh.vertexCount;


        BoneWeight[] weights = new BoneWeight[vertex];

        for (int i = 0; i < vertex; i++)
        {
            weights[i].weight0 = 1f;
            weights[i].boneIndex0 = i;
        }


        mesh.boneWeights = weights;



        bones = new Transform[vertex];
        Matrix4x4[] bindPoses = new Matrix4x4[vertex];
        for (int i = 0; i < vertex; i++)
        {
            bones[i] = new GameObject("SmallBone_" + i).transform;
            bones[i].localRotation = Quaternion.identity;
            bones[i].position = mesh.vertices[i] + transform.position;
            bones[i].parent = transform;
           bindPoses[i] = bones[i].worldToLocalMatrix * transform.localToWorldMatrix;
        }

        mesh.bindposes = bindPoses;

        // Assign bones and bind poses
        rend.bones = bones;
        rend.sharedMesh = mesh;

    }

    private float time = 0;

    private void Update()
    {
        if (bones == null || !isActive || blockedMesh != null) return;
        if(time + 0.1f > KnifeSlice.Instance.GetTime()) return;
        if (Input.GetMouseButton(0) || (Input.touchCount > 0)){
            time += Time.deltaTime;
            foreach (Transform tr in bones)
            {
                tr.position += new Vector3(0, -0.1f, -1) * Time.deltaTime * speed * 0.5f * Mathf.Pow(tr.position.y, 3) / 1;
                tr.eulerAngles += new Vector3(0, 0, 0) * Time.deltaTime * speed * 3f * Mathf.Pow(tr.position.y, 3) / 1;

            }

            if (time >= 1f)
            {
                isActive = false;
            }
        }
    }
}
