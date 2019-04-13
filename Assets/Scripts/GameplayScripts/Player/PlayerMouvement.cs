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
    
    [SerializeField] private float playerSpeed;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        body2D = GetComponent<Rigidbody2D>();
        
        
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
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        
        Vector2 movement = new Vector2(h,v);
    }
    
    void Shoot()
    {
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }
}
