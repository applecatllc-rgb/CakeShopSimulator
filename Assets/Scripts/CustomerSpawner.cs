using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRoot;
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private GameObject orderPrefab;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform waitingPoint;
    [SerializeField] private ScoreManager scoreManager;

    [SerializeField] private int customerCount = 3;
    [SerializeField] private float spawnDelay = 2f;
    [SerializeField] private float waitingSpacing = 190f;
    [SerializeField] private float orderSpacing = 260f;
    [SerializeField] private OrderListManager orderListManager;

    private IEnumerator Start()
    {
        if (canvasRoot == null || customerPrefab == null ||
            orderPrefab == null || spawnPoint == null ||
            waitingPoint == null || scoreManager == null)
        {
            Debug.LogError("CustomerSpawner 有未绑定的字段。", this);
            yield break;
        }

        for (int i = 0; i < customerCount; i++)
        {
            // 为这一位顾客创建独立的等候位置。
            GameObject pointObject = new GameObject(
                $"WaitingPoint_{i + 1}", typeof(RectTransform));

            RectTransform point =
                pointObject.GetComponent<RectTransform>();

            point.SetParent(canvasRoot, false);
            point.position = waitingPoint.position;
            point.localPosition += Vector3.right * waitingSpacing * i;

            // 每位顾客拥有独立的订单。
            GameObject order = Instantiate(orderPrefab, canvasRoot);
            order.SetActive(false);

            GameObject customer = Instantiate(customerPrefab, canvasRoot);
            RectTransform customerRect =
                customer.GetComponent<RectTransform>();

            customerRect.position = spawnPoint.position;
            Transform counter = canvasRoot.Find("Counter");
            if (counter != null)
                customer.transform.SetSiblingIndex(counter.GetSiblingIndex());

            CustomerReception reception =
                customer.GetComponent<CustomerReception>();

            if (reception == null)
            {
                Debug.LogError("顾客 Prefab 缺少 CustomerReception 脚本。");
                Destroy(customer);
                Destroy(order);
                Destroy(pointObject);
                yield break;
            }

            customer.SetActive(true);
            reception.Initialize(order, point, scoreManager);
            reception.SetOrderListManager(orderListManager);
            reception.SetOrderPosition(new Vector2(40f, -40f));

            yield return new WaitUntil(
                () => reception == null || reception.ReceptionFinished);

            if (i < customerCount - 1)
                yield return new WaitForSeconds(spawnDelay);
        }
    }
}
