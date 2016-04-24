using UnityEngine;
using System.Collections;


public class AudioManager : MonoBehaviour {

    [Range(0,1)]
    public float MusicVolume = 1;
    [Range(0,1)]
    public float SFXVolume = 1;

    //Holds audio source instances for audio playback
    private AudioSource _musicPlaybackSource;
    private AudioSource _sfxPlaybackSource;


    
    void Awake()
    {
        //Setup audio sources
        _musicPlaybackSource = gameObject.AddComponent<AudioSource>();
        _musicPlaybackSource.playOnAwake = false;
        _musicPlaybackSource.loop = true;
        _musicPlaybackSource.volume = MusicVolume;
        _musicPlaybackSource.clip = null;
        _musicPlaybackSource.mute = true;


        _sfxPlaybackSource = gameObject.AddComponent<AudioSource>();
        _sfxPlaybackSource.playOnAwake = false;
        _sfxPlaybackSource.loop = false;
        _sfxPlaybackSource.volume = SFXVolume;
    }


    public void PlaySFXSound(AudioClip Clip)
    {
        if (Clip != null)
            _sfxPlaybackSource.PlayOneShot(Clip);
    }

    public void PlayMusic(AudioClip Clip)
    {
        if (Clip != null)
        {
            _musicPlaybackSource.clip = Clip;
            _musicPlaybackSource.Play();
        }
    }



    public void ToggleSFX(bool value)
    {
        _sfxPlaybackSource.mute = !value;
    }

    public void ToggleMusic(bool value)
    {
        _musicPlaybackSource.mute = !value;
    }

}
