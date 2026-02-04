using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamagePopup : MonoBehaviour
{
 [SerializeField] private TextMeshProUGUI damageText;
    public void SetDamage(int damage)
        {
          damageText.text = damage.ToString();
        }
}
