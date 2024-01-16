using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour {
    public float speed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    private float blastTimer = 0;
    private float coyoteTime = 0;
    private bool isCoyote = true;
    private float invincibilityTimer = 0;
    public float rocketHorizontalVelocity;
    public CapsuleCollider2D blast;
    public Transform cameraOffset;
    private float cameraOffsetTimer = 0;
    public float cameraShakePower = 0;

    public float timeInRound = 0;


    public Rigidbody2D rb;
    private bool isGrounded;
    private PlayerNetwork player;

    private NetworkVariable<bool> blastActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> blastPower = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> rocketTimer = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);




    void Start() {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<PlayerNetwork>();

    }

    void Update() {

        blast.enabled = blastActive.Value;
        blast.size = new Vector2((blastPower.Value / 5f) + 1, (blastPower.Value / 5f) + 2);

        if (!IsOwner || player.spectating) return;
        // Check if the character is grounded
        bool check = Physics2D.OverlapArea(new Vector2(groundCheck.position.x - 0.25f, groundCheck.position.y - 0.1f), new Vector2(groundCheck.position.x + 0.25f, groundCheck.position.y + 0.1f), groundLayer);

        if (check) isCoyote = false;
        if ((isGrounded && !check && !Input.GetButton("Jump")) || isCoyote) {
            if (!isCoyote) coyoteTime = 0.15f; isCoyote = true;
            coyoteTime -= Time.deltaTime;
            check = coyoteTime > 0;
            if (!check || Input.GetButton("Jump")) isCoyote = false;
        }
        if (!isGrounded && check) player.killer.Value = -1;
        isGrounded = check;

        if (Physics2D.OverlapBox(new Vector2(groundCheck.position.x, groundCheck.position.y + 1), Vector2.one * 1.1f, 0, groundLayer)) {
            rocketHorizontalVelocity = 0;
        }
        float frictionMult = isGrounded ? 2 : 1;
        if (Mathf.Abs(rocketHorizontalVelocity) < 0.2f) {
            rocketHorizontalVelocity = 0;
        } else {
           if (rocketHorizontalVelocity < 0) {
                rocketHorizontalVelocity += 10 * Time.deltaTime * frictionMult;
            } else {
                rocketHorizontalVelocity -= 10 * Time.deltaTime * frictionMult;
            }
        }

        // Handle player input for movement
        rb.velocity = new Vector2(Input.GetAxis("Horizontal") * speed + rocketHorizontalVelocity, rb.velocity.y);
        

        // Handle player input for jumping
        if (isGrounded && Input.GetButton("Jump")) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        invincibilityTimer -= Time.deltaTime;
        blastTimer -= Time.deltaTime;
        timeInRound += Time.deltaTime;
        cameraOffsetTimer -= Time.deltaTime;

        if (timeInRound > 5 || SceneManager.GetActiveScene().name == "Lobby Scene") rocketTimer.Value = Mathf.Clamp(rocketTimer.Value + Time.deltaTime * 4, -3, 20);

        if (Input.GetKeyDown(KeyCode.LeftShift) && rocketTimer.Value >= 0) {
            Vector2 vector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            rb.velocity = (vector.y * (rocketTimer.Value * 0.9f) + (rb.velocity.y / 2)) * Vector2.up;
            rocketHorizontalVelocity = vector.x * rocketTimer.Value * 0.9f;

            blastActive.Value = true;
            blastPower.Value = rocketTimer.Value;
            blastTimer = 0.2f;

            cameraOffsetTimer = 0.4f;
            cameraShakePower = blastPower.Value / 16f;

            rocketTimer.Value = -3;

        }

        if (blastActive.Value && blastTimer < 0) {
            blastActive.Value = false;
        }

        print(cameraOffsetTimer);
        if (cameraOffsetTimer > 0) {
            cameraOffset.transform.localPosition = (Vector3)Random.insideUnitCircle * cameraShakePower;
        } else {
            cameraOffset.transform.localPosition = Vector3.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.transform.parent != transform.parent && collision.transform.name == "BlastHitbox" && invincibilityTimer < 0 && !player.spectating) {
            Vector2 center = collision.transform.position;
            Vector2 vector = (Vector2)transform.position - center;
            float power = collision.GetComponent<CapsuleCollider2D>().size.x * 6;
            Vector2 force = vector.normalized * (Vector2.one / Mathf.Clamp(vector.magnitude, 1f, 10)) * power;
            rb.velocity += new Vector2(0, force.y);
            rocketHorizontalVelocity += force.x;

            invincibilityTimer = 0.2f;
            player.killer.Value = (int)collision.transform.parent.GetComponent<PlayerNetwork>().OwnerClientId;
            cameraOffsetTimer = 0.4f;
            cameraShakePower = power / 16f;
        }

    }
}
