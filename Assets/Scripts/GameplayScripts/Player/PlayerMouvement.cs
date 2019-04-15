using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    public static PlayerMouvement instance { get; private set; }
    [SerializeField] private GameObject bulletPrefab;
    private float timeDelay;
    private float h;
    private float v;
    private Rigidbody2D body2D;
    private Animator animator;
    private Vector2 target;
    [SerializeField] private float playerSpeed;


    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        
        body2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
    }

    private void FixedUpdate()
    {
        Movement();
    }

    // Update is called once per frame
    void Update()
    {
       
       body2D.velocity = new Vector2(h,v) * playerSpeed;
       if (Input.GetMouseButtonDown(1) && timeDelay < Time.time)
       {
           Shoot();
       }
       
    }

    void Movement()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"), 0.0f) * playerSpeed;
        
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Magnitude", movement.magnitude);

        transform.position = transform.position + movement * Time.deltaTime;
    }
    
    void Shoot()
    {
        
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
       
    }
}
