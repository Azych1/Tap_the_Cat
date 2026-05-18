using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class ColorSelectionManager : MonoBehaviour
{
    public static ColorSelectionManager Instance { get; private set; }

    [Header("Главные кнопки и панели")]
    [SerializeField] private GameObject purchasePrefabObject;
    [SerializeField] private Button selectColorButton;
    [SerializeField] private GameObject selectColorText;
    [SerializeField] private GameObject colorSelectionPanel;
    [SerializeField] private Button closePanelButton;
    [SerializeField] private GameObject hintText;

    [Header("Настройка цветов")]
    [SerializeField] private Transform colorButtonsContainer;
    [SerializeField] private Click clickManager;

    [Header("Идентификатор покупки")]
    [SerializeField] private string allColorsPurchaseId = "all_colors";

    private Coroutine hintCoroutine;
    private const int MAX_LEVEL = 8;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        YG2.onPurchaseSuccess += OnPurchaseSuccess;
        YG2.onGetSDKData += OnSDKDataLoaded;

        selectColorButton.onClick.AddListener(OnSelectColorClick);
        closePanelButton.onClick.AddListener(CloseColorPanel);

        SetupColorButtons();
        UpdateUI();

        if (YG2.saves.selectedColorIndex >= 0)
            ApplyColor(YG2.saves.selectedColorIndex);
    }

    void OnDestroy()
    {
        YG2.onPurchaseSuccess -= OnPurchaseSuccess;
        YG2.onGetSDKData -= OnSDKDataLoaded;
    }

    private void OnSDKDataLoaded()
    {
        UpdateUI();
        if (YG2.saves.selectedColorIndex >= 0)
            ApplyColor(YG2.saves.selectedColorIndex);
    }

    private void OnPurchaseSuccess(string id)
    {
        if (id == allColorsPurchaseId)
        {
            YG2.saves.allColorsPurchased = true;
            YG2.SaveProgress();
            UpdateUI();
            Debug.Log("Набор всех цветов куплен!");
        }
    }

    private void OnSelectColorClick()
    {
        bool canSelect = YG2.saves.allColorsPurchased || clickManager.GetCurrentLevel() >= MAX_LEVEL;

        if (canSelect)
        {
            selectColorButton.gameObject.SetActive(false);
            if (selectColorText != null) selectColorText.SetActive(false);
            colorSelectionPanel.SetActive(true);
            if (closePanelButton != null) closePanelButton.gameObject.SetActive(true);
        }
        else
        {
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);
            hintCoroutine = StartCoroutine(ShowHintForSeconds(10f));
        }
    }

    private IEnumerator ShowHintForSeconds(float seconds)
    {
        hintText.SetActive(true);
        yield return new WaitForSeconds(seconds);
        hintText.SetActive(false);
    }

    private void CloseColorPanel()
    {
        colorSelectionPanel.SetActive(false);
        if (closePanelButton != null) closePanelButton.gameObject.SetActive(false);
        selectColorButton.gameObject.SetActive(true);
        if (selectColorText != null) selectColorText.SetActive(true);
        UpdateUI();
    }

    private void SetupColorButtons()
    {
        Sprite[] allColorSprites = clickManager.LevelSprites;

        for (int i = 0; i < colorButtonsContainer.childCount; i++)
        {
            if (i >= allColorSprites.Length) break;

            Transform buttonTransform = colorButtonsContainer.GetChild(i);
            Button btn = buttonTransform.GetComponent<Button>();
            if (btn == null) continue;

            btn.interactable = true;
            Transform abilitySpriteTransform = buttonTransform.Find("AbilitySprite");
            Image targetImage = abilitySpriteTransform != null ? abilitySpriteTransform.GetComponent<Image>() : buttonTransform.GetComponent<Image>();
            if (targetImage != null) targetImage.sprite = allColorSprites[i];

            int index = i;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnColorButtonClick(index));
        }
    }

    private void OnColorButtonClick(int colorIndex)
    {
        ApplyColor(colorIndex);
        YG2.saves.selectedColorIndex = colorIndex;
        YG2.SaveProgress();
        CloseColorPanel();
    }

    private void ApplyColor(int colorIndex)
    {
        if (clickManager != null)
            clickManager.ForceSetSprite(colorIndex);
    }

    public void UpdateUI()
    {
        bool colorsUnlocked = YG2.saves.allColorsPurchased || clickManager.GetCurrentLevel() >= MAX_LEVEL;

        if (purchasePrefabObject != null)
            purchasePrefabObject.SetActive(!YG2.saves.allColorsPurchased && clickManager.GetCurrentLevel() < MAX_LEVEL);

        if (!colorSelectionPanel.activeSelf)
        {
            selectColorButton.gameObject.SetActive(true);
            if (selectColorText != null) selectColorText.SetActive(true);
        }
    }
}