using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesBehavior : MonoBehaviour
{
    public state currentState = state.IDLE;

    public enum state
    {
        IDLE,
        FOLLOW,
        ATTACK,
        DEAD
    }
    
    [SerializeField] private float _detectRange;
    [SerializeField] private float _attackRange;
    [SerializeField] private int enemyLife = 500;
        
    private Rigidbody2D _rigidbody2D;
    private List<Vector2> followingPath;
    private int indexPath;
    private float timer = 0;
    private Vector3 mouvement;
    private bool inAttackRange;
    private Animator enemyAnimation;
    private Transform player;
    private float attackDelay;
    
    private bool enemyDie = false;
    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        enemyAnimation = GetComponent<Animator>();
        followingPath = new List<Vector2>();
        player = FindObjectOfType<PlayerMouvement>().transform;
    }

    private void Update()
    {
        mouvement = new Vector3(_rigidbody2D.velocity.x, _rigidbody2D.velocity.y, 0.0f);

        enemyAnimation.SetFloat("Horizontal", mouvement.x);
        enemyAnimation.SetFloat("Vertical", mouvement.y);
        enemyAnimation.SetFloat("Magnitude", mouvement.magnitude);

        

        if (Vector2.Distance(transform.position, player.position) >= _attackRange && Vector2.Distance(transform.position, player.position) <= _detectRange)
        {
            currentState = state.FOLLOW;
            Debug.Log("player enter follow range");
        }
        
        if (Vector2.Distance(transform.position, player.position) <= _attackRange)
        {
            currentState = state.ATTACK;
            Debug.Log("player enter attack range");
        }

        if (enemyLife <= 0)
        {  
            currentState = state.DEAD;
        }
        
        switch (currentState)
        {
            case state.IDLE:
                inAttackRange = false;
                break;
            case state.FOLLOW:
                inAttackRange = false;
                if (timer > 1)
                {
                    FindPlayer();
                    timer = 0;
                }
                FollowPath();
                break;
            case state.ATTACK:
                inAttackRange = true;
                break;
            case state.DEAD:
                enemyDie = true;
                Die();
                break;

        }
        AttackPlayer();
        timer += Time.deltaTime;
    }

    private void FindPlayer()
    {
        followingPath = Pathfinding.Instance.Astar(transform.position);
        indexPath = 1;
    }

    private void FollowPath()
    {
        
        if (indexPath >= followingPath.Count)
        {
            _rigidbody2D.velocity = Vector2.zero;
            FindPlayer();
            return;
        }
        _rigidbody2D.velocity = followingPath[indexPath] - (Vector2) transform.position;
        _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * 2f;

        if (Vector2.Distance(transform.position, followingPath[indexPath]) < 0.1f)
        {
            indexPath++;
        }
    }

    private void AttackPlayer()
    {
        enemyAnimation.SetBool("InRange", inAttackRange);
    }

    private void Die()
    {
        _rigidbody2D.velocity = Vector2.zero;
        enemyAnimation.SetBool("EnemyDie", enemyDie);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Fireball"))
        {
            enemyLife -= 1;
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _detectRange);
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (followingPath == null) return;
        if (followingPath.Count <= indexPath) return;
        foreach (Vector2 node in followingPath)
        {
            Gizmos.color = Color.green;
            if (node == followingPath[indexPath])
            {
                Gizmos.color = Color.yellow;
            }

            Gizmos.DrawWireSphere(node, 0.1f);
        }
    }
}
