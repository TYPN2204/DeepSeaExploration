using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameOverTrigger : MonoBehaviour
{
    [Header("Settings")]
    public float triggerDelaySeconds = 3f; // Chờ 3 giây

    private HashSet<Jellyfish> jellyfishInZone = new HashSet<Jellyfish>();
    private Dictionary<Jellyfish, float> entryTimes = new Dictionary<Jellyfish, float>();
    private bool gameOverTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameOverTriggered) return;

        Jellyfish jelly = collision.GetComponent<Jellyfish>();
        
        if (jelly != null && !jellyfishInZone.Contains(jelly))
        {
            jellyfishInZone.Add(jelly);
            entryTimes[jelly] = Time.time;
            
            Debug.Log($"Jellyfish entered danger zone. Count: {jellyfishInZone.Count}");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Jellyfish jelly = collision.GetComponent<Jellyfish>();
        
        if (jelly != null && jellyfishInZone.Contains(jelly))
        {
            jellyfishInZone.Remove(jelly);
            entryTimes.Remove(jelly);
            
            Debug.Log($"Jellyfish left danger zone. Count: {jellyfishInZone.Count}");
        }
    }

    private void Update()
    {
        if (gameOverTriggered) return;
        if (jellyfishInZone.Count == 0) return;

        // Kiểm tra xem có jellyfish nào ở trong zone quá lâu không
        foreach (var jelly in jellyfishInZone)
        {
            if (jelly == null) continue;

            float timeInZone = Time.time - entryTimes[jelly];
            
            if (timeInZone >= triggerDelaySeconds)
            {
                Debug.Log($"Game Over! Jellyfish stayed {timeInZone}s in danger zone");
                TriggerGameOver();
                break;
            }
        }
    }

    private void TriggerGameOver()
    {
        gameOverTriggered = true;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}