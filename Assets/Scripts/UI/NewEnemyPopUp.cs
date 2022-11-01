using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NewEnemyPopUp : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public Image EnemyImage;
    public Button CloseButton;
    Queue<EnemyInfo> newEnemies = new Queue<EnemyInfo>();

    public void DisplayNewEnemy(Enemy enemyUnit, Sprite enemySprite)
    {
        if (gameObject.activeInHierarchy)
        {
            EnemyInfo newEnemy = new EnemyInfo();
            newEnemy.name = enemyUnit.GetName();
            newEnemy.description = enemyUnit.GetDescription();
            newEnemy.sprite = enemySprite;
            newEnemies.Enqueue(newEnemy);
        }
        else
        {
            gameObject.SetActive(true);
            Name.text = enemyUnit.GetName();
            Description.text = enemyUnit.GetDescription();
            EnemyImage.sprite = enemySprite;
        }
    }

    public void ExitPopUp()
    {
        if (newEnemies.Count > 0)
        {
            EnemyInfo newEnemy = newEnemies.Dequeue();
            Name.text = newEnemy.name;
            Description.text = newEnemy.description;
            EnemyImage.sprite = newEnemy.sprite;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    struct EnemyInfo
    {
        public string name;
        public string description;
        public Sprite sprite;
    }
}