using System.Collections.Generic;
using UnityEngine;

public class GameStateSoundManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip[] VehicleGameStateSounds;
    [SerializeField] AudioClip EnemySpawnGameStateSound;

    Queue<AudioClip> audioClips;

    private void Awake()
    {
        audioClips = new Queue<AudioClip>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!audioSource.isPlaying && audioClips.Count > 0)
        {
            audioSource.clip = audioClips.Dequeue();
            audioSource.Play();
        }
    }

    public void PlayVehicleGameState()
    {
        audioClips.Enqueue(VehicleGameStateSounds[Random.Range(0, VehicleGameStateSounds.Length)]);
    }

    public void PlayEnemySpawnGameState()
    {
        audioClips.Enqueue(EnemySpawnGameStateSound);
    }
}