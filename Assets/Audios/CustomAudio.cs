using UnityEngine;

public class CustomAudio : MonoBehaviour
{
    public static void PlaySoundOnSceneLoaded(string CurrentScene)
    {
        string BGMid = CurrentScene switch
        {
            "game.0.0"=> "0",
            "game.0.1"=> "0",
            "game.0.2"=> "0",
            "game.1.0"=> "1",
            "game.1.1"=> "1",
            "game.1.2"=> "1",
            _ => "2",
        };
        AudioManager.Instance.PlayBGM(BGMid); 
    }
}
