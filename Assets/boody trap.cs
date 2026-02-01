using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class boodytrap : MonoBehaviour
{ 
    [Header("¬‰ Øœ›⁄Â≈‰÷√")]
    public GameObject rockPrefab;       
    public Transform spawnPoint;        
    public float fallDelay = 0.5f;     
    public int damage = 5;             
    public bool isReusable = true;      
    public float resetTime = 3f;        
    public float destroyTime = 2f;      

    private GameObject currentRock;
    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isTriggered)
        {
            isTriggered = true;
            Invoke(nameof(SpawnRock), fallDelay);
        }
    }

    void SpawnRock()
    {
        currentRock = Instantiate(rockPrefab, spawnPoint.position, Quaternion.identity);
        Rock rock = currentRock.GetComponent<Rock>();
        if (rock != null)
        {
            rock.SetDamage(damage);
            rock.OnRockLanded += OnRockLanded;
        }
    }

    void OnRockLanded()
    {
        if (isReusable)
        {
            Invoke(nameof(ResetTrap), resetTime);
        }
        else
        {
            Destroy(currentRock, destroyTime);
            Destroy(gameObject);
        }
    }

    void ResetTrap()
    {
        Destroy(currentRock);
        isTriggered = false;
    }
}



