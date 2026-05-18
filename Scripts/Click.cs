using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class Click : MonoBehaviour
{
    public static Click Instance;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI goldPerClickText;
    [SerializeField] private TextMeshProUGUI goldPerSecondText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI clicksText;
    [SerializeField] private TextMeshProUGUI maxLevelText;
    [SerializeField] private Button mainButton;
    [SerializeField] private Sprite[] levelSprites;

    public Button MainButton => mainButton;
    public Sprite[] LevelSprites => levelSprites;

    [SerializeField] private GameObject milkSpritePrefab;
    [SerializeField] private RectTransform milkSpawnPoint;
    [SerializeField] private RectTransform milkTargetPoint;
    [SerializeField] private float milkMoveSpeed = 10f;

    private float goldAccumulator = 0f;
    private float updateInterval = 0.1f;
    private float timer = 0f;
    private float saveInterval = 1.0f;
    private float saveTimer = 0f;

    private const int MAX_LEVEL = 8;
    private int clicksForNextLevel = 800;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (YG2.isSDKEnabled)
            YG2.onGetSDKData += OnDataLoaded;
    }

    private void OnDestroy()
    {
        if (YG2.isSDKEnabled)
            YG2.onGetSDKData -= OnDataLoaded;
    }

    private void OnDataLoaded()
    {
        if (YG2.isSDKEnabled)
        {
            if (YG2.saves.currentLevel == 0)
            {
                YG2.saves.currentLevel = 1;
                YG2.saves.clicksForNextLevel = 800;
                SaveAllProgress();
            }
            clicksForNextLevel = YG2.saves.clicksForNextLevel;
        }

        // Восстанавливаем цвет кота
        if (YG2.saves.selectedColorIndex >= 0 && YG2.saves.selectedColorIndex < levelSprites.Length)
            ForceSetSprite(YG2.saves.selectedColorIndex);
        else
            SetButtonSpriteForLevel(GetCurrentLevel());

        UpdateUI();

        // Перезагружаем котят (они вызовут пересчёт дохода внутри себя)
        if (KittenManager.Instance != null)
            KittenManager.Instance.ReloadKittens();

        if (ColorSelectionManager.Instance != null)
            ColorSelectionManager.Instance.UpdateUI();
    }

    void Start()
    {
        if (YG2.isSDKEnabled && YG2.saves != null)
        {
            if (YG2.saves.selectedColorIndex >= 0 && YG2.saves.selectedColorIndex < levelSprites.Length)
                ForceSetSprite(YG2.saves.selectedColorIndex);
            else
                SetButtonSpriteForLevel(GetCurrentLevel());
        }
        else
            SetButtonSpriteForLevel(GetCurrentLevel());

        UpdateUI();
    }

    void Update()
    {
        timer += Time.deltaTime;
        saveTimer += Time.deltaTime;

        float currentGoldPerSecond = GetGoldPerSecond();
        goldAccumulator += currentGoldPerSecond * Time.deltaTime;

        if (timer >= updateInterval && currentGoldPerSecond > 0)
        {
            if (goldAccumulator >= 1f)
            {
                int earned = Mathf.FloorToInt(goldAccumulator);
                SetGold(GetGold() + earned);
                goldAccumulator -= earned;

                if (earned > 0)
                {
                    UpdateUI();
                    if (saveTimer >= saveInterval && YG2.isSDKEnabled)
                    {
                        SaveAllProgress();
                        saveTimer = 0f;
                    }
                }
            }
            timer = 0f;
        }

        if (saveTimer >= saveInterval && YG2.isSDKEnabled && currentGoldPerSecond > 0)
        {
            SaveAllProgress();
            saveTimer = 0f;
        }
    }

    public void OnBtnClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.InitializeAudio();

        SetGold(GetGold() + GetGoldPerClick());

        if (milkSpritePrefab != null && milkSpawnPoint != null && milkTargetPoint != null)
            CreateMilkSprite();

        if (YG2.isSDKEnabled)
        {
            YG2.saves.totalClicks++;

            if (YG2.saves.currentLevel < MAX_LEVEL &&
                YG2.saves.totalClicks >= YG2.saves.clicksForNextLevel)
                LevelUp();

            SaveAllProgress();
            saveTimer = 0f;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMainClick();

        UpdateUI();
    }

    private void CreateMilkSprite()
    {
        GameObject milkSprite = Instantiate(milkSpritePrefab, milkSpawnPoint.parent);
        RectTransform milkRect = milkSprite.GetComponent<RectTransform>();
        if (milkRect != null)
        {
            milkRect.anchoredPosition = milkSpawnPoint.anchoredPosition;
            milkRect.sizeDelta = milkSpawnPoint.sizeDelta;
        }
        StartCoroutine(MoveMilkSprite(milkSprite));
    }

    private System.Collections.IEnumerator MoveMilkSprite(GameObject milkSprite)
    {
        if (milkSprite == null) yield break;

        RectTransform milkRect = milkSprite.GetComponent<RectTransform>();
        if (milkRect == null) yield break;

        Vector2 startPosition = milkRect.anchoredPosition;
        Vector2 targetPosition = milkTargetPoint.anchoredPosition;
        float journeyLength = Vector2.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (milkSprite != null && Vector2.Distance(milkRect.anchoredPosition, targetPosition) > 1f)
        {
            float distanceCovered = (Time.time - startTime) * milkMoveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            milkRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, fractionOfJourney);
            yield return null;
        }

        if (milkSprite != null) Destroy(milkSprite);
    }

    private void LevelUp()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.saves.currentLevel++;
            YG2.saves.clicksForNextLevel *= 2;
            clicksForNextLevel = YG2.saves.clicksForNextLevel;

            SaveAllProgress();

            if (YG2.saves.selectedColorIndex == -1)
                SetButtonSpriteForLevel(YG2.saves.currentLevel);

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayLevelUp();

            Debug.Log($"Новый уровень: {YG2.saves.currentLevel}! Кликов до следующего: {YG2.saves.clicksForNextLevel}");

            if (ColorSelectionManager.Instance != null)
                ColorSelectionManager.Instance.UpdateUI();
        }
    }

    private void SetButtonSpriteForLevel(int level)
    {
        if (mainButton != null && levelSprites != null && levelSprites.Length > 0)
        {
            int safeLevel = Mathf.Clamp(level, 1, Mathf.Min(levelSprites.Length, MAX_LEVEL)) - 1;
            if (safeLevel >= 0 && safeLevel < levelSprites.Length)
            {
                Image btnImage = mainButton.GetComponent<Image>();
                if (btnImage != null && levelSprites[safeLevel] != null)
                    btnImage.sprite = levelSprites[safeLevel];
            }
        }
    }

    public void ForceSetSprite(int spriteIndex)
    {
        if (mainButton != null && levelSprites != null && spriteIndex >= 0 && spriteIndex < levelSprites.Length)
        {
            Image btnImage = mainButton.GetComponent<Image>();
            if (btnImage != null && levelSprites[spriteIndex] != null)
                btnImage.sprite = levelSprites[spriteIndex];
        }
    }

    public int GetCurrentSpriteIndex()
    {
        if (mainButton != null)
        {
            Image btnImage = mainButton.GetComponent<Image>();
            if (btnImage != null && btnImage.sprite != null && levelSprites != null)
            {
                for (int i = 0; i < levelSprites.Length; i++)
                {
                    if (levelSprites[i] == btnImage.sprite)
                        return i;
                }
            }
        }
        return -1;
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = GetGold().ToString();
        if (goldPerClickText != null) goldPerClickText.text = $"+{FormatNumber(GetGoldPerClick())}";
        if (goldPerSecondText != null) goldPerSecondText.text = $"+{FormatNumber(GetGoldPerSecond())}";
        if (levelText != null) levelText.text = $"{GetCurrentLevel()}";

        if (clicksText != null && maxLevelText != null)
        {
            int currentClicks = GetTotalClicks();
            int neededClicks = GetClicksForNextLevel();

            if (GetCurrentLevel() >= MAX_LEVEL)
            {
                clicksText.text = "";
                maxLevelText.gameObject.SetActive(true);
            }
            else
            {
                clicksText.text = $"{currentClicks}/{neededClicks}";
                maxLevelText.gameObject.SetActive(false);
            }
        }
        else if (clicksText != null)
        {
            int currentClicks = GetTotalClicks();
            int neededClicks = GetClicksForNextLevel();
            clicksText.text = (GetCurrentLevel() >= MAX_LEVEL) ? "МАКС" : $"{currentClicks}/{neededClicks}";
        }
    }

    private string FormatNumber(int num) => num >= 1000 ? $"{num / 1000f:0.#}Т" : num.ToString();
    private string FormatNumber(float num) => num >= 1000 ? $"{num / 1000f:0.#}Т" : num.ToString("0.#");

    public int GetGold() => YG2.isSDKEnabled ? YG2.saves.gold : 0;
    public void SetGold(int value) { if (YG2.isSDKEnabled) YG2.saves.gold = value; }
    public int GetGoldPerClick() => YG2.isSDKEnabled ? YG2.saves.goldPerClick : 1;
    public void SetGoldPerClick(int value) { if (YG2.isSDKEnabled) YG2.saves.goldPerClick = value; }
    public float GetGoldPerSecond() => YG2.isSDKEnabled ? YG2.saves.goldPerSecond : 0f;
    public void SetGoldPerSecond(float value) { if (YG2.isSDKEnabled) YG2.saves.goldPerSecond = value; }
    public int GetTotalClicks() => YG2.isSDKEnabled ? YG2.saves.totalClicks : 0;
    public int GetCurrentLevel() => YG2.isSDKEnabled ? YG2.saves.currentLevel : 1;
    public int GetClicksForNextLevel() => YG2.isSDKEnabled ? YG2.saves.clicksForNextLevel : clicksForNextLevel;

    public void SaveAllProgress()
    {
        if (YG2.isSDKEnabled && YG2.saves != null)
        {
            YG2.saves.gold = GetGold();
            YG2.saves.goldPerClick = GetGoldPerClick();
            YG2.saves.goldPerSecond = GetGoldPerSecond();
            YG2.saves.currentLevel = GetCurrentLevel();
            YG2.saves.clicksForNextLevel = GetClicksForNextLevel();
            YG2.saves.totalClicks = GetTotalClicks();

            YG2.SaveProgress();
        }
    }
}