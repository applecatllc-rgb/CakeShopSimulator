using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderListManager : MonoBehaviour
{
    [SerializeField] private GameObject listPanel;
    [SerializeField] private RectTransform orderGrid;
    [SerializeField] private GameObject orderPrefab;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        listPanel.SetActive(false);
        closeButton.onClick.AddListener(Hide);
    }

    public void RegisterOrder(string content)
    {
        GameObject card = Instantiate(orderPrefab, orderGrid);
        card.SetActive(false);

        RectTransform rect = card.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        // 列表里的卡片只负责展示。
        Button button = card.GetComponent<Button>();
        if (button != null)
            button.enabled = false;

        foreach (Graphic graphic in card.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;

        TMP_Text text = card.GetComponentInChildren<TMP_Text>(true);
        text.text = content;
        text.fontSize = 28;
        text.color = new Color(0.25f, 0.15f, 0.1f);

        // 按注册顺序排列：旧订单在前，新订单在后。
        card.transform.SetAsLastSibling();
        card.SetActive(true);
    }

    public void Show()
    {
        listPanel.SetActive(true);
        listPanel.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        listPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);
    }
}
