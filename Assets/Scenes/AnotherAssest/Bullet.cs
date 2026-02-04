using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}

