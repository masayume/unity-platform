using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    bool isInvincible; // flag for enemies that don't take damage
    public int currentHealth;
    public int maxHealth = 1;
    public int contactDamage = 1;



    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;   
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
        }
    }

    void Defeat()
    {
        Destroy(gameObject);
    }

    // track player continuos collide with enemy
    private void OnTriggerStay2D(Collider2D other) { 
        if (other.gameObject.CompareTag("Player"))
        {
            // Debug.Log("Player Hit");
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            player.HitSide(transform.position.x > player.transform.position.x);
            player.TakeDamage(this.contactDamage);
        }
    }



}
