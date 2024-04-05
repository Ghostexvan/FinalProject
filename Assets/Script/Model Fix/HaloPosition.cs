using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HaloPosition : MonoBehaviour
{
    // We drag this from the root bone transform in the prefab
    [SerializeField]
    private Transform HaloTargetTransform;

    [SerializeField]
    private float offsetX, offsetY, offsetZ;
    private Vector3 offsetV3;
    private Vector3 temp;

    private void Awake()
    {
        if (HaloTargetTransform == null)
        {
            Debug.LogError("Missing transform for Halo model");
        }

        //offsetX = -0.75f;
        //offsetY = 3.15f;
        offsetV3 = new Vector3(offsetX, offsetY, offsetZ);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = new Vector3(HaloTargetTransform.position.x - offsetX, HaloTargetTransform.position.y - offsetY, HaloTargetTransform.position.z);
        //this.transform.rotation = HaloTargetTransform.rotation * Quaternion.Euler(-90, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //offsetV3 = new Vector3(offsetX, offsetY, offsetZ);
        temp = new Vector3(HaloTargetTransform.position.x, HaloTargetTransform.position.y, HaloTargetTransform.position.z);
        offsetV3 = new Vector3(offsetX, offsetY, offsetZ);

        this.transform.position = temp - offsetV3;
            //new Vector3(HaloTargetTransform.position.x - offsetX, HaloTargetTransform.position.y - offsetY, HaloTargetTransform.position.z);
        //this.transform.position = HaloTargetTransform.position - offsetV3;
        //this.transform.rotation = HaloTargetTransform.rotation * Quaternion.Euler(-90, 0, 0);
    }
}
