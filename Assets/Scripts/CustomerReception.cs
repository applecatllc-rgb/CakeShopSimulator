using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CustomerReception : MonoBehaviour
{
    [SerializeField] private GameObject orderPanel;
    [SerializeField] private TMP_Text orderText;
    [SerializeField] private RectTransform waitingPoint;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private float itemInterval = 0.2f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private float orderScale = 0.45f;
    [SerializeField] private float customerScale = 0.55f;

    private enum State
    {
        Waiting,
        Revealing,
        OrderReady,
        Moving,
        Queued,
        Left
    }

    private State state;

    private Button customerButton;
    private Button orderButton;
    private RectTransform customerRect;
    private RectTransform orderRect;

    private GameObject fullScreenClickArea;
    private float appearedAt;
    private Vector2 compactOrderPosition = new Vector2(40f, -40f);
    private OrderListManager orderListManager;

    public bool ReceptionFinished =>
        state == State.Queued || state == State.Left;

    public void SetOrderListManager(OrderListManager manager)
    {
        orderListManager = manager;
    }

    public void SetOrderPosition(Vector2 position)
    {
        compactOrderPosition = position;
    }

    private void Awake()
    {
        customerButton = GetComponent<Button>();
        customerRect = GetComponent<RectTransform>();

        if (orderPanel != null)
        {
            orderButton = orderPanel.GetComponent<Button>();
            orderRect = orderPanel.GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        appearedAt = Time.time;
        state = State.Waiting;

        customerButton.interactable = true;
        customerButton.onClick.AddListener(OnCustomerClicked);

        if (orderButton != null)
        {
            orderButton.onClick.AddListener(OnOrderClicked);
            orderButton.interactable = false;
        }

        if (orderPanel != null)
            orderPanel.SetActive(false);
    }

    private void OnDisable()
    {
        HideFullScreenClickArea();

        customerButton.onClick.RemoveListener(OnCustomerClicked);

        if (orderButton != null)
            orderButton.onClick.RemoveListener(OnOrderClicked);

        StopAllCoroutines();
    }

    public void Initialize(
        GameObject ownOrderPanel,
        RectTransform ownWaitingPoint,
        ScoreManager manager)
    {
        HideFullScreenClickArea();

        if (orderButton != null)
            orderButton.onClick.RemoveListener(OnOrderClicked);

        orderPanel = ownOrderPanel;
        orderRect = orderPanel.GetComponent<RectTransform>();
        orderButton = orderPanel.GetComponent<Button>();
        orderText = orderPanel.GetComponentInChildren<TMP_Text>(true);

        waitingPoint = ownWaitingPoint;
        scoreManager = manager;

        orderButton.onClick.AddListener(OnOrderClicked);
        orderButton.interactable = false;
        orderPanel.SetActive(false);

        appearedAt = Time.time;
        state = State.Waiting;
        customerButton.interactable = true;
    }

    private void Update()
    {
        if (state == State.Waiting &&
            Time.time - appearedAt >= 60f)
        {
            LeaveAngrily();
        }
    }

    private void OnCustomerClicked()
    {
        if (state != State.Waiting)
            return;

        if (orderRect == null ||
            orderButton == null ||
            orderText == null ||
            waitingPoint == null)
        {
            Debug.LogError(
                "请绑定订单面板、订单文字和等候位置，并给订单面板添加 Button。",
                this);
            return;
        }

        float elapsed = Time.time - appearedAt;

        if (elapsed >= 60f)
        {
            LeaveAngrily();
            return;
        }

        state = State.Revealing;
        customerButton.interactable = false;

        int points = 6 - Mathf.FloorToInt(elapsed / 10f);

        if (scoreManager != null)
            scoreManager.AddScore(points);

        Debug.Log(
            $"接待顾客：+{points} 分，等待 {elapsed:F1} 秒");

        StartCoroutine(RevealOrder());
    }

    private IEnumerator RevealOrder()
    {
        orderText.text = "";
        orderPanel.SetActive(true);

        string[] items =
        {
            "CupCake",
            " + Strawberry Cream",
            " + Strawberry"
        };

        for (int i = 0; i < items.Length; i++)
        {
            orderText.text += items[i];

            if (i < items.Length - 1)
                yield return new WaitForSeconds(itemInterval);
        }

        state = State.OrderReady;
        orderButton.interactable = true;

        ShowFullScreenClickArea();
    }

    private void ShowFullScreenClickArea()
    {
        HideFullScreenClickArea();

        Canvas canvas = orderPanel.GetComponentInParent<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("订单面板需要放在 Canvas 下。", this);
            return;
        }

        // 在最外层 Canvas 下创建覆盖整个屏幕的透明按钮。
        Transform canvasTransform = canvas.rootCanvas.transform;

        fullScreenClickArea = new GameObject(
            "FullScreenOrderClick",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button)
        );

        RectTransform rect =
            fullScreenClickArea.GetComponent<RectTransform>();

        rect.SetParent(canvasTransform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        Image image = fullScreenClickArea.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = true;

        Button button = fullScreenClickArea.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(OnOrderClicked);
    }

    private void HideFullScreenClickArea()
    {
        if (fullScreenClickArea == null)
            return;

        fullScreenClickArea.SetActive(false);
        Destroy(fullScreenClickArea);
        fullScreenClickArea = null;
    }

    private void OnOrderClicked()
    {
        if (state == State.Queued)
        {
            if (orderListManager != null)
                orderListManager.Show();

            return;
        }

        if (state != State.OrderReady)
            return;

        state = State.Moving;

        // 立即关闭全屏按钮，避免挡住后续操作。
        HideFullScreenClickArea();
        orderButton.interactable = false;

        StartCoroutine(MoveToWaitingArea());
    }

    private IEnumerator MoveToWaitingArea()
    {
        // 订单改用左上角锚点，保留当前画面位置。
        Vector3 originalOrderPosition = orderRect.position;
        Vector2 originalSize = orderRect.rect.size;

        orderRect.anchorMin = new Vector2(0f, 1f);
        orderRect.anchorMax = new Vector2(0f, 1f);
        orderRect.pivot = new Vector2(0f, 1f);
        orderRect.sizeDelta = originalSize;
        orderRect.position = originalOrderPosition;

        Vector2 orderStart = orderRect.anchoredPosition;
        Vector2 orderTarget = compactOrderPosition;

        Vector3 customerStart = customerRect.localPosition;
        Vector3 customerTarget =
            customerRect.parent.InverseTransformPoint(
                waitingPoint.position);

        customerTarget.z = customerStart.z;

        Vector3 orderStartScale = orderRect.localScale;
        Vector3 customerStartScale = customerRect.localScale;

        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(elapsed / duration));

            orderRect.anchoredPosition =
                Vector2.Lerp(orderStart, orderTarget, t);

            orderRect.localScale = Vector3.Lerp(
                orderStartScale,
                Vector3.one * orderScale,
                t);

            customerRect.localPosition =
                Vector3.Lerp(customerStart, customerTarget, t);

            customerRect.localScale = Vector3.Lerp(
                customerStartScale,
                Vector3.one * customerScale,
                t);

            yield return null;
        }

        orderRect.anchoredPosition = orderTarget;
        orderRect.localScale = Vector3.one * orderScale;

        customerRect.localPosition = customerTarget;
        customerRect.localScale = Vector3.one * customerScale;

        state = State.Queued;
        orderButton.interactable = true;

        // 最新收起的订单盖到旧订单上方。
        orderRect.SetAsLastSibling();

        if (orderListManager != null)
            orderListManager.RegisterOrder(orderText.text);

        Debug.Log("订单已收起，顾客进入等候区。");
    }

    private void LeaveAngrily()
    {
        state = State.Left;
        HideFullScreenClickArea();

        if (scoreManager != null)
            scoreManager.AddScore(-10);

        Debug.Log("顾客生气离开：-10 分");
        gameObject.SetActive(false);
    }
}