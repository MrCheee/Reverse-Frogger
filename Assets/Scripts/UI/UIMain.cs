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

    public StartMenu StartMenu;
    public InfoPopup InfoPopup;
    public NewEnemyPopUp NewEnemyPopUp;
    public RectTransform GameOverPopUp;
    public HealthBar HealthBar;
    public SkillOrbBar SkillOrbBar;
    public RectTransform VehicleSelectionUI;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI KillsText;
    public TextMeshProUGUI BestScoreText;
    public TextMeshProUGUI BestScoreDifficulty;
    public GameObject ReferenceGrid;
    public GameObject DamageTakenPanel;

    public GameObject DamageTakenInfo;
    public TextMeshProUGUI DamageTakenText;
    public GameObject SkillGainInfo;
    public TextMeshProUGUI SkillGainText;

    GameLogWindow GameLogWindow;
    GameStateIndicators gameStateIndicators;

    [SerializeField] Camera GameCamera;

    protected IUIInfoContent m_CurrentContent;
    protected List<Status> m_ContentBuffer = new List<Status>();

    private void Awake()
    {
        Instance = this;
        InfoPopup.gameObject.SetActive(false);
        GameLogWindow = GameObject.Find("GameLogWindow").GetComponent<GameLogWindow>();
        gameStateIndicators = GameObject.Find("GameStateIndicatorYellow").GetComponent<GameStateIndicators>();
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void CloseStartMenu()
    {
        //StartMenu.gameObject.SetActive(false);
        StartMenu.CloseMenu();
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
            if (entry.count == 0) continue;
            InfoPopup.AddToStatusContent(entry.statusType, entry.count);
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

    public void UpdateBestScore(string difficulty, int bestScore)
    {
        BestScoreDifficulty.text = difficulty;
        if (difficulty == "Easy")
        {
            BestScoreDifficulty.color = Color.green;
        }
        else if (difficulty == "Medium")
        {
            BestScoreDifficulty.color = Color.yellow;
        }
        else if (difficulty == "Hard")
        {
            BestScoreDifficulty.color = Color.red;
        }
        BestScoreText.text = bestScore.ToString();
    }

    public void DisplayGameOver()
    {
        GameOverPopUp.gameObject.SetActive(true);
    }

    public void DisplayNewEnemy(Enemy enemyUnit)
    {
        NewEnemyPopUp.DisplayNewEnemy(enemyUnit);
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
        if (damage > 0)
        {
            StartCoroutine("DisplayDamageTakenFlash");
            DamageTakenText.text = damage.ToString();
            DamageTakenInfo.SetActive(true);
        }
        HealthBar.RemoveHealth(damage);
    }

    private IEnumerator DisplayDamageTakenFlash()
    {
        DamageTakenPanel.SetActive(true);
        Image panelImage = DamageTakenPanel.GetComponent<Image>();
        Color red = panelImage.color;
        float currentAlpha = 0f;
        float targetAlpha = .25f;
        float incrementAlpha = Time.deltaTime;
        while (currentAlpha < targetAlpha)
        {
            currentAlpha += incrementAlpha;
            red.a = currentAlpha;
            panelImage.color = red;
            yield return null;
        }
        while (currentAlpha > 0f)
        {
            currentAlpha -= incrementAlpha;
            red.a = currentAlpha;
            panelImage.color = red;
            yield return null;
        }
        DamageTakenPanel.SetActive(false);
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

    public void DisplaySkillGain(int gainedOrbCount)
    {
        if (gainedOrbCount > 0)
        {
            SkillGainText.text = gainedOrbCount.ToString();
            SkillGainInfo.SetActive(true);
        }
    }

    public void ResetHealthAndSkillGainInfo()
    {
        DamageTakenInfo.SetActive(false);
        SkillGainInfo.SetActive(false);
    }

    public void UpdateGameLog(string newLog)
    {
        GameLogWindow.AddToGameLog(newLog);
    }

    public void UpdateGameState(GameState currentGameState)
    {
        gameStateIndicators.UpdateGameState(currentGameState);
    }

    public void ToggleReferenceGrid()
    {
        if (ReferenceGrid.activeInHierarchy)
        {
            ReferenceGrid.SetActive(false);
        }
        else
        {
            ReferenceGrid.SetActive(true);
        }
    }
}
