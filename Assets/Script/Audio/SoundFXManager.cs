using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We only have one of these in the scene, hence why I'm making this a Singleton
// (even though I knew jack about it)
public class SoundFXManager : MonoBehaviour
{
    // Making this a Singleton
    public static SoundFXManager instance;
    // Now this class can be called from literally any other scripts.

    [SerializeField]
    private AudioSource soundFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// This is a PlayClipAtPoint replica, with the ability for us to access and change the sound's volume
    /// (the original one doesn't allow that since we can't really access it)
    /*
    1. Instantiate GameObject
    2. Play Sound
    3. Destroy GameObject when clip finished
    */
    public void PlayRandomSFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        // Spawn in GameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // Assign the AudioClip
        audioSource.clip = audioClip;

        // Assign Volume
        audioSource.volume = volume;

        // Play Sound
        audioSource.Play();

        // Get length of SFX clip
        float clipLength = audioSource.clip.length;

        // Destroy the clip after it's done playing
        Destroy(audioSource.gameObject, clipLength);
    }

    public void PlayRandomSoundSFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        // Assign a random index
        int rand = Random.Range(0, audioClip.Length);

        // Spawn in GameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        // Assign the AudioClip
        audioSource.clip = audioClip[rand];

        // Assign Volume
        audioSource.volume = volume;

        // Play Sound
        audioSource.Play();

        // Get length of SFX clip
        float clipLength = audioSource.clip.length;

        // Destroy the clip after it's done playing
        Destroy(audioSource.gameObject, clipLength);
    }
}
