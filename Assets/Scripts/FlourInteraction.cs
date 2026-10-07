using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class FlourInteraction : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private RectTransform pourZone;

    [Header("倾倒动画")]
    [SerializeField] private float tiltAngle = 65f;
    [SerializeField] private float tiltDuration = 0.25f;
    [SerializeField] private float pourDuration = 0.8f;
    [SerializeField] private float returnDuration = 0.35f;

    [Header("完成事件，暂时留空")]
    [SerializeField] private UnityEvent onFlourAdded =
        new UnityEvent();

    private enum State { Ready, Pouring, Added }
    private State state = State.Ready;

    private RectTransform flourRect;
    private RectTransform parentRect;

    private Vector2 homePosition;
    private Quaternion homeRotation;
    private int homeSiblingIndex;
    private Vector2 dragOffset;
    private bool dragging;

    public bool FlourAdded => state == State.Added;

    private void Awake()
    {
        flourRect = GetComponent<RectTransform>();
        parentRect = flourRect.parent as RectTransform;

        homePosition = flourRect.anchoredPosition;
        homeRotation = flourRect.localRotation;
        homeSiblingIndex = flourRect.GetSiblingIndex();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (state != State.Ready || parentRect == null)
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
            (Vector2)flourRect.localPosition - pointerPosition;

        flourRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || state != State.Ready)
            return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            return;
        }

        Vector3 position = flourRect.localPosition;
        position.x = pointerPosition.x + dragOffset.x;
        position.y = pointerPosition.y + dragOffset.y;
        flourRect.localPosition = position;

        if (IsAboveBowl())
        {
            state = State.Pouring;
            dragging = false;
            eventData.eligibleForClick = false;

            StartCoroutine(PourFlour());
        }
    }

    private bool IsAboveBowl()
    {
        if (pourZone == null)
            return false;

        Vector3 worldBottom = flourRect.TransformPoint(
            new Vector3(
                flourRect.rect.center.x,
                flourRect.rect.yMin,
                0f));

        Vector3 localBottom =
            pourZone.InverseTransformPoint(worldBottom);

        return pourZone.rect.Contains(
            new Vector2(localBottom.x, localBottom.y));
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        eventData.eligibleForClick = false;

        // 自动倾倒开始后，由动画负责归位。
        if (state != State.Ready)
            return;

        flourRect.anchoredPosition = homePosition;
        flourRect.localRotation = homeRotation;
        flourRect.SetSiblingIndex(homeSiblingIndex);
    }

    private IEnumerator PourFlour()
    {
        Quaternion startRotation = flourRect.localRotation;
        Quaternion tiltedRotation =
            homeRotation * Quaternion.Euler(0f, 0f, tiltAngle);

        float duration = Mathf.Max(0.01f, tiltDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / duration));

            flourRect.localRotation = Quaternion.Slerp(
                startRotation, tiltedRotation, t);

            yield return null;
        }

        flourRect.localRotation = tiltedRotation;

        // 暂时停留表示倒面粉，后续在这里加入面粉下落效果。
        yield return new WaitForSeconds(Mathf.Max(0f, pourDuration));

        state = State.Added;
        Debug.Log("面粉已加入碗中。", this);
        onFlourAdded.Invoke();

        Vector2 returnStart = flourRect.anchoredPosition;
        Quaternion returnRotation = flourRect.localRotation;

        duration = Mathf.Max(0.01f, returnDuration);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / duration));

            flourRect.anchoredPosition =
                Vector2.Lerp(returnStart, homePosition, t);

            flourRect.localRotation = Quaternion.Slerp(
                returnRotation, homeRotation, t);

            yield return null;
        }

        flourRect.anchoredPosition = homePosition;
        flourRect.localRotation = homeRotation;
        flourRect.SetSiblingIndex(homeSiblingIndex);
    }
}
