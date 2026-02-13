using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStayInBounds2D : MonoBehaviour
{
    public Transform boundsObject;
    private Collider2D boundsColl;

    void Start()
    {
        boundsColl = boundsObject.GetComponent<Collider2D>();
    }

    void LateUpdate()
    {
        Vector2 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, boundsColl.bounds.min.x, boundsColl.bounds.max.x);
        pos.y = Mathf.Clamp(pos.y, boundsColl.bounds.min.y, boundsColl.bounds.max.y);

        transform.position = pos;
    }
}

