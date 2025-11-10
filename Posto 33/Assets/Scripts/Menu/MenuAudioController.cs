using UnityEngine;

public class MenuAudioController : MonoBehaviour
{
    public AudioSource music;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){

    }
    public void ChangeVolume(float volume){
        PlayerPrefs.SetFloat("volume", volume);
        music.volume = volume;
    }
    // Update is called once per frame
    void Update(){

    }
}
