using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    
    public List<Waypoint> neighbors;
    public Waypoint previous { get; set; }
    public float distance { get; set; }

    private void OnDrawGizmos()
    {
        if (neighbors == null)
        {
            return;
        }
        
        Gizmos.color = new Color(0f,0f,0f);
        
        foreach (Waypoint neighbour in neighbors)
        {
            if (neighbour != null)
            {
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
    }
}
