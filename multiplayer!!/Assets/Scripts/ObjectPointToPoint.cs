using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPointToPoint : MonoBehaviour
{
    public float startTime;
    public Vector2 direction;
    public float cycleTime;

    private float timer = 0;
    private Rigidbody2D rb;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        timer += Time.deltaTime;

        int mult = (timer % cycleTime > cycleTime / 2) ? -1 : 1;
        if (timer > startTime) rb.velocity = direction * mult;
    }
}
