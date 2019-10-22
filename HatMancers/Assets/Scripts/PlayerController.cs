﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles all inputs from the player. Currently consists of:
///     - Movement
///     - Jumping
/// Authors: Abhi, David
/// Code Source: https://forum.unity.com/threads/proper-velocity-based-movement-101.462598/
/// </summary>
public class PlayerController : MonoBehaviour
{
    // Modifiable Attributes
    public float speed;
    public float rotationSpeed;
    public float jumpForce;

    // Rigidbody for physics
    private Rigidbody body;

    // Stored values for axes
    private float vertical;
    private float horizontal;

    // Flags
    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get axes values
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        // Check if player wants to jump and can jump
        if (Input.GetAxis("Jump") > 0 && grounded)
        {
            body.AddForce(transform.up * jumpForce);
        }

        // Create own gravity for rigidbody
        Vector3 velocity = (transform.forward * vertical) * speed * Time.fixedDeltaTime;
        velocity.y = body.velocity.y;
        body.velocity = velocity;

        // Rotate player
        transform.Rotate((transform.up * horizontal) * rotationSpeed * Time.fixedDeltaTime);
    }

    // Check for when a collider hits this game object
    void OnCollisionEnter (Collision collision)
    {
        // Check if hitting the ground (Layer 8)
        if (collision.gameObject.layer == 8)
        {
            grounded = true;
        }
    }

    // Check for when a collider no longer overlaps this game object
    void OnCollisionExit (Collision collision)
    {
        // Check if leaving the ground (Layer 8)
        if (collision.gameObject.layer == 8)
        {
            grounded = false;
        }
    }
}