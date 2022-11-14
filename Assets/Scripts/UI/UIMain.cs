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
    public GameObject InstructionPopUp;
    public GameObject SkillsInfoPopUp;
    public NewEnemyPopUp NewEnemyPopUp;
    public GameObject GameOverPopUp;
    public HealthBar HealthBar;
    public SkillOrbBar SkillOrbBar;
    public RectTransform VehicleSelectionUI;
    public TextMeshProUGUI LevelLabel;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI KillsText;
    public TextMeshProUGUI BestScoreText;
    public TextMeshProUGUI BestScoreDifficulty;
    public GameObject ReferenceGrid;
    public GameObject DamageTakenPanel;
    public GameObject CreditsPopUp;

    public GameObject DamageTakenInfo;
    public TextMeshProUGUI DamageTakenText;
    public GameObject SkillGainInfo;
    public TextMeshProUGUI SkillGainText;

    GameStateIndicators gameStateIndicators;

    [SerializeField] Camera GameCamera;
    [SerializeField] AudioSource BGMSource;
    [SerializeField] AudioListener PlayerAudio;
    bool soundOn = true;
    bool bgmOn = true;
    [SerializeField] Image BGMButtonImg;
    [SerializeField] Image AudioButtonImg;
    [SerializeField] Sprite BGMOn;
    [SerializeField] Sprite BGMOff;
    [SerializeField] Sprite AudioOn;
    [SerializeField] Sprite AudioOff;

    protected IUIInfoContent m_CurrentContent;
    protected List<Status> m_ContentBuffer = new List<Status>();

    Coroutine m_Coroutine;

    private void Awake()
    {
        Instance = this;
        InfoPopup.gameObject.SetActive(false);
        gameStateIndicators = GameObject.Find("GameStateIndicatorYellow").GetComponent<GameStateIndicators>();
        m_Coroutine = null;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void CloseStartMenu()
    {
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
        if (level < 30)
        {
            m_Coroutine = StartCoroutine(LevelUpFlashing());
            LevelText.text = level.ToString();
        }
        else
        {
            LevelText.text = "MAX";
        }
    }

    private IEnumerator LevelUpFlashing()
    {
        bool flashed = false;
        while (true)
        {
            if (flashed)
            {
                LevelLabel.color = Color.white;
                LevelText.color = Color.white;
                flashed = false;
            }
            else
            {
                LevelLabel.color = Color.yellow;
                LevelText.color = Color.yellow;
                flashed = true;
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void UpdateKills(int kills)
    {
        KillsText.text = kills.ToString();
    }

    public void UpdateBestScore(string difficulty, int bestScore)
    {
        BestScoreDifficulty.text = difficulty;
        if (difficulty == "Normal")
        {
            BestScoreDifficulty.color = Color.green;
        }
        else if (difficulty == "Advanced")
        {
            BestScoreDifficulty.color = Color.yellow;
        }
        else if (difficulty == "Expert")
        {
            BestScoreDifficulty.color = Color.red;
        }
        BestScoreText.text = bestScore.ToString();
    }

    public void DisplayGameOver()
    {
        GameOverPopUp.SetActive(true);
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
        if (m_Coroutine != null)
        {
            StopCoroutine(m_Coroutine);
            LevelLabel.color = Color.white;
            LevelText.color = Color.white;
        }
        DamageTakenInfo.SetActive(false);
        SkillGainInfo.SetActive(false);
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

    public void ToggleBGM()
    {
        if (bgmOn)
        {
            bgmOn = false;
            BGMSource.volume = 0;
            BGMButtonImg.sprite = BGMOn;
        }
        else
        {
            bgmOn = true;
            BGMSource.volume = .25f;
            BGMButtonImg.sprite = BGMOff;
        }
    }

    public void ToggleAudio()
    {
        if (soundOn)
        {
            soundOn = false;
            PlayerAudio.enabled = false;
            AudioButtonImg.sprite = AudioOn;
        }
        else
        {
            soundOn = true;
            PlayerAudio.enabled = true;
            AudioButtonImg.sprite = AudioOff;
        }
    }

    public void ToggleInstructions()
    {
        if (InstructionPopUp.activeInHierarchy)
        {
            InstructionPopUp.SetActive(false);
        }
        else
        {
            InstructionPopUp.SetActive(true);
        }
    }

    public void ToggleSkillsInfo()
    {
        if (SkillsInfoPopUp.activeInHierarchy)
        {
            SkillsInfoPopUp.SetActive(false);
        }
        else
        {
            SkillsInfoPopUp.SetActive(true);
        }
    }

    public void ToggleCredits()
    {
        if (CreditsPopUp.activeInHierarchy)
        {
            CreditsPopUp.SetActive(false);
        }
        else
        {
            CreditsPopUp.SetActive(true);
        }
    }

    public void CloseCredits()
    {
        CreditsPopUp.SetActive(false);
    }
}
