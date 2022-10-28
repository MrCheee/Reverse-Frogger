using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPopup : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public RectTransform HealthTransform;
    public RectTransform StatusTransform;
    public Sprite HeartSprite;
    public Sprite DamageSprite;
    public Sprite ChargingSprite;
    public Sprite StunnedSprite;

    public ContentEntry EntryPrefab;

    public void ClearContent()
    {
        foreach (Transform child in HealthTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in StatusTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddToStatusContent(string status, int count)
    {
        var newEntry = Instantiate(EntryPrefab, StatusTransform);

        newEntry.Status.text = "";
        //newEntry.Status.text = status;
        newEntry.Count.text = count.ToString();
        if (status == "Damage")
        {
            newEntry.Icone.sprite = DamageSprite;
        } else if (status == "Charging")
        {
            newEntry.Icone.sprite = ChargingSprite;
        } else if (status == "Stunned")
        {
            newEntry.Icone.sprite = StunnedSprite;
        }
    }

    public void UpdateHealthContent(string alive, int count)
    {
        var newEntry = Instantiate(EntryPrefab, HealthTransform);

        newEntry.Status.text = "";
        newEntry.Count.text = count.ToString();
        newEntry.Icone.sprite = HeartSprite;
    }
}