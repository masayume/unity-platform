using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator; // to control animations
    BoxCollider2D box2d; // to control jump
    Rigidbody2D rb2d;


    float keyHorizontal;
    bool keyJump;
    bool keyShoot;
    bool isGrounded;
    bool isShooting;
    bool isTakingDamage;
    bool isInvincible;
    bool isFacingRight; // default facing for static assets, must be initialized
    bool hitSideRight; // the side where player is hit
    float shootTime;
    bool keyShootRelease;

    public int currentHealth;
    public int maxHealth = 28;

    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float jumpSpeed = 5f;

    [SerializeField] int bulletDamage = 1;

    [SerializeField] float bulletSpeed = 5;

    [SerializeField] Transform bulletShootPos;

    [SerializeField] GameObject bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        box2d = GetComponent<BoxCollider2D>();

        isFacingRight = true;

        currentHealth = maxHealth;
    }

    private void FixedUpdate() {
        isGrounded = false; // determine if touching the ground
        Color raycastColor;
        RaycastHit2D raycastHit;
        float raycastDistance = 0.05f;
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        // ground check
        Vector3 box_origin = box2d.bounds.center; 
        box_origin.y = box2d.bounds.min.y + (box2d.bounds.extents.y / 4f); 
        Vector3 box_size = box2d.bounds.size;
        box_size.y = box2d.bounds.size.y / 4f; // mini box unter the player
        // check if touching down layer
        raycastHit = Physics2D.BoxCast(box_origin, box_size, 0f, Vector2.down, raycastDistance, layerMask);
        if (raycastHit.collider != null)
        {
            isGrounded = true;
        }
        // raycast draw debug lines, visible in the Scene (green: touches; red: no touch)
        raycastColor = (isGrounded) ? Color.green : Color.red;
        Debug.DrawRay(box_origin + new Vector3(box2d.bounds.extents.x, 0), Vector2.down * (box2d.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(box_origin - new Vector3(box2d.bounds.extents.x, 0), Vector2.down * (box2d.bounds.extents.y / 4f + raycastDistance), raycastColor);
        Debug.DrawRay(box_origin - new Vector3(box2d.bounds.extents.x, box2d.bounds.extents.y / 4f + raycastDistance), Vector2.right * (box2d.bounds.extents.x * 2), raycastColor);

    }

    // Update is called once per frame
    void Update()
    {
        if (isTakingDamage)
        {
            animator.Play("Player_Hit");
            return;
        }

        PlayerDirectionInput();
        PlayerJumpInput();
        PlayerShootinput();
        PlayerMovement();

    }

    void PlayerJumpInput()
    {
        keyJump = Input.GetKeyDown(KeyCode.Space);

        isShooting = keyShoot;

        rb2d.velocity = new Vector2(keyHorizontal * moveSpeed, rb2d.velocity.y);
    }

    void PlayerDirectionInput()
    {
        keyHorizontal = Input.GetAxisRaw("Horizontal");


    }

    void PlayerMovement()
    {
        if (keyHorizontal < 0) // pressing LEFT
        {
            if (isFacingRight)
            {
                Flip();
            }
            if (isGrounded) 
            {
                if (isShooting)
                {
                    animator.Play("Player_RunShoot");
                }
                else 
                {
                    animator.Play("Player_Run");
                }
            }
            rb2d.velocity = new Vector2(-moveSpeed, rb2d.velocity.y);

        }
        else if (keyHorizontal > 0) // pressing RIGHT
        {
            if (!isFacingRight)
            {
                Flip();
            }
            if (isGrounded) 
            {
                if (isShooting)
                {
                    animator.Play("Player_RunShoot");
                }
                else 
                {
                    animator.Play("Player_Run");
                }
            }
            rb2d.velocity = new Vector2(moveSpeed, rb2d.velocity.y);
        }
        else // nothing pressed
        {
            if (isGrounded) 
            {
                if (isShooting)
                {
                    animator.Play("Player_Shoot");
                }
                else 
                {
                    animator.Play("Player_Idle");
                }
            }
            rb2d.velocity = new Vector2(0f, rb2d.velocity.y);

        }

        if (keyJump && isGrounded) // only jump if isGrounded
        {
            if (isShooting)
            {
                animator.Play("Player_Shoot");
            }
            else 
            {
                animator.Play("Player_Jump");
            }
            rb2d.velocity = new Vector2(rb2d.velocity.x, jumpSpeed);
        }

        if (!isGrounded) // falling
        {
            if (isShooting)
            {
                animator.Play("Player_JumpShoot");
            }
            else 
            {
                animator.Play("Player_Jump");
            }

        }

    }

    void PlayerShootinput()
    {
        float shootTimeLength;
        float keyShootReleaseTimeLength = 0;

        keyShoot = Input.GetKeyDown(KeyCode.C);

        if (keyShoot && keyShootRelease)
        {
            isShooting = true;
            keyShootRelease = false;
            shootTime = Time.time;
            
            ShootBullet(); // shoot Bullet

            // Invoke("ShootBullet", 0.1f); // delay shooting with coroutines

        }

        if (!keyShoot && !keyShootRelease)
        {
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }

        if (isShooting)
        {
            shootTimeLength = Time.time - shootTime;
            // how long the shooting happen for  and how long the key was pressed for
            if (shootTimeLength >= 0.25f || keyShootReleaseTimeLength >= 0.15)
            {
                isShooting = false;
            }
        }
    }


    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void ShootBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletShootPos.position, Quaternion.identity);
        bullet.GetComponent<BulletScript>().SetDamageValue(bulletDamage);
        bullet.GetComponent<BulletScript>().SetBulletSpeed(bulletSpeed);
        bullet.GetComponent<BulletScript>().SetBulletDirection((isFacingRight) ? Vector2.right : Vector2.left);
        bullet.GetComponent<BulletScript>().Shoot();
    }

    public void HitSide(bool rightSide)
    {
        hitSideRight = rightSide; 
    }

    public void Invincible(bool invincibility)
    {
        isInvincible = invincibility;
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;
            Mathf.Clamp(currentHealth, 0, maxHealth);
            if (currentHealth <= 0)
            {
                Defeat();
            }
            else
            {
                StartDamageAnimation();
            }
        }
    }

    void StartDamageAnimation()
    {
        if (!isTakingDamage)
        {
            isTakingDamage = true;
            isInvincible = true;
            float hitForceX = 0.50f;
            float hitForceY = 1.5f;
            if (hitSideRight) hitForceX = -hitForceX;
            rb2d.velocity = Vector2.zero;
            rb2d.AddForce(new Vector2(hitForceX, hitForceY), ForceMode2D.Impulse);

        }
    }

    void StopDamageAnimation()
    {
        isTakingDamage = false;
        isInvincible = false;
        animator.Play("Player_Hit", -1, 0f);

    }

    void Defeat()
    {
        Destroy(gameObject);
    }



}
