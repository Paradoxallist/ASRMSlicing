using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeSlice : MonoBehaviour
{

    public static KnifeSlice Instance;

    [SerializeField]
    private Transform startPos, endPos;
    private bool moved;
    private float time = 0;

    [SerializeField]
    private Vector3 triggerEnterTipPosition, triggerEnterBasePosition, triggerExitTipPosition;
    [SerializeField]
    Vector3 offset = new Vector3(0, 0, 0.01f);
    [SerializeField]
    private Material paintSliceMeshMaterial;

    private void Awake()
    {
        Instance = this;
    }


    public void MoveDown()
    {
        moved = true;
    }

    public float GetTime()
    {
        return time;
    }

    public Vector3 GetKnifePos()
    {
        return transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            time += Time.deltaTime;
        }
        else
        {
            time -= Time.deltaTime;
        }
        time = Mathf.Clamp(time, 0.0f, 1f);
        transform.position = Vector3.Lerp(startPos.position, endPos.position, time);

        if (moved)
        {
            if (time >= 1)
            {
                moved = false;
                PlayerController.Instance.EndSlice();
            }
        }
        else
        {
            if (time <= 0)
            {
                PlayerController.Instance.MoveAgain();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Sclice(other);
    }

    private Collider Sclice(Collider other)
    {
        if (other.GetComponent<Sliceable>() == null) return null;

        bool paint = other.GetComponent<Sliceable>().PaintMesh;
        Collider newRelatedObject = null;
        if (other.GetComponent<Sliceable>().RelatedObject != null)
        {
            newRelatedObject = Sclice(other.GetComponent<Sliceable>().RelatedObject);
        }

        GameObject[] slices = ScliceMesh(triggerEnterBasePosition, triggerEnterTipPosition, triggerExitTipPosition, other);

        if (slices == null) return null;

        GameObject[] falled = ScliceMesh(triggerEnterBasePosition - offset, triggerEnterTipPosition - offset, triggerExitTipPosition - offset, slices[0].GetComponent<Collider>());
        GameObject[] moved = ScliceMesh(triggerEnterBasePosition + offset, triggerEnterTipPosition + offset, triggerExitTipPosition + offset, slices[1].GetComponent<Collider>());

        falled[0].AddComponent<SlicibleMoveDown>();
        PlayerController.Instance.AddMovedSubObject(moved[1]);
        PlayerController.Instance.AddFalledSubObject(falled[0]);

        if (paint)
        {
            falled[1].AddComponent<SlicibleMoveDown>();
            moved[0].AddComponent<SlicibleMoveDown>().BlockedMesh(moved[1]);
            moved[0].GetComponent<MeshRenderer>().material = paintSliceMeshMaterial;
            falled[1].GetComponent<MeshRenderer>().material = paintSliceMeshMaterial;
            PlayerController.Instance.AddMovedSubObject(moved[0]);
            PlayerController.Instance.AddAdgeMesh(moved[0]);
            PlayerController.Instance.AddFalledSubObject(falled[1]);
        }
        else
        {
            Destroy(moved[0].gameObject);
            Destroy(falled[1].gameObject);
        }

        if (newRelatedObject != null)
        {
            moved[1].GetComponent<Sliceable>().RelatedObject = newRelatedObject;
        }

        return moved[1].GetComponent<Collider>();
    }

    private GameObject[] ScliceMesh(Vector3 triggerEnterBasePosition, Vector3 triggerEnterTipPosition, Vector3 triggerExitTipPosition, Collider other)
    {
        //Create a triangle between the tip and base so that we can get the normal
        Vector3 side1 = triggerExitTipPosition - triggerEnterTipPosition;
        Vector3 side2 = triggerExitTipPosition - triggerEnterBasePosition;

        //Get the point perpendicular to the triangle above which is the normal
        Vector3 normal = Vector3.Cross(side1, side2).normalized;

        //Transform the normal so that it is aligned with the object we are slicing's transform.
        Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

        //Get the enter position relative to the object we're cutting's local transform
        Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(triggerEnterTipPosition);

        Plane plane = new Plane();

        plane.SetNormalAndPosition(
                transformedNormal,
                transformedStartingPoint);

        var direction = Vector3.Dot(Vector3.up, transformedNormal);

        //Flip the plane so that we always know which side the positive mesh is on
        if (direction < 0)
        {
            plane = plane.flipped;
        }

        GameObject[] slices = Slicer.Slice(plane, other.gameObject);

        Destroy(other.gameObject);

        return slices;
    }
}
