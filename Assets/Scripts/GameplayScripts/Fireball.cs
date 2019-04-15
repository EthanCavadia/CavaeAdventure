using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] private float bulletSpeed;
    private Rigidbody2D body2D;
    private Vector2 target;
    
    private void Start()
    {
        body2D = GetComponent<Rigidbody2D>();
        target = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
    }

    private void Update()
    {   
        body2D.velocity = target.normalized * bulletSpeed;
        Destroy(gameObject, 0.2f);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
