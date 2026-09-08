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
    }

    public void GenerateOrder()
    {
        currentCake = "Cupcake";

        string[] flavors = { "Vanilla", "Chocolate" };
        currentFlavor = flavors[Random.Range(0, flavors.Length)];

        string[] toppings = { "Strawberry", "Blueberry" };
        currentTopping = toppings[Random.Range(0, toppings.Length)];

        orderText.text =
            "ORDER\n\n" +
            currentCake +
            "\n\nCream:\n" +
            currentFlavor +
            "\n\nTopping:\n" +
            currentTopping;
    }

    public void SelectCupcake()
    {
        selectedCake = "Cupcake";
    }

    public void SelectVanilla()
    {
        selectedFlavor = "Vanilla";
    }

    public void SelectChocolate()
    {
        selectedFlavor = "Chocolate";
    }

    public void SelectStrawberry()
    {
        selectedTopping = "Strawberry";
    }

    public void SelectBlueberry()
    {
        selectedTopping = "Blueberry";
    }

    public void Serve()
    {
        if (selectedCake == currentCake &&
            selectedFlavor == currentFlavor &&
            selectedTopping == currentTopping)
        {
            score += 100;
            UpdateScoreText();
            ResetSelection();

            Debug.Log("Perfect! Order completed! +100");
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

    void ResetSelection()
    {
        selectedCake = "";
        selectedFlavor = "";
        selectedTopping = "";
    }
}