using UnityEngine;
using TMPro;

public class WaveLabelController : MonoBehaviour
{
    TextMeshProUGUI tmp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME)
        {
            tmp.text = "Choose a level";
        }
        else if (GameManager.Instance.state == GameManager.GameState.COUNTDOWN)
        {
            tmp.text = "Wave " + GameManager.Instance.wave + " starting in " + GameManager.Instance.countdown;
        }
        else if (GameManager.Instance.state == GameManager.GameState.INWAVE)
        {
            string waveText = "Wave " + GameManager.Instance.wave;
            if (GameManager.Instance.maxWaves > 0)
            {
                waveText += "/" + GameManager.Instance.maxWaves;
            }
            tmp.text = waveText + " - Enemies left: " + GameManager.Instance.enemy_count;
        }
        else if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            tmp.text = "Wave " + GameManager.Instance.wave + " complete - defeated " +
                       GameManager.Instance.enemiesDefeated + "/" + GameManager.Instance.enemiesSpawned;
        }
        else if (GameManager.Instance.state == GameManager.GameState.GAMEOVER ||
                 GameManager.Instance.state == GameManager.GameState.VICTORY)
        {
            tmp.text = GameManager.Instance.resultMessage + " Defeated " +
                       GameManager.Instance.enemiesDefeated + "/" + GameManager.Instance.enemiesSpawned;
        }
    }
}
