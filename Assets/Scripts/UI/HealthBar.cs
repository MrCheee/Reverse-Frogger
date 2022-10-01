using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public RectTransform PlayerHealthTransform;
    public ContentEntry HealthPrefab;

    int maxHealthCount = 10;

    public void AddHealth(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (PlayerHealthTransform.childCount < maxHealthCount)
            {
                Instantiate(HealthPrefab, PlayerHealthTransform);
            }
        }
    }

    public void RemoveHealth(int count)
    {
        int currentHealth = PlayerHealthTransform.childCount;
        int healthToRemove = Mathf.Min(currentHealth, count);
        for (int i = 0; i < healthToRemove; i++)
        {
            Destroy(PlayerHealthTransform.GetChild(PlayerHealthTransform.childCount - 1 - i).gameObject);
        }
    }
}
