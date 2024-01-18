using UnityEngine;
using System.Collections;
using System;

public class ShowCollider : MonoBehaviour
{
    void OnDrawGizmos()
    {

        BoxCollider bCol = GetComponent<BoxCollider>();
        SphereCollider sCol = GetComponent<SphereCollider>();

        Gizmos.color = Color.green;
        
        if (bCol != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(bCol.center, bCol.size);
        }

        if (sCol != null)
        {
            float maxScale = Math.Max(gameObject.transform.localScale.x,
            Math.Max(gameObject.transform.localScale.y, gameObject.transform.localScale.z));

            Gizmos.DrawWireSphere(sCol.bounds.center, sCol.radius * maxScale);
        }

    }

}