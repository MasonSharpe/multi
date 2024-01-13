using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    public static PlayerMovement instance;
    public float speed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    public float rocketTimer = 0;
    private float blastTimer = 0;
    private float rocketVerticalVelocity;
    public CircleCollider2D blast;


    private Rigidbody2D rb;
    private bool isGrounded;

    private NetworkVariable<bool> blastActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> blastPower = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);



    private void Awake() {
       instance = this;
    }
    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {

        blast.enabled = blastActive.Value;
        blast.radius = blastPower.Value / 5f + 1;

        if (!IsOwner) return;
        // Check if the character is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        if (isGrounded || Mathf.Abs(rocketVerticalVelocity) < 0.2f) {
            rocketVerticalVelocity = 0;
        } else {
           if (rocketVerticalVelocity < 0) {
                rocketVerticalVelocity += 10 * Time.deltaTime;
            } else {
                rocketVerticalVelocity -= 10 * Time.deltaTime;
            }
        }

        // Handle player input for movement
        rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed + rocketVerticalVelocity, rb.velocity.y);
        

        // Handle player input for jumping
        if (isGrounded && Input.GetButtonDown("Jump")) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        blastTimer -= Time.deltaTime;
        rocketTimer = 10;//Mathf.Clamp(rocketTimer + Time.deltaTime, -3, 20);

        if (Input.GetKeyDown(KeyCode.LeftShift) && rocketTimer >= 0) {
            Vector2 vector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            rb.velocity += vector.y * (rocketTimer * 1) * Vector2.up;
            rocketVerticalVelocity = vector.x * (rocketTimer * 1);
            rocketTimer = -3;

            blastActive.Value = true;
            blastPower.Value = rocketTimer;
            blastTimer = 0.2f;
        }

        if (blastActive.Value && blastTimer < 0) {
            blastActive.Value = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Vector2 center = collision.transform.position;
        Vector2 vector = (Vector2)transform.position - center;

        rb.velocity += vector * collision.GetComponent<CircleCollider2D>().radius * 5;
    }
}
