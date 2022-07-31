using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable()]
public struct SoundParameters
{
    [Range(0, 1)]
    public float Volume;
    [Range(-3, 3)]
    public float Pitch;
    public bool Loop;
}
[System.Serializable()]

public class Sound
{
    [SerializeField] string name;
    public string Name { get { return name; } }

    [SerializeField] AudioClip clip;
    public AudioClip Clip { get { return clip; } }

    [SerializeField] SoundParameters parameters;
    public SoundParameters Parameters { get { return parameters; } }

    [HideInInspector]
    public AudioSource Source;

    public void Play()
    {
        Source.clip = Clip;

        Source.volume = Parameters.Volume;
        Source.pitch = Parameters.Pitch;
        Source.loop = Parameters.Loop;

        Source.Play();

    }
    public void Stop()
    {
        Source.Stop();
    }

}
public class AudioManager : MonoBehaviour
{

    public static AudioManager Instance;

    [SerializeField] Sound[] sounds;
    [SerializeField] AudioSource sourcePrefab;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        InitSounds();
    }

    void InitSounds()

    {
        foreach (var sound in sounds)
        {
            AudioSource source = (AudioSource)Instantiate(sourcePrefab, gameObject.transform);
            source.name = sound.Name;

            sound.Source = source;

        }
    }

    public void PlaySound(string name)
    {
        var sound = GetSound(name);
        if (sound != null)
        {
            sound.Play();
        }
        else
        {
            Debug.LogWarningFormat("Sound by the name {0} is not found. Issue occurred at AudioManager.PlaySound()", name);
        }
    }

    public void StopSound(string name)
    {
        var sound = GetSound(name);
        if (sound != null)
        {
            sound.Stop();
        }
        else
        {
            Debug.LogWarningFormat("Sound by the name {0} is not found. Issue occurred at AudioManager.StopSound()", name);
        }
    }

    public Sound GetSound(string name)
    {
        foreach (var sound in sounds)
        {
            if (sound.Name == name)
            {
                return sound;
            }
        }
        return null;
    }



    public IEnumerator FadeOut(string audioSourceName, float FadeTime)
    {
        var audioSourceSound = GetSound(audioSourceName);
        if (audioSourceSound != null)
        {
            float startVolume = audioSourceSound.Source.volume;

            while (audioSourceSound.Source.volume > 0)
            {
                audioSourceSound.Source.volume -= startVolume * Time.deltaTime / FadeTime;

                yield return null;
            }

            audioSourceSound.Source.Stop();
            audioSourceSound.Source.volume = startVolume;
        }
        else
        {
            Debug.Log("controlla se hai scritto giusto il nome della traccia");
        }
    }


}



