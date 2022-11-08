using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultyDescription : MonoBehaviour
{
    TextMeshProUGUI difficultyText;
    Dictionary<string, string> difficultyDescriptions; 

    private void Awake()
    {
        difficultyText = GetComponent<TextMeshProUGUI>();
        difficultyDescriptions = new Dictionary<string, string>()
        {
            { "Easy", "Slower start with lower tier monsters (Recommended for first playthrough)" },
            { "Medium", "Accelerated start with higher tier monsters"},
            { "Hard", "Accelerated start with higher tier and more monsters, and vehicles can be destroyed" }
        };
        SetEasyDesc();
    }

    public void SetEasyDesc()
    {
        difficultyText.text = difficultyDescriptions["Easy"];
    }

    public void SetMediumDesc()
    {
        difficultyText.text = difficultyDescriptions["Medium"];
    }

    public void SetHardDesc()
    {
        difficultyText.text = difficultyDescriptions["Hard"];
    }
}