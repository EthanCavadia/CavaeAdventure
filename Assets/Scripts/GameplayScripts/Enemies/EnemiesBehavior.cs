using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesBehavior : MonoBehaviour
{
    public state currentState = state.IDLE;
    public subTag _subTag;

    public enum state
    {
        IDLE,
        ATTACK,
        FOLLOW,
        RETURN
    }

    public enum subTag
    {
        GOBLIN,
        GOLEM
    }

    private Rigidbody2D _rigidbody2D;
    private List<Vector2> followingPath;
    private int indexPath;
    private float timer = 0;
    private PathManager pathManager;
    private GameObject player;
    private Collider2D rangeCollider;
    private Vector3 startPos;
    
    [SerializeField] private Animator mouvmentAnimation;
    [SerializeField] private int _detectRange;
    [SerializeField] private int _minRange;
    [SerializeField] private Collider2D upCollider, downColider, leftCollider, rightCollider;
    public bool path;
    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        mouvmentAnimation = GetComponent<Animator>();
        followingPath = new List<Vector2>();
        player = GameObject.FindGameObjectWithTag("Player");

        startPos = transform.position;
    }

    private void Update()
    {

        Collider2D collider = Physics2D.OverlapCircle(transform.position, _detectRange);
        rangeCollider = Physics2D.OverlapCircle(transform.position, _detectRange);

        Vector3 mouvement = new Vector3(_rigidbody2D.velocity.x, _rigidbody2D.velocity.y, 0.0f);

        mouvmentAnimation.SetFloat("Horizontal", mouvement.x);
        mouvmentAnimation.SetFloat("Vertical", mouvement.y);
        mouvmentAnimation.SetFloat("Magnitude", mouvement.magnitude);

        if (collider.CompareTag("Player"))
        {
            currentState = state.FOLLOW;
        }
        else
        {
            if (transform.position != startPos)
            {
                //currentState = state.RETURN;
            }
            else
            {
                currentState = state.IDLE;
            }
        }

        
        
        timer += Time.deltaTime;

        switch (currentState)
        {
            case state.IDLE:
                //patrol random
                break;
            case state.FOLLOW:
                if (timer > 1)
                {
                    FindPlayer();
                    timer = 0;
                }
                FollowPath();
                break;
            case state.RETURN:
                //go back to pos
                break;
            case state.ATTACK:
                AttackPlayer();
                break;
        }
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

        _rigidbody2D.velocity = followingPath[indexPath] - (Vector2)transform.position;
        _rigidbody2D.velocity = _rigidbody2D.velocity.normalized * 2f;

        if (Vector2.Distance(transform.position, followingPath[indexPath]) < 0.1f)
        {
            indexPath++;
        }
    }

    public void AttackPlayer()
    {
        if (timer > 3)
        {
            //play animation
            //rangeCollider.enabled = true;
        }

        timer += Time.deltaTime;
    }

    private void RandomMove()
    {
        //TODO
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _detectRange);
        Gizmos.DrawWireSphere(transform.position, _minRange);
        
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
