using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在每个场景的「切换区」上
public class Sceneswitcher : MonoBehaviour
{
    private static readonly int Fadestart = Animator.StringToHash("Fadestart");
    public Animator transition;
    [Header("目标场景名字（和文件名一致）")]
    public string Targetscene = "game.2.1";
    public float switchlatency;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("player"))
        {
            StartCoroutine(Loadlevel());
        }
    }
    
    public void MessageSwitch() {
        StartCoroutine(Loadlevel());
    }
    
    IEnumerator Loadlevel()
    {
        transition.SetTrigger(Fadestart);
        yield return new WaitForSeconds(switchlatency);
        SceneManager.LoadScene(Targetscene);
    }
}