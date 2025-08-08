using UnityEngine;

public class Botsingdetectie : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
        void OnCollisionEnter(Collision collision)
    {
        // Controleer of het object waarmee we botsen een "Obstacle" tag heeft
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Botsing met obstakel: " + collision.gameObject.name);
            // Hier kun je de game over logica toevoegen:
            // - Game over scherm tonen
            // - Taxi laten exploderen/stoppen
            // - Geluid afspelen
            // - Spel herstarten
        }
    }
}