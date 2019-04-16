using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMouvement : MonoBehaviour
{
    public static PlayerMouvement instance { get; private set; }
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int playerLife = 3;
    [SerializeField] private GameObject hitCollider;
    [SerializeField] private float blinkInterval = 0.2f;
    [SerializeField] private float maxInvicibilityBlink = 1;
    [SerializeField] private GameObject playerSprite;
    
    private bool _isInvincible;
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
        animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    // Update is called once per frame
    void Update()
    {
       
       body2D.velocity = new Vector2(h,v) * playerSpeed;
       if (Input.GetMouseButton(1) && timeDelay < Time.time)
       {
           Shoot();
       }
       Die();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Hitbox") && !_isInvincible)
        {
            StartCoroutine(InvincibilityBlink(maxInvicibilityBlink));
            playerLife--;
        }

        if (other.gameObject.CompareTag("Goal"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void Die()
    {
        if (playerLife == 0)
        {
            Scene loadedLevel = SceneManager.GetActiveScene();
            SceneManager.LoadScene(loadedLevel.buildIndex);
        }
    }

    public int GetLife()
    {
        return playerLife;
    }
    
    private IEnumerator InvincibilityBlink(float time)
    {
        _isInvincible = true;
        hitCollider.SetActive(false);
        for (float i = 0; i < time; i += blinkInterval)
        {
            if (playerSprite.activeInHierarchy)
            {
                playerSprite.SetActive(false);
            }
            
            else
            {
                playerSprite.SetActive(true);
            }
            
            yield return new WaitForSeconds(blinkInterval);
        }
        
        playerSprite.SetActive(true);
        hitCollider.SetActive(true);

        _isInvincible = false;
    }
    
}
