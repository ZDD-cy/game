using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freezetrap : MonoBehaviour
{
    [SerializeField]
    [Header("冰冻陷阱配置")]
    public float slowDownRate = 0.6f;
    public float damagePerSec = 2f;
    public float freezeDuration = 3f;
    public float trapCD = 1f;

    private float _cdTimer;
    private bool _isTrapActive = true;

    void Update()
    {
        if (!_isTrapActive)
        {
            _cdTimer += Time.deltaTime;
            if (_cdTimer >= trapCD)
            {
                _isTrapActive = true;
                _cdTimer = 0;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("陷阱触发检测：进入 OnTriggerEnter2D 方法");
        if (other.CompareTag("player") && _isTrapActive)
        {
            Debug.Log("陷阱触发：检测到 Player 标签，且陷阱处于激活状态");
            Player player = other.GetComponent<Player>();
            if (player != null && !player.isFrozen)
            {
                Debug.Log("陷阱触发：成功获取 Player 组件，开始施加冰冻效果");
                player.isInIceTrap = true;
                player.isFrozen = true;
                player.currentSpeed = player.moveSpeed * (1 - slowDownRate);

                StartCoroutine(playerTakeDamage(player));
                _isTrapActive = false;
            }
            else
            {
                if (player == null)
                {
                    Debug.LogError("陷阱触发：未找到 Player 组件");
                }
                if (player != null && player.isFrozen)
                {
                    Debug.Log("陷阱触发：玩家已处于冻结状态，无法重复触发");
                }
            }
        }
        else
        {
            if (!other.CompareTag("player"))
            {
                Debug.Log("陷阱触发：触发对象标签不是 player，标签为：" + other.tag);
            }
            if (!_isTrapActive)
            {
                Debug.Log("陷阱触发：陷阱处于冷却状态，无法触发");
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("陷阱持续触发：保持玩家冻结和减速状态");
                player.isFrozen = true;
                player.currentSpeed = player.moveSpeed * (1 - slowDownRate);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("陷阱触发结束：玩家离开陷阱，开始恢复计时");
                player.isInIceTrap = false;
                Invoke(nameof(RecoverplayerSpeed), freezeDuration);
            }
        }
    }

    void RecoverplayerSpeed()
    {
        Player player = GameObject.FindGameObjectWithTag("player")?.GetComponent<Player>();
        if (player != null && !player.isInIceTrap)
        {
            Debug.Log("陷阱效果恢复：玩家状态恢复正常");
            player.isFrozen = false;
            player.currentSpeed = player.moveSpeed;
        }
    }

    IEnumerator playerTakeDamage(Player player)
    {
        Debug.Log("陷阱伤害开始：启动持续掉血协程");
        float damageTimer = 0;
        while (player.isInIceTrap || player.isFrozen)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= 1f)
            {
                player.hp -= damagePerSec;
                Debug.Log("陷阱伤害：玩家当前生命值为 " + player.hp);
                damageTimer = 0;
                if (player.hp <= 0)
                {
                    Debug.Log("陷阱伤害：玩家生命值耗尽，解除冻结");
                    player.isFrozen = false;
                    player.currentSpeed = player.moveSpeed;
                    yield break;
                }
            }
            yield return null;
        }
        Debug.Log("陷阱伤害结束：协程退出");
    }
}
