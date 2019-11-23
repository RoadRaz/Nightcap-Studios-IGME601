﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{

    public GameObject bounds; // contains a reference to the arenabounds object
    public float spawnHeight; // set the spawn height in the inspector

    private Vector3 lastRespawnCenter = Vector3.zero; // used for debugging

    public bool debug = false;

    public List<GameObject> players;

    // Start is called before the first frame update
    void Start()
    {
        lastRespawnCenter = bounds.GetComponent<ArenaBounds>().center;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Returns a spawn location based on the position of the other player
    /// </summary>
    /// <param name="opp">Reference to the oppoenent GameObject</param>
    /// <returns>Spawn location</returns>
    public Vector3 FindSpawnPoint(GameObject opp)
    {
        players = GameObject.Find("Manager").GetComponent<Manager>().players;
        ArenaBounds abounds = bounds.GetComponent<ArenaBounds>();
        Vector3 result = Vector3.zero;

        Vector3 quadCenter = abounds.center;
        Vector3 quadSize = (abounds.PointA - abounds.PointB) / 4;

        int[] quadCounts = { 0, 0, 0, 0 };

        foreach(GameObject p in players)
        {
            int oppQuad = FindQuadrant(p);
            //Debug.Log(oppQuad);

            //set spawn quad to be 2
            if (oppQuad == 0)
            {
                //quadCenter = abounds.quad2;
                quadCounts[0]++;
            }
            else if (oppQuad == 1) // 3
            {
                //quadCenter = abounds.quad3;
                quadCounts[1]++;

            }
            else if (oppQuad == 2) // 0
            {
                //quadCenter = abounds.quad0;
                quadCounts[2]++;

            }
            else if (oppQuad == 3) // 1
            {
                //quadCenter = abounds.quad1;
                quadCounts[3]++;

            }
        }

        if(debug) Debug.Log(quadCounts[0] + " " + quadCounts[1] + " " + quadCounts[2] + " " + quadCounts[3]);

        bool foundSpawn = false;
        int least = 10;
        int leastQuad = 0;

        for(int i = 0; i < 4; i++)
        {
            if(quadCounts[i] <= least)
            {
                least = quadCounts[i];
                leastQuad = i;
            }

            if(quadCounts[i] == 0)
            {
                foundSpawn = true;
                leastQuad = i;
                switch (i)
                {
                    case 0: quadCenter = abounds.quad2;break;
                    case 1: quadCenter = abounds.quad3;break;
                    case 2: quadCenter = abounds.quad0;break;
                    case 3: quadCenter = abounds.quad1;break;
                    default: quadCenter = abounds.quad0;break;
                }
            }
        }

        if (!foundSpawn)
        {
            switch (leastQuad)
            {
                case 0: quadCenter = abounds.quad2; break;
                case 1: quadCenter = abounds.quad3; break;
                case 2: quadCenter = abounds.quad0; break;
                case 3: quadCenter = abounds.quad1; break;
                default: quadCenter = abounds.quad0; break;
            }
        }

        if(debug) Debug.Log("Spawn Quad: " + leastQuad);

        float rX = Random.Range(-quadSize.x/2, quadSize.x/2);
        float rZ = Random.Range(-quadSize.z/2, quadSize.z/2);

        result.x = quadCenter.x + rX;
        result.z = quadCenter.z + rZ;
        result.y = spawnHeight;

        lastRespawnCenter = quadCenter;

        return result;
    }

    /// <summary>
    /// Finds the quadrant of the arena the opponent is in
    /// </summary>
    /// <param name="opp">Reference to the opponent GameObject</param>
    /// <returns>Quadrant index</returns>
    int FindQuadrant(GameObject opp)
    {
        /*
                     Z
                     ^
                     |
                1    |    0
                     |
           -X<-------C------->X  
                     |
                2    |    3
                     |
                     v
                    -Z
         
         
         */


        bool top = false;
        bool left = false;

        if(opp.transform.position.x > bounds.GetComponent<ArenaBounds>().center.x)
        {
            left = true;
        }

        if (opp.transform.position.z < bounds.GetComponent<ArenaBounds>().center.z)
        {
            top = true;
        }

        if(top && !left)
        {
            return 0;
        }
        else if(top && left)
        {
            return 1;
        }
        else if (!top && left)
        {
            return 2;
        }
        else if (!top && !left)
        {
            return 3;
        }


        return -1;
    }

    void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(lastRespawnCenter, 1);
    }
}

/*public class QuadTree
{
    public QuadTree parent;
    public QuadTree[] children = new QuadTree[4];
    public Vector2 center;
    public Vector2 size;
    public BoxCollider col;

    public QuadTree(Vector2 _center, Vector2 _size, QuadTree _parent=null)
    {
        center = _center;
        size = _size;
        parent = _parent;
        col = new BoxCollider();
        col.center = new Vector3(center.x, 0, center.y);
        col.size = new Vector3(size.x, 10, size.y);
        col.isTrigger = true;
    }

    public void Subdivide()
    {
        GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(center.x, 0, center.y), new Vector3(size.x, 0, size.y));
    }
}*/