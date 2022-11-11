using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class NewEnemyPopUp : MonoBehaviour
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public Image EnemyImage;
    public Button CloseButton;
    Queue<EnemyInfo> newEnemies = new Queue<EnemyInfo>();
    AudioSource audioSource;
    [SerializeField] AudioClip openSound;
    [SerializeField] AudioClip closeSound;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void DisplayNewEnemy(Enemy enemyUnit)
    {
        if (gameObject.activeInHierarchy)
        {
            EnemyInfo newEnemy = new EnemyInfo();
            newEnemy.name = enemyUnit.GetName();
            newEnemy.description = enemyUnit.GetDescription();

            SpriteRenderer enemySpriteRenderer = enemyUnit.GetComponentInChildren<SpriteRenderer>();
            newEnemy.sprite = enemySpriteRenderer.sprite;
            newEnemy.spriteColor = enemySpriteRenderer.color;

            newEnemies.Enqueue(newEnemy);
        }
        else
        {
            gameObject.SetActive(true);

            audioSource.clip = openSound;
            audioSource.Play();

            Name.text = enemyUnit.GetName();
            Description.text = enemyUnit.GetDescription();

            SpriteRenderer enemySpriteRenderer = enemyUnit.GetComponentInChildren<SpriteRenderer>();
            EnemyImage.sprite = enemySpriteRenderer.sprite;
            Color tempColor = enemySpriteRenderer.color;
            tempColor.a = 1f;
            EnemyImage.color = tempColor;
        }
    }

    public void ExitPopUp()
    {
        if (newEnemies.Count > 0)
        {
            audioSource.clip = openSound;
            audioSource.Play();

            EnemyInfo newEnemy = newEnemies.Dequeue();
            Name.text = newEnemy.name;
            Description.text = newEnemy.description;
            EnemyImage.sprite = newEnemy.sprite;
            Color tempColor = newEnemy.spriteColor;
            tempColor.a = 1f;
            EnemyImage.color = tempColor;
        }
        else
        {
            audioSource.clip = closeSound;
            audioSource.Play();
            gameObject.SetActive(false);
        }
    }

    struct EnemyInfo
    {
        public string name;
        public string description;
        public Sprite sprite;
        public Color spriteColor;
    }
}