using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro textMesh;

    public void SetDamage(int damage)
    {
        textMesh.text = damage.ToString();
        Destroy(gameObject, 1f);
    }
}
