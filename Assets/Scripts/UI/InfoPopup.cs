using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPopup : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public RectTransform HealthTransform;
    public RectTransform StatusTransform;

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

        newEntry.Status.text = status;
        newEntry.Count.text = count.ToString();
        //newEntry.Icone.sprite = Icone;
    }

    public void UpdateHealthContent(string alive, int count)
    {
        var newEntry = Instantiate(EntryPrefab, HealthTransform);

        newEntry.Status.text = alive;
        newEntry.Count.text = count.ToString();
        //newEntry.Icone.sprite = Icone;
    }
}