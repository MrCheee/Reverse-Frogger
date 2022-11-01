using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMain : MonoBehaviour
{
    public static UIMain Instance { get; private set; }

    public interface IUIInfoContent
    {
        string GetName();
        string GetDescription();
        void GetContent(ref List<Status> content);
    }

    public RectTransform StartMenu;
    public InfoPopup InfoPopup;
    public NewEnemyPopUp NewEnemyPopUp;
    public RectTransform GameOverPopUp;
    public HealthBar HealthBar;
    public SkillOrbBar SkillOrbBar;
    public RectTransform VehicleSelectionUI;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI KillsText;
    public TextMeshProUGUI BestScoreText;

    GameLogWindow GameLogWindow;
    GameStateToggles gameStateToggles;

    [SerializeField] Camera GameCamera;
    public GameObject EnemyKilledPrefab;

    protected IUIInfoContent m_CurrentContent;
    protected List<Status> m_ContentBuffer = new List<Status>();

    private void Awake()
    {
        Instance = this;
        InfoPopup.gameObject.SetActive(false);
        GameLogWindow = GameObject.Find("GameLogWindow").GetComponent<GameLogWindow>();
        gameStateToggles = GameObject.Find("GameStateIndicator").GetComponent<GameStateToggles>();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void CloseStartMenu()
    {
        StartMenu.gameObject.SetActive(false);
    }

    public void SetNewInfoContent(IUIInfoContent content)
    {
        if (content == null)
        {
            InfoPopup.gameObject.SetActive(false);
        }
        else
        {
            InfoPopup.gameObject.SetActive(true);
            m_CurrentContent = content;
            InfoPopup.Name.text = content.GetName();
            InfoPopup.Description.text = content.GetDescription();
            UpdateContent();
        }
    }

    public void UpdateContent()
    {
        if (m_CurrentContent == null) return;

        InfoPopup.ClearContent();
        m_ContentBuffer.Clear();

        m_CurrentContent.GetContent(ref m_ContentBuffer);
        foreach (var entry in m_ContentBuffer)
        {
            //Sprite icon = null;
            //if (ResourceDB != null)
            //    icon = ResourceDB.GetItem(entry.ResourceId)?.Icone;
            if (entry.statusType == "Health")
            {
                if (entry.count == 0)
                {
                    InfoPopup.UpdateHealthContent("DEAD", entry.count);
                }
                else
                {
                    InfoPopup.UpdateHealthContent("HP", entry.count);
                }
            }
            else
            {
                if (entry.count == 0) continue;
                InfoPopup.AddToStatusContent(entry.statusType, entry.count);
            }
        }
    }

    public void UpdateLevel(int level)
    {
        LevelText.text = level.ToString();
    }

    public void UpdateKills(int kills)
    {
        KillsText.text = kills.ToString();
    }

    public void UpdateBestScore(int bestScore)
    {
        BestScoreText.text = bestScore.ToString();
    }

    public void DisplayGameOver()
    {
        GameOverPopUp.gameObject.SetActive(true);
    }

    public void DisplayNewEnemy(Enemy enemyUnit, Sprite enemyImage)
    {
        NewEnemyPopUp.DisplayNewEnemy(enemyUnit, enemyImage);
    }

    public void ActivateVehicleSelectionUI()
    {
        VehicleSelectionUI.gameObject.SetActive(true);
    }

    public void DeactivateVehicleSelectionUI()
    {
        VehicleSelectionUI.gameObject.SetActive(false);
    }

    public void AddHealth(int health)
    {
        HealthBar.AddHealth(health);
    }

    public void RemoveHealth(int damage)
    {
        HealthBar.RemoveHealth(damage);
    }

    public void AddSkillOrb(int count)
    {
        SkillOrbBar.AddSkillOrb(count);
    }

    public void RemoveSkillOrb(int count)
    {
        SkillOrbBar.RemoveSkillOrb(count);
    }

    public void DeactivateSkillOrb(int count)
    {
        SkillOrbBar.DeactivateSkillOrb(count);
    }

    public void ReactivateSkillOrb(int count)
    {
        SkillOrbBar.ReactivateSkillOrb(count);
    }

    public void RefreshSkillOrbBar()
    {
        SkillOrbBar.FullRefreshSkillbar();
    }

    public void UpdateGameLog(string newLog)
    {
        GameLogWindow.AddToGameLog(newLog);
    }

    public void UpdateGameState(GameState currentGameState)
    {
        gameStateToggles.UpdateGameState(currentGameState);
    }

    public void DisplayKilledEnemy(Vector3 killedPos)
    {
        var killedUI = Instantiate(EnemyKilledPrefab, GameCamera.WorldToScreenPoint(killedPos), EnemyKilledPrefab.transform.rotation, transform);
        killedUI.transform.localScale -= new Vector3(0.4f, 0.4f, 0.4f);
        killedUI.transform.SetSiblingIndex(0);
    }
}