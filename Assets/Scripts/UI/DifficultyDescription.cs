using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultyDescription : MonoBehaviour
{
    TextMeshProUGUI difficultyText;
    Dictionary<string, string> difficultyDescriptions;
    [SerializeField] AudioSource menuAudioSource;
    [SerializeField] AudioClip difficultySelectSound;

    private void Awake()
    {
        difficultyText = GetComponent<TextMeshProUGUI>();
        difficultyDescriptions = new Dictionary<string, string>()
        {
            { "Normal", "Standard progression to higher tier monsters (Recommended for first playthrough)" },
            { "Advanced", "Accelerated progression to higher tier monsters"},
            { "Expert", "Accelerated progression with more monster spawns and vehicles can be destroyed" }
        };
        SetEasyDesc();
    }

    public void SetEasyDesc()
    {
        menuAudioSource.PlayOneShot(difficultySelectSound, 0.5f);
        difficultyText.text = difficultyDescriptions["Normal"];
    }

    public void SetMediumDesc()
    {
        menuAudioSource.PlayOneShot(difficultySelectSound, 0.5f);
        difficultyText.text = difficultyDescriptions["Advanced"];
    }

    public void SetHardDesc()
    {
        menuAudioSource.PlayOneShot(difficultySelectSound, 0.5f);
        difficultyText.text = difficultyDescriptions["Expert"];
    }
}