using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public float walkSpeed = 5.0f;

    private Stack<Vector2> currentPath;
    private Vector2 currentWaypointPosition;
    private float moveTimeTotal;
    private float moveTimeCurrent;

    private void Update()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            if (moveTimeCurrent < moveTimeTotal)
            {
                moveTimeCurrent += Time.deltaTime;
                if (moveTimeCurrent > moveTimeTotal)
                {
                    moveTimeCurrent = moveTimeTotal;
                }
                transform.position = Vector2.Lerp(currentWaypointPosition, currentPath.Peek(), moveTimeCurrent / moveTimeTotal);
            }
            else
            {
                currentWaypointPosition = currentPath.Pop();
                if (currentPath.Count == 0)
                {
                    Stop();
                }
                else
                {
                    moveTimeCurrent = 0;
                    moveTimeTotal = (currentWaypointPosition - currentPath.Peek()).magnitude / walkSpeed;
                }
            }
        }
    }

    public void NavigateTo(Vector2 destination)
    {
        currentPath = new Stack<Vector2>();
        Waypoint currentNode = FindClosestWaypoint(transform.position);
        Waypoint endNode = FindClosestWaypoint(destination);
        
        if (currentNode == null || endNode == null || currentNode == endNode)
        {
            return;
        }
        
        SortedList<float, Waypoint> openList = new SortedList<float, Waypoint>();
        List<Waypoint> closedList = new List<Waypoint>();
        openList.Add(0,currentNode);
        currentNode.previous = null;
        currentNode.distance = 0f;

        while (openList.Count > 0)
        {
            currentNode = openList.Values[0];
            openList.RemoveAt(0);
            float dist = currentNode.distance;
            closedList.Add(currentNode);
            if (currentNode == endNode)
            {
                break;
            }

            foreach (Waypoint neighbor in currentNode.neighbors)
            {
                if (closedList.Contains(neighbor) || openList.ContainsValue(neighbor)) continue;

                neighbor.previous = currentNode;
                neighbor.distance = dist + (neighbor.transform.position - currentNode.transform.position).magnitude;
                float distanceToTarget = (neighbor.transform.position - currentNode.transform.position).magnitude;
                openList.Add(neighbor.distance + distanceToTarget, neighbor);
            }
        }

        if (currentNode == endNode)
        {
            while (currentNode.previous != null)
            {
                currentPath.Push(currentNode.transform.position);
                currentNode = currentNode.previous;
            }
        }
    }

    public void Stop()
    {
        currentPath = null;
        moveTimeTotal = 0;
        moveTimeCurrent = 0;
    }

    private Waypoint FindClosestWaypoint(Vector2 target)
    {
        GameObject closest = null;
        float closestDist = Mathf.Infinity;

        foreach (GameObject waypoint in GameObject.FindGameObjectsWithTag("Player"))
        {
            float dist = ((Vector2)waypoint.transform.position - target).magnitude;
            if (dist < closestDist)
            {
                closest = waypoint;
                closestDist = dist;
            }
        }

        if (closest != null)
        {
            return closest.GetComponent<Waypoint>();
        }
        
        
        return null;
    }
}

