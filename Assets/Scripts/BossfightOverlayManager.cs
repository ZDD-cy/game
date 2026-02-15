using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossfightOverlayManager : MonoBehaviour
{
    private static readonly int Fadestart = Animator.StringToHash("Fadestart");
    public Canvas PassCanvas;
    public Canvas FailCanvas;
    public Animator Passtransition;
    public Animator Failtransition;

    void Start()
    {
        if(PassCanvas != null)PassCanvas.enabled = false;
        FailCanvas.enabled = false;
    }

    public void PassFight()
    {
        if(PassCanvas != null)StartCoroutine(PassIEnu());
    }

    public void FailFight()
    {
        StartCoroutine(FailIEnu());
    }
    
    public void Restart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    IEnumerator PassIEnu()
    {
        PassCanvas.enabled = true;
        Passtransition.SetTrigger(Fadestart);
        yield return new WaitForSeconds(1);
    }
    IEnumerator FailIEnu()
    {
        FailCanvas.enabled = true;
        Failtransition.SetTrigger(Fadestart);
        yield return new WaitForSeconds(1);
    }
}
