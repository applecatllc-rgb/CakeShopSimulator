using UnityEngine;
using UnityEngine.UI;

public class CustomerOrderUI : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject orderTicket;
    public Button customerButton;
    public RectTransform customer;
    public RectTransform waitingSpot;

    private bool orderTaken = false;

    void Start()
    {
        orderTicket.SetActive(false);
    }

    public void TakeOrder()
    {
        if (orderTaken)
        {
            return;
        }

        orderTaken = true;

        gameManager.GenerateOrder();
        orderTicket.SetActive(true);

        customerButton.interactable = false;

        customer.SetParent(waitingSpot, false);
        customer.anchoredPosition = Vector2.zero;
        customer.localScale = new Vector3(0.45f, 0.45f, 1f);
    }
}