using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// all enemies will require these components
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class EnemyController : MonoBehaviour
{
    Animator animator;
    BoxCollider2D box2d;
    Rigidbody2D rb2d;
    SpriteRenderer sprite;

    bool isInvincible;

    GameObject explodeEffect;

    RigidbodyConstraints2D rb2dConstraints;

    public bool freezeEnemy;

    public int scorePoints = 500;
    public int currentHealth;
    public int maxHealth = 1;
    public int contactDamage = 1;
    public int explosionDamage = 0;
    public int bulletDamage = 1;
    public float bulletSpeed = 3f;

    [Header("Bonus Item Settings")]
    public ItemScript.ItemTypes bonusItemType;
    public ItemScript.BonusBallColors bonusBallColor;
    public ItemScript.WeaponPartColors weaponPartColor;
    public float bonusDestroyDelay = 5f;
    public Vector2 bonusVelocity = new Vector2(0, 3f);

    [Header("Audio Clips")]
    public AudioClip damageClip;
    public AudioClip blockAttackClip;
    public AudioClip shootBulletClip;
    public AudioClip energyFillClip;

    [Header("Positions and Prefabs")]
    public GameObject bulletShootPos;
    public GameObject bulletPrefab;
    public GameObject explodeEffectPrefab;

    void Awake()
    {
        // get handles to components
        animator = GetComponent<Animator>();
        box2d = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // start at full health
        currentHealth = maxHealth;
    }

    public void Flip()
    {
        transform.Rotate(0, 180f, 0);
    }

    public void Invincible(bool invincibility)
    {
        isInvincible = invincibility;
    }

    public void TakeDamage(int damage)
    {
        // take damage if not invincible
        if (!isInvincible)
        {
            // take damage amount from health and call defeat if no health
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            SoundManager.Instance.Play(damageClip);
            if (currentHealth <= 0)
            {
                Defeat();
            }
        }
        else
        {
            // block attack sound - dink!
            SoundManager.Instance.Play(blockAttackClip);
        }
    }

    void StartDefeatAnimation()
    {
        // play explosion animation
        //   create copy of prefab, place its spawn location at center of sprite, 
        //   set explosion damage value (if any), destroy after two seconds
        explodeEffect = Instantiate(explodeEffectPrefab);
        explodeEffect.name = explodeEffectPrefab.name;
        explodeEffect.transform.position = sprite.bounds.center;
        explodeEffect.GetComponent<ExplosionScript>().SetDamageValue(this.explosionDamage);
        Destroy(explodeEffect, 2f);

        // get the bonus item prefab
        GameObject bonusItemPrefab = GameManager.Instance.GetBonusItem(bonusItemType);
        if (bonusItemPrefab != null)
        {
            // instantiate the bonus item
            GameObject bonusItem = Instantiate(bonusItemPrefab);
            bonusItem.name = bonusItemPrefab.name;
            bonusItem.transform.position = explodeEffect.transform.position;
            bonusItem.GetComponent<ItemScript>().Animate(true);
            bonusItem.GetComponent<ItemScript>().SetDestroyDelay(bonusDestroyDelay);
            bonusItem.GetComponent<ItemScript>().SetBonusBallColor(bonusBallColor);
            bonusItem.GetComponent<ItemScript>().SetWeaponPartColor(weaponPartColor);
            // give the bonus item a bounce effect
            bonusItem.GetComponent<Rigidbody2D>().velocity = bonusVelocity;
        }
    }

    void StopDefeatAnimation()
    {
        // we have this function in case we want to remove the explosion before it finishes
        Destroy(explodeEffect);
    }

    void Defeat()
    {
        // play explosion animation, remove enemy, give player score points
        StartDefeatAnimation();
        Destroy(gameObject);
        GameManager.Instance.AddScorePoints(this.scorePoints);
    }

    public void FreezeEnemy(bool freeze)
    {
        // freeze/unfreeze the enemy on screen
        // zero animation speed and freeze XYZ rigidbody constraints
        // NOTE: this will be called from the GameManager but could be used in other scripts
        if (freeze)
        {
            freezeEnemy = true;
            animator.speed = 0;
            rb2dConstraints = rb2d.constraints;
            rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
            freezeEnemy = false;
            animator.speed = 1;
            rb2d.constraints = rb2dConstraints;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // check for collision with player
        if (other.gameObject.CompareTag("Player"))
        {
            // colliding with player inflicts damage and takes contact damage away from health
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.HitSide(transform.position.x > player.transform.position.x);
            player.TakeDamage(this.contactDamage);
        }
    }
}