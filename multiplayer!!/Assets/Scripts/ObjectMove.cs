using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour
{
    public float startTime;
    public Vector2 direction;

    private float timer = 0;
    private Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        timer += Time.deltaTime;

        if (timer > startTime) rb.velocity = direction;
    }
}
