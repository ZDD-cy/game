using UnityEngine;

public class DamageZone : MonoBehaviour
{
    public int damage = 2;
    public float damageInterval = 0.5f;
    private float damageTimer;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                other.GetComponent<PlayerController>().TakeDamage(damage);
                damageTimer = 0;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) damageTimer = 0;
    }
}