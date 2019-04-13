using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D body2D;
    // Start is called before the first frame update
    void Start()
    {
        body2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 shoot = (transform.position - transform.position).normalized;
        body2D.AddForce(shoot * 500.0f);
    }
}
