using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public float lineWidth = 7f;
    public float depth = -5f;

    private LineRenderer lineRenderer;
    private GameObject trackPath;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        trackPath =  this.gameObject;

        int numPath = trackPath.transform.childCount;
        lineRenderer.positionCount = numPath + 1;

        for (int index = 0; index < numPath; index++){
            lineRenderer.SetPosition(index, new Vector3(trackPath.transform.GetChild(index).transform.position.x,
                                                        depth,
                                                        trackPath.transform.GetChild(index).transform.position.z));
        }

        lineRenderer.SetPosition(numPath, lineRenderer.GetPosition(0));

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
