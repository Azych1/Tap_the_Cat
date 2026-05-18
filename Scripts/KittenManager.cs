using UnityEngine;
using UnityEngine.UI;
using YG;

public class KittenManager : MonoBehaviour
{
    public static KittenManager Instance;

    [System.Serializable]
    public class Kitten
    {
        public int id;
        public Button buyButton;
        public Button deleteButton;
        public Image kittenImage;
        public Image buttonImage;

        [Header("Цена и доход")]
        public int price = 50;
        public float goldPerSecond = 1f;

        private Sprite originalSprite;
        private Color originalColor;
        private bool isPurchased = false;
        private Sprite purchasedSprite;

        public void Initialize(int index)
        {
            id = index;

            if (originalSprite == null && kittenImage != null)
            {
                originalSprite = kittenImage.sprite;
                originalColor = kittenImage.color;
            }

            if (YG2.isSDKEnabled)
            {
                isPurchased = YG2.saves.GetKittenPurchased(id);
                int savedSpriteIndex = YG2.saves.GetKittenSpriteIndex(id);
                if (savedSpriteIndex >= 0 && Click.Instance != null && Click.Instance.LevelSprites != null)
                {
                    if (savedSpriteIndex < Click.Instance.LevelSprites.Length)
                        purchasedSprite = Click.Instance.LevelSprites[savedSpriteIndex];
                }
            }

            ApplyState();
        }

        public void ApplyState()
        {
            if (isPurchased)
            {
                if (kittenImage != null)
                {
                    if (purchasedSprite != null) kittenImage.sprite = purchasedSprite;
                    kittenImage.color = Color.white;
                }
                if (buttonImage != null)
                {
                    if (purchasedSprite != null) buttonImage.sprite = purchasedSprite;
                    buttonImage.color = Color.white;
                }
                if (buyButton != null) buyButton.interactable = false;
                if (deleteButton != null) deleteButton.interactable = true;
            }
            else
            {
                if (kittenImage != null)
                {
                    kittenImage.sprite = originalSprite;
                    kittenImage.color = originalColor;
                }
                if (buttonImage != null)
                {
                    buttonImage.sprite = originalSprite;
                    buttonImage.color = originalColor;
                }
                if (buyButton != null) buyButton.interactable = CanBuy();
                if (deleteButton != null) deleteButton.interactable = false;
            }
        }

        private Sprite GetCurrentMainButtonSprite()
        {
            if (Click.Instance != null && Click.Instance.MainButton != null)
            {
                Image mainButtonImage = Click.Instance.MainButton.GetComponent<Image>();
                if (mainButtonImage != null && mainButtonImage.sprite != null)
                    return mainButtonImage.sprite;
            }
            return null;
        }

        public void UpdateButtonState()
        {
            if (buyButton != null) buyButton.interactable = CanBuy();
            if (deleteButton != null) deleteButton.interactable = isPurchased;
        }

        public bool CanBuy() => !isPurchased && Click.Instance.GetGold() >= price;

        public void Buy()
        {
            if (!CanBuy()) return;

            Click.Instance.SetGold(Click.Instance.GetGold() - price);
            purchasedSprite = GetCurrentMainButtonSprite();

            int spriteIndex = Click.Instance.GetCurrentSpriteIndex();
            if (spriteIndex < 0) spriteIndex = 0;

            isPurchased = true;

            float currentGPS = Click.Instance.GetGoldPerSecond();
            Click.Instance.SetGoldPerSecond(currentGPS + goldPerSecond);

            if (YG2.isSDKEnabled)
            {
                YG2.saves.SetKittenPurchased(id, true);
                YG2.saves.SetKittenSpriteIndex(id, spriteIndex);
                YG2.saves.SetKittenGoldPerSecond(id, goldPerSecond);
            }

            ApplyState();
            Click.Instance.SaveAllProgress();
            Click.Instance.UpdateUI();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUpgradeType1();

            Debug.Log($"Котенок {id} куплен. Доход: {goldPerSecond}/сек, Индекс спрайта: {spriteIndex}");
        }

