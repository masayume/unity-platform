using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator; // to control animations
    BoxCollider2D box2d; // to control jump
    Rigidbody2D rb2d;
    SpriteRenderer sprite; // explosion from the center of the sprite

    float keyHorizontal;
    bool keyJump;
    bool keyShoot;
    bool isGrounded;
    bool isJumping;
    bool isShooting;
    bool isTakingDamage;
    bool isInvincible;
    bool isFacingRight; // default facing for static assets, must be initialized
    bool hitSideRight; // the side where player is hit

    bool freezeInput;
    bool freezePlayer;
    bool freezeBullets; // player related
    float shootTime;
    bool keyShootRelease;

    RigidbodyConstraints2D rb2dConstraints;

    public int currentHealth;
    public int maxHealth = 28;

    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float jumpSpeed = 5f;

    [SerializeField] int bulletDamage = 1;

    [SerializeField] float bulletSpeed = 5;

    [SerializeField] Transform bulletShootPos;

    [SerializeField] GameObject bulletPrefab;

    [SerializeField] AudioClip jumpLandedClip;
    [SerializeField] AudioClip shootBulletClip;
    [SerializeField] AudioClip takingDamageClip;
    [SerializeField] AudioClip explodeEffectClip;

    [SerializeField] GameObject explodeEffectPrefab;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        box2d = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();

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
            if (isJumping)
            {
                SoundManager.Instance.Play(jumpLandedClip);
                isJumping = false;
            }
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

        PlayerDebugInput();
        PlayerDirectionInput();
        PlayerJumpInput();
        PlayerShootinput();
        PlayerMovement();

    }

    void PlayerDebugInput()
    {
        // B for Bullets
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
            if (bullets.Length > 0)
            {
                freezeBullets = !freezeBullets;
                foreach (GameObject bullet in bullets)
                {
                    bullet.GetComponent<BulletScript>().FreezeBullet(freezeBullets);
                }
            }
            Debug.Log("Freeze Bullets: " + freezeBullets);
        }

        // E for Explosions
        if (Input.GetKeyDown(KeyCode.E))
        {
            Defeat();
            Debug.Log("Defeat");
        }

        // I for Invincible
        if (Input.GetKeyDown(KeyCode.I))
        {
            Invincible(!isInvincible);
            Debug.Log("Invincible: " + isInvincible);
        }

        // K for Keyboard
        if (Input.GetKeyDown(KeyCode.K))
        {
            FreezeInput(!freezeInput);
            Debug.Log("Freeze Input: " + freezeInput);
        }

        // P for Player
        if (Input.GetKeyDown(KeyCode.P))
        {
            FreezePlayer(!freezePlayer);
            Debug.Log("Freeze Player: " + freezePlayer);
        }
    }

    void PlayerDirectionInput()
    {
        if (!freezeInput)
        {
        keyHorizontal = Input.GetAxisRaw("Horizontal");

        }
    }

    void PlayerJumpInput()
    {
        // get keyboard input
        if (!freezeInput)
        {
            keyJump = Input.GetKeyDown(KeyCode.Space);
        }

        // rb2d.velocity = new Vector2(keyHorizontal * moveSpeed, rb2d.velocity.y);
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
            isJumping = true;
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
        float shootTimeLength = 0;
        float keyShootReleaseTimeLength = 0;

        // get keyboard input
        if (!freezeInput)
        {
            keyShoot = Input.GetKey(KeyCode.C);
        }

        // shoot key is being pressed and key release flag true
        if (keyShoot && keyShootRelease)
        {
            isShooting = true;
            keyShootRelease = false;
            shootTime = Time.time;
            // Shoot Bullet
            Invoke("ShootBullet", 0.1f);
        }
        // shoot key isn't being pressed and key release flag is false
        if (!keyShoot && !keyShootRelease)
        {
            keyShootReleaseTimeLength = Time.time - shootTime;
            keyShootRelease = true;
        }
        // while shooting limit its duration
        if (isShooting)
        {
            shootTimeLength = Time.time - shootTime;
            if (shootTimeLength >= 0.25f || keyShootReleaseTimeLength >= 0.15f)
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

        SoundManager.Instance.Play(shootBulletClip);
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
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
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
            Invincible(true);
            FreezeInput(true);
            float hitForceX = 0.50f;
            float hitForceY = 1.5f;
            if (hitSideRight) hitForceX = -hitForceX;
            rb2d.velocity = Vector2.zero;
            rb2d.AddForce(new Vector2(hitForceX, hitForceY), ForceMode2D.Impulse);

            SoundManager.Instance.Play(takingDamageClip);

        }
    }

    void StopDamageAnimation()
    {
        // this function is called at the end of the Hit animation
        // and we reset the animation because it doesn't loop otherwise
        // we can end up stuck in it
        isTakingDamage = false;
        Invincible(false);
        FreezeInput(false);
        animator.Play("Player_Hit", -1, 0f);

    }

    void StartDefeatAnimation()
    {
        FreezeInput(true);
        FreezePlayer(true);
        GameObject explodeEffect = Instantiate(explodeEffectPrefab);
        explodeEffect.name = explodeEffectPrefab.name;
        explodeEffect.transform.position = sprite.bounds.center;

        SoundManager.Instance.Play(explodeEffectClip);
        Destroy(gameObject);
    }

    void StopDefeatAnimation()
    {
        FreezeInput(false);
        FreezePlayer(false);
    }

    void Defeat()
    {
        GameManager.Instance.PlayerDefeated();
        Invoke("StartDefeatAnimation", 0.5f);
    }

    public void FreezeInput(bool freeze)
    {
        freezeInput = freeze;

    }

    public void FreezePlayer(bool freeze)
    {
         if (freeze)
        {
            freezePlayer = true;
            animator.speed = 0;
            rb2dConstraints = rb2d.constraints;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
            rb2d.velocity = Vector2.zero;
        }
        else
        {
            freezePlayer = false;
            animator.speed = 1;
            rb2d.constraints = rb2dConstraints;
            // ?? rb2d.velocity = bulletDirection * bulletSpeed;
        }         
    }


}
