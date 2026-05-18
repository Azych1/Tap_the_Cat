using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform secondaryPanel;

    [Header("Позиции панелей")]
    [SerializeField] private Vector2 mainPanelVisiblePosition = Vector2.zero;
    [SerializeField] private Vector2 mainPanelHiddenPosition = new Vector2(-2000, 0);
    [SerializeField] private Vector2 secondaryPanelVisiblePosition = Vector2.zero;
    [SerializeField] private Vector2 secondaryPanelHiddenPosition = new Vector2(2000, 0);

    [Header("Настройки анимации")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private bool useAnimation = true;

    private UIPanelState currentState = UIPanelState.Main;
    private bool isAnimating = false;

    private enum UIPanelState
    {
        Main,
        Secondary
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Инициализация позиций
        if (mainPanel != null)
        {
            mainPanel.anchoredPosition = mainPanelVisiblePosition;
        }

        if (secondaryPanel != null)
        {
            secondaryPanel.anchoredPosition = secondaryPanelHiddenPosition;
        }
    }

    public void SwitchToSecondaryPanel()
    {
        if (isAnimating || currentState == UIPanelState.Secondary) return;

        if (useAnimation)
        {
            StartCoroutine(AnimatePanels(
                mainPanelHiddenPosition,
                secondaryPanelVisiblePosition,
                false
            ));
        }
        else
        {
            SetPanelsImmediate(false);
        }

        currentState = UIPanelState.Secondary;
    }

    public void SwitchToMainPanel()
    {
        if (isAnimating || currentState == UIPanelState.Main) return;

        if (useAnimation)
        {
            StartCoroutine(AnimatePanels(
                secondaryPanelHiddenPosition,
                mainPanelVisiblePosition,
                true
            ));
        }
        else
        {
            SetPanelsImmediate(true);
        }

        currentState = UIPanelState.Main;
    }

    public void TogglePanels()
    {
        if (currentState == UIPanelState.Main)
        {
            SwitchToSecondaryPanel();
        }
        else
        {
            SwitchToMainPanel();
        }
    }

    private System.Collections.IEnumerator AnimatePanels(Vector2 hidePosition, Vector2 showPosition, bool toMain)
    {
        isAnimating = true;

        RectTransform panelToHide = toMain ? secondaryPanel : mainPanel;
        RectTransform panelToShow = toMain ? mainPanel : secondaryPanel;

        Vector2 startHidePos = panelToHide.anchoredPosition;
        Vector2 startShowPos = panelToShow.anchoredPosition;

        float elapsedTime = 0f;
        float duration = 0.5f; // Фиксированная длительность анимации в секундах

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Плавное движение с использованием Lerp
            if (panelToHide != null)
                panelToHide.anchoredPosition = Vector2.Lerp(startHidePos, hidePosition, t);

            if (panelToShow != null)
                panelToShow.anchoredPosition = Vector2.Lerp(startShowPos, showPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Устанавливаем точные конечные позиции
        if (panelToHide != null)
            panelToHide.anchoredPosition = hidePosition;

        if (panelToShow != null)
            panelToShow.anchoredPosition = showPosition;

        isAnimating = false;
    }

    // Метод для быстрого переключения без анимации
    public void SetPanelsImmediate(bool showMain)
    {
        StopAllCoroutines();
        isAnimating = false;

        if (showMain)
        {
            if (mainPanel != null) mainPanel.anchoredPosition = mainPanelVisiblePosition;
            if (secondaryPanel != null) secondaryPanel.anchoredPosition = secondaryPanelHiddenPosition;
            currentState = UIPanelState.Main;
        }
        else
        {
            if (mainPanel != null) mainPanel.anchoredPosition = mainPanelHiddenPosition;
            if (secondaryPanel != null) secondaryPanel.anchoredPosition = secondaryPanelVisiblePosition;
            currentState = UIPanelState.Secondary;
        }
    }

    // Публичные методы для проверки текущего состояния
    public bool IsMainPanelActive() => currentState == UIPanelState.Main;
    public bool IsSecondaryPanelActive() => currentState == UIPanelState.Secondary;
}