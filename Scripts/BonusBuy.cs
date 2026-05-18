using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class BonusBuy : MonoBehaviour
{
    public static BonusBuy Instance;
    public Upgrade[] upgrades;

    [System.Serializable]
    public class Upgrade
    {
        public string upgradeName;
        public Button buyButton;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI maxText;
        public TextMeshProUGUI costWordText;

        public int baseCost = 10;
        public int bonusPerClick = 1;
        public float bonusPerSecond = 0f;
        public int maxLevel = 10;

        [HideInInspector] public int currentLevel = 0;
        [HideInInspector] public int currentCost;
        [HideInInspector] public int upgradeIndex = -1;

        public void Initialize(int index)
        {
            upgradeIndex = index;
            if (YG2.isSDKEnabled && YG2.saves.upgradeLevels != null && index < YG2.saves.upgradeLevels.Length)
                currentLevel = YG2.saves.GetUpgradeLevel(index);
            else
                currentLevel = 0;

            currentCost = baseCost * (int)Mathf.Pow(2, currentLevel);
            UpdateUI();
        }

        public void UpdateUI()
        {
            string formattedCost = FormatCost(currentCost);
            if (costText != null) costText.text = $"{formattedCost}";
            if (levelText != null) levelText.text = $"{currentLevel}";

            if (currentLevel >= maxLevel)
            {
                if (costText != null) costText.gameObject.SetActive(false);
                if (maxText != null)
                {
                    maxText.gameObject.SetActive(true);
                    if (costWordText != null) costWordText.gameObject.SetActive(false);
                }
            }
            else
            {
                if (costText != null)
                {
                    costText.text = $"{formattedCost}";
                    costText.gameObject.SetActive(true);
                }
                if (maxText != null) maxText.gameObject.SetActive(false);
            }
        }

        private string FormatCost(int value) => value >= 1000 ? $"{value / 1000f:0.#}Т" : value.ToString();

        public bool CanBuy(int gold) => currentLevel < maxLevel && gold >= currentCost;

        public void Buy()
        {
            if (!CanBuy(Click.Instance.GetGold())) return;

            int playerGold = Click.Instance.GetGold();
            int playerGoldPerClick = Click.Instance.GetGoldPerClick();
            float playerGoldPerSecond = Click.Instance.GetGoldPerSecond();

            playerGold -= currentCost;
            playerGoldPerClick += bonusPerClick;
            playerGoldPerSecond += bonusPerSecond;
            currentLevel++;
            currentCost *= 2;

            Click.Instance.SetGold(playerGold);
            Click.Instance.SetGoldPerClick(playerGoldPerClick);
            Click.Instance.SetGoldPerSecond(playerGoldPerSecond);

            if (YG2.isSDKEnabled)
                YG2.saves.SetUpgradeLevel(upgradeIndex, currentLevel);

            Click.Instance.SaveAllProgress();
            UpdateUI();
            Click.Instance.UpdateUI();

            if (SoundManager.Instance != null)
            {
                if (bonusPerSecond >= 1f)
                    SoundManager.Instance.PlayUpgradeType2();
                else
                    SoundManager.Instance.PlayUpgradeType1();
            }
        }
    }

    void Start()
    {
        Instance = this;

        for (int i = 0; i < upgrades.Length; i++)
        {
            upgrades[i].Initialize(i);
            int idx = i;
            upgrades[i].buyButton.onClick.AddListener(() => BuyUpgrade(upgrades[idx]));
        }

        UpdateButtons();

        if (YG2.isSDKEnabled)
            YG2.onGetSDKData += OnSDKDataLoaded;
        else
            ApplyUpgradeLevels(); // если нет SDK, применяем сразу
    }

    void OnDestroy()
    {
        if (YG2.isSDKEnabled)
            YG2.onGetSDKData -= OnSDKDataLoaded;
    }

    private void OnSDKDataLoaded()
    {
        for (int i = 0; i < upgrades.Length; i++)
            upgrades[i].Initialize(i);
        UpdateButtons();
        ApplyUpgradeLevels(); // пересчитываем доход
    }

    void Update()
    {
        UpdateButtons();
    }

    public void BuyUpgrade(Upgrade upgrade)
    {
        if (upgrade.CanBuy(Click.Instance.GetGold()))
        {
            upgrade.Buy();
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        int playerGold = Click.Instance.GetGold();
        foreach (var upgrade in upgrades)
            if (upgrade.buyButton != null)
                upgrade.buyButton.interactable = upgrade.CanBuy(playerGold);
    }

    // Метод для получения общего дохода от улучшений в секунду
    public float GetTotalGoldPerSecondFromUpgrades()
    {
        float total = 0f;
        foreach (var upgrade in upgrades)
        {
            if (upgrade.currentLevel > 0)
                total += upgrade.bonusPerSecond * upgrade.currentLevel;
        }
        return total;
    }

    private void ApplyUpgradeLevels()
    {
        int totalBonusPerClick = 0;
        float totalBonusPerSecond = 0f;

        foreach (var upgrade in upgrades)
            if (upgrade.currentLevel > 0)
            {
                totalBonusPerClick += upgrade.bonusPerClick * upgrade.currentLevel;
                totalBonusPerSecond += upgrade.bonusPerSecond * upgrade.currentLevel;
            }

        int baseGoldPerClick = 1;
        float baseGoldPerSecond = 0f;

        Click.Instance.SetGoldPerClick(baseGoldPerClick + totalBonusPerClick);
        Click.Instance.SetGoldPerSecond(baseGoldPerSecond + totalBonusPerSecond);

        Click.Instance.UpdateUI();
        Click.Instance.SaveAllProgress();
    }
}