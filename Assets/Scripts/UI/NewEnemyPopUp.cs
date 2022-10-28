using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewEnemyPopUp : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public Image EnemyImage;
    public Button CloseButton;

    public void DisplayNewEnemy(Enemy enemyUnit, Sprite enemySprite)
    {
        gameObject.SetActive(true);
        Name.text = enemyUnit.GetName();
        Description.text = enemyUnit.GetDescription();
        EnemyImage.sprite = enemySprite;
    }

    public void ExitPopUp()
    {
        gameObject.SetActive(false);
    }
}