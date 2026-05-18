using UnityEngine;
using YG;

public class PurchaseManager : MonoBehaviour
{
    [SerializeField] private string allColorsPurchaseId = "all_colors";

    void OnEnable()
    {
        YG2.onPurchaseSuccess += OnPurchaseSuccess;

        // Можно также обработать неудачные покупки
        // YG2.onPurchaseFailed += OnPurchaseFailed;
    }

    void OnDisable()
    {
        YG2.onPurchaseSuccess -= OnPurchaseSuccess;

        // YG2.onPurchaseFailed -= OnPurchaseFailed;
    }

    private void OnPurchaseSuccess(string id)
    {
        if (id == allColorsPurchaseId)
        {
            // Покупка уже обработана в ColorSelectionManager, но можно продублировать на всякий случай
            // Главное, чтобы сохранение было в одном месте. Оставим обработку в ColorSelectionManager.
            Debug.Log($"PurchaseManager: покупка {id} успешна.");
        }
    }

    // Можно добавить метод для консумации, если нужно обработать необработанные покупки при старте
    void Start()
    {
        // Консумация уже будет выполнена компонентом ConsumePurchasesYG на сцене
    }
}