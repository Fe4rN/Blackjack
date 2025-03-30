using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Asegúrate de incluir esta línea

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text PlayerPoints;
    public Text DealerPoints;
    public Text playerBank;  // Texto para mostrar la banca del jugador
    public TMP_Dropdown betOptions;  // Dropdown para seleccionar la apuesta

    public int[] values = new int[52];
    int cardIndex = 0;
    private int playerBalance = 1000;  // Banca inicial del jugador
    private int currentBet = 10;  // Apuesta actual inicializada a 10

    private void Awake()
    {
        InitCardValues();
        UpdateBankDisplay();  // Mostrar la banca inicial
        InitializeBetOptions();  // Inicializar las opciones de apuesta
    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
    }

    private void InitCardValues()
    {
        for (int i = 0; i < 52; i++)
        {
            int cardValue = (i % 13) + 1;
            if (cardValue > 10) cardValue = 10;
            if (cardValue == 1) cardValue = 11;
            values[i] = cardValue;
        }
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < 52; i++)
        {
            int randomIndex = Random.Range(i, 52);
            (faces[i], faces[randomIndex]) = (faces[randomIndex], faces[i]);
            (values[i], values[randomIndex]) = (values[randomIndex], values[i]);
        }
        cardIndex = 0;
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        if (player.GetComponent<CardHand>().points == 21 || dealer.GetComponent<CardHand>().points == 21)
        {
            finalMessage.text = "¡Blackjack!";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    private void CalculateProbabilities()
    {
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerKnownValue = (dealer.GetComponent<CardHand>().cards.Count > 1) ? dealer.GetComponent<CardHand>().cards[1].GetComponent<CardModel>().value : 0;
        int remainingCards = 52 - cardIndex;
        if (remainingCards == 0) return;

        int higherDealer = 0, safeDraw = 0, bust = 0;

        for (int i = cardIndex; i < 52; i++)
        {
            int newCardValue = values[i];
            int simulatedDealerPoints = dealerKnownValue + newCardValue;
            while (simulatedDealerPoints < 17 && i + 1 < 52)
            {
                simulatedDealerPoints += values[i + 1];
            }

            if (simulatedDealerPoints > playerPoints) higherDealer++;
            if (playerPoints + newCardValue >= 17 && playerPoints + newCardValue <= 21) safeDraw++;
            if (playerPoints + newCardValue > 21) bust++;
        }

        probMessage.text = $"Deal > Play: {((float)higherDealer / remainingCards) * 100:F1}%\n" +
                           $"17<=X<=21: {((float)safeDraw / remainingCards) * 100:F1}%\n" +
                           $"X > 21: {((float)bust / remainingCards) * 100:F1}%";
    }

    void PushDealer()
    {
        if (cardIndex >= values.Length)
        {
            Debug.LogWarning("Se agotaron las cartas. No se puede sacar más.");
            return;
        }

        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        dealer.GetComponent<CardHand>().cards[^1].GetComponent<CardModel>().ToggleFace(dealer.GetComponent<CardHand>().cards.Count > 1);

        if (dealer.GetComponent<CardHand>().cards.Count > 1)
        {
            DealerPoints.text = $"{values[cardIndex]} + ?";
        }
        else
        {
            DealerPoints.text = $"{values[cardIndex]}";
        }

        cardIndex++;
    }

    void PushPlayer()
    {
        if (cardIndex >= values.Length)
        {
            Debug.LogWarning("Se agotaron las cartas. No se puede sacar más.");
            return;
        }

        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        PlayerPoints.text = $"Puntos: {player.GetComponent<CardHand>().points}";
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        PushPlayer();

        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "¡Te pasaste de 21! Pierdes.";
            UpdateBankAfterLoss();  // Actualizar la banca después de perder
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        while (dealer.GetComponent<CardHand>().points < 17)
        {
            PushDealer();
        }

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        DealerPoints.text = $"Puntos: {dealerPoints}";

        if (dealerPoints > 21 || playerPoints > dealerPoints)
        {
            finalMessage.text = "¡Ganaste!";
            UpdateBankAfterWin();  // Actualizar la banca después de ganar
        }
        else if (playerPoints == dealerPoints)
        {
            finalMessage.text = "¡Empate!";
        }
        else
        {
            finalMessage.text = "¡El dealer gana!";
            UpdateBankAfterLoss();  // Actualizar la banca después de perder
        }

        hitButton.interactable = false;
        stickButton.interactable = false;
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        PlayerPoints.text = "Puntos: 0";
        DealerPoints.text = "Puntos: ?";
        ShuffleCards();
        StartGame();
    }

    // Inicializa las opciones de apuesta en el dropdown
    private void InitializeBetOptions()
    {
        betOptions.ClearOptions();
        var options = new List<string>();
        
        // Añadir opciones de apuesta
        for (int i = 10; i <= playerBalance; i += 10)
        {
            options.Add(i.ToString());
        }

        betOptions.AddOptions(options);
        betOptions.value = 0;  // Establecer la opción predeterminada a 0 (10 euros)
        betOptions.RefreshShownValue();  // Refrescar el dropdown para mostrar el valor predeterminado

        // Establecer la apuesta inicial en base a la selección del dropdown
        currentBet = int.Parse(options[0]); // Establecer la apuesta inicial a 10
    }

    // Método llamado cuando el jugador cambia la apuesta en el dropdown
    public void OnBetChanged()
    {
        currentBet = int.Parse(betOptions.options[betOptions.value].text); // Actualizar la apuesta actual según la opción seleccionada
        UpdateBankDisplay();  // Actualizar la pantalla de banca
    }

    // Actualizar la pantalla de banca
    private void UpdateBankDisplay()
    {
        playerBank.text = $"Banca: {playerBalance} €";
    }

    // Actualizar la banca después de ganar
    private void UpdateBankAfterWin()
    {
        playerBalance += currentBet * 2;  // Doblar la apuesta
        UpdateBankDisplay();
    }

    // Actualizar la banca después de perder
    private void UpdateBankAfterLoss()
    {
        playerBalance -= currentBet;  // Perder la apuesta
        UpdateBankDisplay();
    }
}
