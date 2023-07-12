using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField]
    private GameObject movedObject;

    [SerializeField]
    private GameObject falledObject; 

    [SerializeField]
    private KnifeSlice knifeSlice;

    private bool startSlice;

    private List<GameObject> falledObjects = new List<GameObject>();
    private List<GameObject> movedObjects = new List<GameObject>();
    private List<GameObject> edgeMeshes = new List<GameObject>();

    private bool startFall = false;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        startSlice = false;
    }

    void Update()
    {
        if (startSlice == false)
        {
            movedObject.transform.position += new Vector3(0, 0, -1) * Time.deltaTime;
            if(movedObject.transform.position.z < -12.5f)
            {
                SceneManager.LoadScene(0);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (startSlice == false)
            {
                startSlice = true;
                knifeSlice.MoveDown();
                movedObjects = new List<GameObject>();
            }
        }
        if (startFall)
        {
            falledObject.transform.position += new Vector3(0, -1, -1) * Time.deltaTime * 3;
            falledObject.transform.eulerAngles += new Vector3(-1, 0, 0) * Time.deltaTime * 60;
        }
    }

    public void AddMovedSubObject(GameObject subObject)
    {
        //movedObjects.Add(subObject);
        subObject.transform.parent = movedObject.transform;
    }

    public void AddAdgeMesh(GameObject edgeMesh)
    {
        edgeMeshes.Add(edgeMesh);
    }

    public void AddFalledSubObject(GameObject subObject)
    {
        if (falledObjects.Count == 0)
        {
            falledObject.transform.position = subObject.transform.position;
            edgeMeshes.ForEach(x =>
            {
                if (x != null)
                {
                    falledObjects.Add(x);
                }
            });
            edgeMeshes = new List<GameObject>();
        }
        falledObjects.Add(subObject);
        subObject.transform.parent = falledObject.transform;
    }

    public void MoveAgain()
    {
        if (startSlice == false) return;
        startSlice = false;
        falledObjects.ForEach(x => Destroy(x.gameObject));
        falledObjects.Clear();
        startFall = false;
        movedObjects.ForEach(x => { if (x != null) x.GetComponent<MeshCollider>().enabled = true; });
    }

    public void EndSlice()
    {
        startFall = true;

        movedObjects = new List<GameObject>();
        for (int i = 0;i < movedObject.transform.childCount;i++)
        {
            movedObjects.Add(movedObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < movedObjects.Count; i++)
        {
            if (movedObjects[i] != null)
            {
                if (GetLastZ(movedObjects[i]) < knifeSlice.transform.position.z)
                {
                    if (falledObjects.Count == 0)
                        falledObject.transform.position = movedObjects[i].transform.position;
                    falledObjects.Add(movedObjects[i]);
                    movedObjects[i].transform.parent = falledObject.transform;
                    movedObjects[i] = null;
                }
            }
        }
        movedObjects.RemoveAll(x => x == null);
    }

    private float GetLastZ(GameObject g)
    {
        Mesh mesh = g.GetComponent<MeshFilter>().mesh;
        float z = 100000000;
        for(int i = 0; i< mesh.vertices.Length; i++)
        {
            if (mesh.vertices[i].z < z)
                z = mesh.vertices[i].z;
        }
        return z + g.transform.position.z;
    }
}
