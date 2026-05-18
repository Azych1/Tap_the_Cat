using UnityEngine;
using UnityEngine.UI;

public class SwitchPanelButton : MonoBehaviour
{
    [SerializeField] private bool switchToSecondary = true;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (UIManager.Instance != null)
        {
            if (switchToSecondary)
            {
                UIManager.Instance.SwitchToSecondaryPanel();
            }
            else
            {
                UIManager.Instance.SwitchToMainPanel();
            }

            // Проигрываем звук клика, если есть SoundManager
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayMainClick();
            }
        }
        else
        {
            Debug.LogError("UIManager.Instance is null!");
        }
    }
}