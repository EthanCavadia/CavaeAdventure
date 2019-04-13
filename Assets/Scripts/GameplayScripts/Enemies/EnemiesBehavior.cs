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
        FOLLOW
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
    [SerializeField] private int _detectRange;
    [SerializeField] private int _minRange;
    [SerializeField] private Collider2D attackCollider;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        followingPath = new List<Vector2>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void FixedUpdate()
    {

        Collider2D collider = Physics2D.OverlapCircle(transform.position, _detectRange);
        
        if (timer > 10)
        {
            timer = 0;
            
            FindPlayer();
            FollowPath();
        }

        timer += Time.deltaTime;
    }

    public void FindPlayer()
    {
        if (followingPath != Pathfinding.instance.Astar(transform.position))
        {
            followingPath = Pathfinding.instance.Astar(transform.position);
            indexPath = 1;
        }
    }

    public void FollowPath()
    {

        if (indexPath >= followingPath.Count)
        {
            _rigidbody2D.velocity = Vector2.zero;
            return;
        }

        _rigidbody2D.velocity = followingPath[indexPath] - (Vector2) transform.position;
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
            attackCollider.enabled = true;
        }

        timer += Time.deltaTime;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _detectRange);
        Gizmos.DrawWireSphere(transform.position, _minRange);
    }
}
