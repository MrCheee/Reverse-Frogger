using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoPopup : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public RectTransform StatusTransform;
    public Sprite HeartSprite;
    public Sprite DamageSprite;
    public Sprite ChargingSprite;
    public Sprite StunnedSprite;

    public ContentEntry EntryPrefab;

    public void ClearContent()
    {
        foreach (Transform child in StatusTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddToStatusContent(string status, int count)
    {
        var newEntry = Instantiate(EntryPrefab, StatusTransform);
        newEntry.Count.text = count.ToString();
        if (status == "Health")
        {
            newEntry.Icone.sprite = HeartSprite;
        }
        else if (status == "Damage")
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
}