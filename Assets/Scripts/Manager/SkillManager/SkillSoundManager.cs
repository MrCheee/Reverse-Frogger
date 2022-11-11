using System.Collections.Generic;
using UnityEngine;

public class SkillSoundManager : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] AudioClip AirdropFallingSound;
    [SerializeField] AudioClip CallingVehSound;
    [SerializeField] AudioClip[] CallinResponseSounds;
    [SerializeField] AudioClip InvalidSelectionSound;
    [SerializeField] AudioClip LaneChangeSound;
    [SerializeField] AudioClip SkillSelectSound;
    [SerializeField] AudioClip SkillDeselectSound;
    [SerializeField] AudioClip SkillConfirmSound;

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

    public void PlayAirdrop()
    {
        audioClips.Enqueue(AirdropFallingSound);
    }

    public void PlayCallinVeh()
    {
        audioClips.Enqueue(CallingVehSound);
        audioClips.Enqueue(CallinResponseSounds[Random.Range(0, CallinResponseSounds.Length)]);
    }

    public void PlayLaneChange()
    {
        audioClips.Enqueue(LaneChangeSound);
    }

    public void PlayInvalidSelection()
    {
        audioClips.Enqueue(InvalidSelectionSound);
    }

    public void PlaySkillSelect()
    {
        audioClips.Enqueue(SkillSelectSound);
    }

    public void PlaySkillDeselect()
    {
        if (!audioClips.Contains(SkillSelectSound) && !audioClips.Contains(SkillConfirmSound))
        {
            audioClips.Enqueue(SkillDeselectSound);
        }
    }

    public void PlaySkillConfirm()
    {
        audioClips.Enqueue(SkillConfirmSound);
    }
}