        public void Delete()
        {
            if (!isPurchased) return;

            isPurchased = false;
            purchasedSprite = null;

            float currentGPS = Click.Instance.GetGoldPerSecond();
            Click.Instance.SetGoldPerSecond(Mathf.Max(0, currentGPS - goldPerSecond));

            if (YG2.isSDKEnabled)
            {
                YG2.saves.SetKittenPurchased(id, false);
                YG2.saves.SetKittenSpriteIndex(id, 0);
                YG2.saves.SetKittenGoldPerSecond(id, 0f);
            }

            ApplyState();
            Click.Instance.SaveAllProgress();
            Click.Instance.UpdateUI();

            Debug.Log($"Котенок {id} удален.");
        }

        public bool IsPurchased() => isPurchased;
        public float GetGoldPerSecond() => goldPerSecond;
    }

    public Kitten[] kittens;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < kittens.Length; i++)
        {
            int idx = i;
            if (kittens[i].buyButton != null)
                kittens[i].buyButton.onClick.AddListener(() => BuyKitten(idx));
            if (kittens[i].deleteButton != null)
                kittens[i].deleteButton.onClick.AddListener(() => DeleteKitten(idx));
        }

        ReloadKittens();

        if (YG2.isSDKEnabled)
            YG2.onGetSDKData += OnSDKDataLoaded;
        else
            RecalculateTotalGoldPerSecond(); // если нет SDK, пересчитываем сразу
    }

    void OnDestroy()
    {
        if (YG2.isSDKEnabled)
            YG2.onGetSDKData -= OnSDKDataLoaded;
    }

    private void OnSDKDataLoaded()
    {
        EnsureKittenArraysSize();
        ReloadKittens();
        RecalculateTotalGoldPerSecond(); // главное – пересчитать доход
        UpdateAllButtons();
    }

    void Update()
    {
        UpdateAllButtons();
    }

    private void UpdateAllButtons()
    {
        foreach (var kitten in kittens)
            kitten.UpdateButtonState();
    }

    // Гарантируем, что массивы в сохранениях имеют нужный размер
    private void EnsureKittenArraysSize()
    {
        int count = kittens.Length;
        if (YG2.saves.kittensPurchased.Length < count)
            System.Array.Resize(ref YG2.saves.kittensPurchased, count);
        if (YG2.saves.kittensSpriteIndex.Length < count)
            System.Array.Resize(ref YG2.saves.kittensSpriteIndex, count);
        if (YG2.saves.kittensGoldPerSecond.Length < count)
            System.Array.Resize(ref YG2.saves.kittensGoldPerSecond, count);
    }

    // Пересчитываем общий доход в секунду (от улучшений + от котят)
    private void RecalculateTotalGoldPerSecond()
    {
        if (Click.Instance == null) return;

        // Получаем доход от улучшений
        float upgradesIncome = 0f;
        if (BonusBuy.Instance != null)
            upgradesIncome = BonusBuy.Instance.GetTotalGoldPerSecondFromUpgrades();

        // Получаем доход от котят из сохранений
        float kittensIncome = 0f;
        for (int i = 0; i < kittens.Length; i++)
        {
            if (YG2.saves.GetKittenPurchased(i))
                kittensIncome += YG2.saves.GetKittenGoldPerSecond(i);
        }

        // Устанавливаем общий доход
        Click.Instance.SetGoldPerSecond(upgradesIncome + kittensIncome);
        Click.Instance.UpdateUI();
    }

    public void ReloadKittens()
    {
        for (int i = 0; i < kittens.Length; i++)
            kittens[i].Initialize(i);
        UpdateAllButtons();
    }

    public void BuyKitten(int id)
    {
        if (id < 0 || id >= kittens.Length) return;
        kittens[id].Buy();
    }

    public void DeleteKitten(int id)
    {
        if (id < 0 || id >= kittens.Length) return;
        kittens[id].Delete();
    }
}