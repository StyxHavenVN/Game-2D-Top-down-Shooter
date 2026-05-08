using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource effectAudioSource;
    private AudioClip shootClip;
    private AudioClip reloadClip;
    private AudioClip energyClip;
    private void PlayShootSound()
    {
        effectAudioSource.PlayOneShot(shootClip);
    }
    private void PlayreloadSound()
    {
        effectAudioSource.PlayOneShot(reloadClip);
    }
    private void PlayenergySound()
    {
        effectAudioSource.PlayOneShot(energyClip);
    }
}
