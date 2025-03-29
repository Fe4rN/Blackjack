using UnityEngine;
using UnityEngine.UI;

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

    public int[] values = new int[52];
    int cardIndex = 0;

    private bool dealerFirstCardRevealed = false; // Variable de control

    private void Awake()
    {
        InitCardValues();

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();
    }

    private void InitCardValues()
    {
        // Asignamos valores a cada carta según las reglas del Blackjack
        for (int i = 0; i < 52; i++)
        {
            int cardValue = (i % 13) + 1; // Valores de 1 a 13 (As a Rey)

            if (cardValue > 10)
                cardValue = 10; // J, Q y K valen 10

            if (cardValue == 1)
                cardValue = 11; // Los Ases valen 11 por defecto

            values[i] = cardValue;
        }
    }



    private void ShuffleCards()
    {
        // Algoritmo de Fisher-Yates para barajar
        for (int i = 0; i < 52; i++)
        {
            int randomIndex = Random.Range(i, 52);

            // Intercambiamos en "faces"
            Sprite tempFace = faces[i];
            faces[i] = faces[randomIndex];
            faces[randomIndex] = tempFace;

            // Intercambiamos en "values"
            int tempValue = values[i];
            values[i] = values[randomIndex];
            values[randomIndex] = tempValue;
        }

        cardIndex = 0; // Reiniciamos el índice
    }



    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        // Comprobamos si hay blackjack
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
        int dealerKnownValue = 0;
        if (dealer.GetComponent<CardHand>().cards.Count > 1)
        {
            dealerKnownValue = dealer.GetComponent<CardHand>().cards[1].GetComponent<CardModel>().value;
        }
        int remainingCards = 52 - cardIndex;

        int higherDealer = 0, safeDraw = 0, bust = 0;

        for (int i = cardIndex; i < 52; i++)
        {
            int newCardValue = values[i];

            if (dealerKnownValue + newCardValue > playerPoints)
                higherDealer++;

            if (playerPoints + newCardValue >= 17 && playerPoints + newCardValue <= 21)
                safeDraw++;

            if (playerPoints + newCardValue > 21)
                bust++;
        }

        probMessage.text = $"Deal > Play: {((float)higherDealer / remainingCards) * 100:F1}%\n" +
                           $"17<=X<=21: {((float)safeDraw / remainingCards) * 100:F1}%\n" +
                           $"X > 21: {((float)bust / remainingCards) * 100:F1}%";
    }



    void PushDealer()
    {
        if (cardIndex >= values.Length)  // Evita desbordamiento
        {
            Debug.LogError("No hay más cartas en el mazo. Barajando nuevamente...");
            ShuffleCards();
            cardIndex = 0;
        }

        // Añadimos la carta a la mano del dealer
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);

        // Si es la primera carta del dealer, la dejamos oculta
        if (dealer.GetComponent<CardHand>().cards.Count == 1)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(false);
        }
        else
        {
            // Las demás cartas sí se revelan
            dealer.GetComponent<CardHand>().cards[^1].GetComponent<CardModel>().ToggleFace(true);
        }

        cardIndex++;
    }


    void PushPlayer()
    {
        if (cardIndex >= values.Length)  // Evita desbordamiento
        {
            Debug.LogError("No hay más cartas en el mazo. Barajando nuevamente...");
            ShuffleCards();
            cardIndex = 0;
        }
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        PushPlayer();

        // Si el jugador se pasa de 21, pierde
        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "¡Te pasaste de 21! Pierdes.";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }




    public void Stand()
{
    // Revelamos la carta oculta del dealer
    dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

    // El dealer saca cartas hasta llegar a 17 o más
    while (dealer.GetComponent<CardHand>().points < 17)
    {
        PushDealer();
    }

    // Evaluamos el resultado
    int playerPoints = player.GetComponent<CardHand>().points;
    int dealerPoints = dealer.GetComponent<CardHand>().points;

    if (dealerPoints > 21 || playerPoints > dealerPoints)
    {
        finalMessage.text = "¡Ganaste!";
    }
    else if (playerPoints == dealerPoints)
    {
        finalMessage.text = "¡Empate!";
    }
    else
    {
        finalMessage.text = "¡El dealer gana!";
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
        cardIndex = 0;
        ShuffleCards();
        StartGame();

        dealerFirstCardRevealed = false; // Restablecemos la variable
    }


}
