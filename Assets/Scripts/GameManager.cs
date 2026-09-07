using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public string selectedCake = "";
    public string selectedFlavor = "";
    public string selectedTopping = "";

    public string currentCake = "";
    public string currentFlavor = "";
    public string currentTopping = "";

    public int score = 0;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI orderText;

    void Start()
    {
        UpdateScoreText();
        GenerateOrder();
    }

    public void SelectCupcake()
    {
        selectedCake = "Cupcake";
        Debug.Log("Player selected cake: " + selectedCake);
    }

    public void SelectVanilla()
    {
        selectedFlavor = "Vanilla";
        Debug.Log("Player selected flavor: " + selectedFlavor);
    }

    public void SelectChocolate()
    {
        selectedFlavor = "Chocolate";
        Debug.Log("Player selected flavor: " + selectedFlavor);
    }

    public void SelectBlueberry()
    {
        selectedTopping = "Blueberry";
        Debug.Log("Player selected topping: " + selectedTopping);
    }

    public void Serve()
    {
        if (selectedCake == currentCake &&
            selectedFlavor == currentFlavor &&
            selectedTopping == currentTopping)
        {
            score += 100;
            UpdateScoreText();

            Debug.Log("Perfect! Order completed! +100");

            ResetSelection();
            GenerateOrder();
        }
        else
        {
            Debug.Log("Wrong order!");
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score: " + score;
    }

    void GenerateOrder()
    {
        currentCake = "Cupcake";
        string[] flavors = { "Vanilla", "Chocolate" };
        currentFlavor = flavors[Random.Range(0, flavors.Length)];
        string[] toppings = { "Strawberry", "Blueberry" };
        currentTopping = toppings[Random.Range(0, toppings.Length)];

        orderText.text =
            "Order: " +
            currentFlavor + " " +
            currentTopping + " " +
            currentCake;

        Debug.Log("New order: " +
                  currentFlavor + " " +
                  currentTopping + " " +
                  currentCake);
    }

    void ResetSelection()
    {
        selectedCake = "";
        selectedFlavor = "";
        selectedTopping = "";

        Debug.Log("Selection reset.");
    }
}