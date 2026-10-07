using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class EggInteraction : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerClickHandler
{
    [Header("检测区域")]
    [SerializeField] private RectTransform leftCrackZone;
    [SerializeField] private RectTransform rightCrackZone;
    [SerializeField] private RectTransform pourZone;

    [Header("临时视觉效果")]
    [SerializeField] private Image bowlImage;
    [SerializeField] private Color crackedColor =
        new Color(0.85f, 0.65f, 0.4f);
    [SerializeField] private Color eggInBowlColor =
        new Color(1f, 0.85f, 0.35f);

    [Header("敲击设置")]
    [Tooltip("向下敲击的最低速度，单位为 Canvas 单位/秒")]
    [SerializeField] private float minimumDownSpeed = 450f;

    [Tooltip("鸡蛋底部需要先抬到碗沿上方的距离")]
    [SerializeField] private float liftDistance = 35f;

    [Header("完成事件，暂时可以留空")]
    [SerializeField] private UnityEvent onEggAdded = new UnityEvent();
    [SerializeField] private UnityEvent onEggWasted = new UnityEvent();

    private enum EggState { Whole, Cracked, Added }
    private EggState state = EggState.Whole;

    private RectTransform eggRect;
    private RectTransform parentRect;
    private Image eggImage;

    private Vector2 homePosition;
    private Color wholeColor;
    private int homeSiblingIndex;

    private Vector2 dragOffset;
    private Vector2 previousBottom;
    private float previousDragTime;

    private bool dragging;
    private bool leftArmed;
    private bool rightArmed;

    public bool EggAdded => state == EggState.Added;

    private void Awake()
    {
        eggRect = GetComponent<RectTransform>();
        parentRect = eggRect.parent as RectTransform;
        eggImage = GetComponent<Image>();

        homePosition = eggRect.anchoredPosition;
        homeSiblingIndex = eggRect.GetSiblingIndex();
        wholeColor = eggImage.color;
    }

    // 鸡蛋底部中点，转换到父物体的坐标中。
    private Vector2 GetEggBottom()
    {
        Vector3 worldPoint = eggRect.TransformPoint(
            new Vector3(eggRect.rect.center.x, eggRect.rect.yMin, 0f));

        return parentRect.InverseTransformPoint(worldPoint);
    }

    // 检测区顶部左右两端，转换到鸡蛋父物体的坐标中。
    private void GetZoneEdge(
        RectTransform zone,
        out Vector2 left,
        out Vector2 right)
    {
        Vector3 worldLeft = zone.TransformPoint(
            new Vector3(zone.rect.xMin, zone.rect.yMax, 0f));

        Vector3 worldRight = zone.TransformPoint(
            new Vector3(zone.rect.xMax, zone.rect.yMax, 0f));

        left = parentRect.InverseTransformPoint(worldLeft);
        right = parentRect.InverseTransformPoint(worldRight);
    }

    private void UpdateArmedZones(Vector2 bottom)
    {
        if (leftCrackZone != null)
        {
            GetZoneEdge(leftCrackZone, out Vector2 left, out _);

            if (bottom.y >= left.y + liftDistance)
                leftArmed = true;
        }

        if (rightCrackZone != null)
        {
            GetZoneEdge(rightCrackZone, out Vector2 left, out _);

            if (bottom.y >= left.y + liftDistance)
                rightArmed = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (state == EggState.Added || parentRect == null)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            return;
        }

        dragging = true;

        dragOffset =
            (Vector2)eggRect.localPosition - pointerPosition;

        previousBottom = GetEggBottom();
        previousDragTime = Time.unscaledTime;

        leftArmed = false;
        rightArmed = false;
        UpdateArmedZones(previousBottom);

        eggRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            return;
        }

        Vector3 position = eggRect.localPosition;
        position.x = pointerPosition.x + dragOffset.x;
        position.y = pointerPosition.y + dragOffset.y;
        eggRect.localPosition = position;

        Vector2 currentBottom = GetEggBottom();

        float now = Time.unscaledTime;
        float deltaTime = Mathf.Max(
            now - previousDragTime, 0.001f);

        float downSpeed =
            (previousBottom.y - currentBottom.y) / deltaTime;

        if (state == EggState.Whole)
        {
            bool hitLeft = TryHitEdge(
                leftCrackZone,
                leftArmed,
                previousBottom,
                currentBottom,
                downSpeed,
                out Vector2 leftHit);

            bool hitRight = TryHitEdge(
                rightCrackZone,
                rightArmed,
                previousBottom,
                currentBottom,
                downSpeed,
                out Vector2 rightHit);

            if (hitLeft || hitRight)
            {
                Vector2 hit = hitLeft ? leftHit : rightHit;

                // 停在碰撞位置，避免这一帧穿过碗沿。
                Vector2 correction = hit - currentBottom;

                Vector3 correctedPosition = eggRect.localPosition;
                correctedPosition.x += correction.x;
                correctedPosition.y += correction.y;
                eggRect.localPosition = correctedPosition;

                state = EggState.Cracked;
                eggImage.color = crackedColor;

                // 更新偏移，后续拖动不会突然跳回鼠标位置。
                dragOffset =
                    (Vector2)eggRect.localPosition - pointerPosition;

                Debug.Log(
                    "鸡蛋已敲裂，可以拖到碗口中间再点击打开。",
                    this);
            }
            else
            {
                UpdateArmedZones(currentBottom);

                // 穿过碗沿却没有敲裂，要重新抬高再敲。
                DisarmBelowEdge(leftCrackZone, currentBottom, ref leftArmed);
                DisarmBelowEdge(rightCrackZone, currentBottom, ref rightArmed);
            }
        }

        previousBottom = GetEggBottom();
        previousDragTime = now;
    }

    private bool TryHitEdge(
        RectTransform zone,
        bool armed,
        Vector2 previous,
        Vector2 current,
        float downSpeed,
        out Vector2 hit)
    {
        hit = Vector2.zero;

        if (zone == null || !armed ||
            downSpeed < minimumDownSpeed)
        {
            return false;
        }

        GetZoneEdge(zone, out Vector2 left, out Vector2 right);

        float edgeY = left.y;

        // 必须从碗沿上方向下穿过。
        if (previous.y <= edgeY || current.y > edgeY)
            return false;

        // 检测运动路径，快速拖动也不会漏掉窄边界。
        float t = (previous.y - edgeY) /
                  (previous.y - current.y);

        float hitX = Mathf.Lerp(previous.x, current.x, t);

        if (hitX < Mathf.Min(left.x, right.x) ||
            hitX > Mathf.Max(left.x, right.x))
        {
            return false;
        }

        hit = new Vector2(hitX, edgeY);
        return true;
    }

    private void DisarmBelowEdge(
        RectTransform zone,
        Vector2 bottom,
        ref bool armed)
    {
        if (zone == null)
            return;

        GetZoneEdge(zone, out Vector2 left, out _);

        if (bottom.y <= left.y)
            armed = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;

        dragging = false;

        // 拖动结束不算点击，必须另点一次才能打开。
        eventData.eligibleForClick = false;

        if (state == EggState.Whole)
        {
            eggRect.anchoredPosition = homePosition;
            eggRect.SetSiblingIndex(homeSiblingIndex);
        }

        // 裂开的鸡蛋留在松手位置，可以再次拖动。
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dragging || eventData.dragging ||
            state != EggState.Cracked)
        {
            return;
        }

        if (pourZone == null)
        {
            Debug.LogError("请绑定 Pour Zone。", this);
            return;
        }

        Vector3 worldBottom =
            parentRect.TransformPoint(GetEggBottom());

        Vector3 localBottom =
            pourZone.InverseTransformPoint(worldBottom);

        bool insideBowl = pourZone.rect.Contains(
            new Vector2(localBottom.x, localBottom.y));

        if (insideBowl)
        {
            state = EggState.Added;

            if (bowlImage != null)
                bowlImage.color = eggInBowlColor;

            Debug.Log("蛋液成功加入碗中。", this);

            onEggAdded.Invoke();
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("蛋液掉到碗外，补一颗新鸡蛋。", this);

            ResetEgg();
            onEggWasted.Invoke();
        }
    }

    private void ResetEgg()
    {
        state = EggState.Whole;
        dragging = false;
        leftArmed = false;
        rightArmed = false;

        eggImage.color = wholeColor;
        eggRect.anchoredPosition = homePosition;
        eggRect.SetSiblingIndex(homeSiblingIndex);
    }
}