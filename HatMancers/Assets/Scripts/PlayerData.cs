﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Contains all the Player gameplay data
/// Authors: Nate, David, Michael
/// </summary>
public class PlayerData : MonoBehaviour
{
    // Contains data about the player's current hat/magic, spawn information, etc.
    [SerializeField]
    private GameObject currHat;

    public string currMagic = "none";

    public int maxHealth = 100;
    public int health = 100;

    public GameObject spawn;
    private float spawnTimer = 0;
    public float spawnTime = 3;
    public bool dead = false;
    private Rigidbody body;

    public float killPlaneDepth = -50;

    //public GameObject opponent;
    //public PlayerData opponentData;
    public int score;

    public bool testDummy = false;

    private PlayerController pCtrl;
    private CapsuleCollider cCollider;

    [SerializeField]
    private GameObject hatPosition;

    public RectTransform fireCrosshair;
    public RectTransform lightningCrosshair;

    public bool debug = false;

    // Private Scripts
    private GameObject lastDamageDealer = null;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        // Dummies have certain components removed since they are not needed
        if (testDummy)
        {
            Destroy(GetComponentInChildren<Camera>());
            Destroy(GetComponentInChildren<PlayerController>());
        }

        pCtrl = GetComponent<PlayerController>();
        cCollider = GetComponent<CapsuleCollider>();

        /*if(pCtrl.playerNum == 1 || pCtrl.playerNum == 3)
        {
            opponent = GameObject.Find("Player2");
        }
        else if (pCtrl.playerNum == 2)
        {
            opponent = GameObject.Find("Player1");
        }
        opponentData = opponent.GetComponent<PlayerData>();*/

        Transform[] children = GetComponentsInChildren<Transform>();
        foreach(Transform g in children)
        {
            if(g.gameObject.name == "HatLocation")
            {
                hatPosition = g.gameObject;
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (debug)
        {
            // Allow devs to kill a player by pressing F1 while the player object is in debug mode
            if (Input.GetKeyDown(KeyCode.F1))
            {
                health = 0;
            }

            // Allow devs to decrease player health with F2 while in debug mode
            if (Input.GetKeyDown(KeyCode.F2))
            {
                health -= Mathf.RoundToInt((maxHealth * 0.05f));
            }
        }

        // Prevent player movement if they are dead
        pCtrl.canMove = !dead;
        cCollider.enabled = !dead;

        // If the player if dead, check if they can respawn
        if (dead)
        {
            Respawn();
        }
        else // Otherwise, do everything else
        {
            StickyHat();
            CheckKillPlane();
            // CheckHealth();
        }
    }

    /// <summary>
    /// Checks to see if the player falls below the map, and kills them if they do
    /// </summary>
    void CheckKillPlane()
    {
        if(transform.position.y <= killPlaneDepth)
        {
            health = 0;
        }
    }

    /// <summary>
    /// Updates the current magic type if the player replaces their hat
    /// </summary>
    void UpdateMagic()
    {
        if(currHat.tag == "FireHat")
        {
            currMagic = "fire";

            // Check for crosshairs
            fireCrosshair.gameObject.SetActive(true);
        }
        else if (currHat.tag == "LightningHat")
        {
            currMagic = "lightning";

            // Check for crosshairs
            lightningCrosshair.gameObject.SetActive(true);
        }
        else if (currHat.tag == "IceHat")
        {
            currMagic = "ice";
        }
    }

    /// <summary>
    /// Keeps the current hat attached to the player's head
    /// </summary>
    void StickyHat()
    {
        if (currHat == null) return;
        currHat.transform.position = hatPosition.transform.position;
        currHat.transform.rotation = hatPosition.transform.rotation;
    }

    /// <summary>
    /// Kills the player if their health falls to/below 0
    /// </summary>
    /*void CheckHealth()
    {
        if(health <= 0)
        {
            dead = true;
            opponentData.IncrementScore();
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        }
    }*/

    /// <summary>
    /// Reports the the player is dead, specifically by setting the "dead" boolean + turning off the mesh renderer
    /// </summary>
    void Die()
    {
        dead = true;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
    }

    /// <summary>
    /// If the player is dead, respawn the player after a set amount of time
    /// </summary>
    void Respawn()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnTime)
        {
            dead = false;
            GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
            //transform.position = spawn.transform.position;
            health = maxHealth;
            spawnTimer = 0;

            //transform.position = GameObject.Find("Manager").GetComponent<RespawnManager>().FindSpawnPoint(opponent);
            transform.position = GameObject.Find("Manager").GetComponent<RespawnManager>().FindSpawnPoint(lastDamageDealer);
        }
    }

    /// <summary>
    /// Processes a damaging object attacking the player.
    /// </summary>
    /// <param name="collider"></param>
    public void ProcessDamage(GameObject collider)
    {
        // Getting the components that are part of the collider
        SpellData spell = collider.gameObject.GetComponent<SpellData>();

        // IF the colliding object has a SpellData instance...
        if (spell != null)
        {
            //body.velocity = new Vector3(0, 40, 0);
            lastDamageDealer = spell.origin;
            health -= spell.damage;
            if (health > 0)
            {
                StartCoroutine("DoBlink");
            }
                if (health < 1 && !dead)
            {
                Die();
                lastDamageDealer.GetComponent<PlayerData>().IncrementScore();
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        // Updates the current hat if one is picked up
        if (col.gameObject.tag.Contains("Hat"))
        {
            if(currHat != null)
            {
                Destroy(currHat);

                // Deactivate crosshairs
                fireCrosshair.gameObject.SetActive(false);
                lightningCrosshair.gameObject.SetActive(false);
            }
            currHat = col.gameObject;
            currHat.GetComponent<Collider>().enabled = false;
            UpdateMagic();

            GameObject.Find("Manager").GetComponent<HatSpawner>().unclaimedHatCount--;
        }
        else if(col.gameObject.tag == "Damage") // Damages the player if they are hit by a damaging source (denoted by the "Damage" tag)
        {
            if (col.gameObject.GetComponent<SpellData>().origin.name != gameObject.name)
            {
                //health -= col.gameObject.GetComponent<SpellData>().damage;
                ProcessDamage(col.gameObject);
            }
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Damage") // Damages the player if they are hit by a damaging source (denoted by the "Damage" tag)
        {
            if (col.gameObject.GetComponent<SpellData>().origin.name != gameObject.name)
            {
                //health -= col.gameObject.GetComponent<SpellData>().damage;
                ProcessDamage(col.gameObject);
            }
        }
    }

    IEnumerator DoBlink()
    {
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine("BlinkAnimation");
            yield return new WaitForSeconds(0.25f);
        }
    }

    IEnumerator BlinkAnimation()
    {
        DisableWizardModel();
        yield return new WaitForSeconds(0.125f);
        EnableWizardModel();
    }

    void DisableWizardModel()
    {
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if(renderer.enabled)
            renderer.enabled = false;
        }
    }

    void EnableWizardModel()
    {
        SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if(!renderer.enabled)
            renderer.enabled = true;
        }
    }

    /// <summary>
    /// Returns the Score of the player.
    /// </summary>
    public int GetScore()
    {
        return score;
    }

    /// <summary>
    /// Adds 1 to the Player's score.
    /// </summary>
    public void IncrementScore()
    {
        score++;
    }
}
