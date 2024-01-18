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
    private float footstepTimer = -1f;
    private float jumpSoundTimer = -1;
    private float rotatedTimer = -1;
    private Vector2 rotateVector = new();
    public Vector2 platformAdder = new();

    public float timeInRound = 0;

    public ParticleSystem particles;
    public Rigidbody2D rb;
    private bool isGrounded;
    private PlayerNetwork player;
    public Animator animator;

    private NetworkVariable<bool> blastActive = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<float> blastPower = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<float> rocketTimer = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);




    void Start() {
        player = GetComponent<PlayerNetwork>();

    }

    void Update() {

        blast.enabled = blastActive.Value;
        blast.size = new Vector2((blastPower.Value / 5f) + 1, (blastPower.Value / 5f) + 2);

        if (!IsOwner || player.spectating) return;

        bool check = Physics2D.OverlapArea(new Vector2(groundCheck.position.x - 0.25f, groundCheck.position.y - 0.1f), new Vector2(groundCheck.position.x + 0.25f, groundCheck.position.y + 0.1f), groundLayer);

        if (check) isCoyote = false;
        if ((isGrounded && !check && !Input.GetButton("Jump")) || isCoyote) {
            if (!isCoyote) coyoteTime = 0.15f; isCoyote = true;
            coyoteTime -= Time.deltaTime;
            check = coyoteTime > 0;
            if (!check || Input.GetButton("Jump")) isCoyote = false;
        }
        if (!isGrounded && check)
        {
            player.killer.Value = -1;
            animator.SetTrigger("justGrounded");
            animator.SetBool("grounded", true);
        }
        if (isGrounded && !check)
        {
            animator.SetTrigger("justLeftGround");
            animator.SetBool("grounded", false);
        }
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

        float x = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(x * speed + rocketHorizontalVelocity, rb.velocity.y) + platformAdder;
        animator.SetFloat("x", x);
        if (x != 0 && isGrounded)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer < 0)
            {
                player.source.PlayOneShot(player.clips[4], 0.1f);
                footstepTimer = 0.5f;
            }
        }

        if (isGrounded && Input.GetButton("Jump")) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            if (jumpSoundTimer < 0)
            {
                player.source.PlayOneShot(player.clips[6], 0.5f);
                jumpSoundTimer = 0.5f;
            }

        }

        invincibilityTimer -= Time.deltaTime;
        blastTimer -= Time.deltaTime;
        timeInRound += Time.deltaTime;
        cameraOffsetTimer -= Time.deltaTime;
        jumpSoundTimer -= Time.deltaTime;
        rotatedTimer -= Time.deltaTime;

        if (timeInRound > 5 || SceneManager.GetActiveScene().name == "Lobby Scene") rocketTimer.Value = Mathf.Clamp(rocketTimer.Value + Time.deltaTime * 4, -3, 20);

        if (Input.GetKeyDown(KeyCode.LeftShift) && rocketTimer.Value >= 0) {
            Vector2 vector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (vector == Vector2.zero) vector = Vector2.up;
            rb.velocity = (vector.y * (rocketTimer.Value * 0.9f) + (rb.velocity.y / 2)) * Vector2.up;
            rocketHorizontalVelocity = vector.x * rocketTimer.Value * 0.9f;

            blastActive.Value = true;
            blastPower.Value = rocketTimer.Value;
            blastTimer = 0.2f;

            cameraOffsetTimer = 0.4f;
            cameraShakePower = blastPower.Value / 16f;

            rocketTimer.Value = -3;

            player.source.PlayOneShot(player.clips[0], 0.7f);
            if (blastPower.Value > 3) {
                animator.SetTrigger("justBlasted");
                rotatedTimer = 1;
                rotateVector = vector.normalized;
                particles.Play();

            }



        }
        Vector2 rotation = rotatedTimer > 0 ? rotateVector : Vector2.zero;
        player.sprite1.transform.up = rotation;
        player.sprite2.transform.up = rotation;
        player.sprite3.transform.up = rotation;
        particles.transform.parent.up = rotation;

        if (blastActive.Value && blastTimer < 0) {
            blastActive.Value = false;
        }

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
            int dealer = (int)collision.transform.parent.GetComponent<PlayerNetwork>().OwnerClientId;
            player.killer.Value = dealer;
            player.sceneManagement.PlayPingServerRpc(dealer);
            cameraOffsetTimer = 0.4f;
            cameraShakePower = power / 16f;

            player.source.PlayOneShot(player.clips[1], 0.7f);
            animator.SetTrigger("justGotBlasted");
            rotatedTimer = 1;
            rotateVector = vector.normalized;
        }



    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 12)
        {
            Vector2 vector2 = collision.transform.parent.GetComponent<Rigidbody2D>().velocity;
            platformAdder = vector2;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 12)
        {
            platformAdder = Vector2.zero;
        }
    }


}
