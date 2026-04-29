using UnityEngine;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    private TextMeshProUGUI buttonText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonText = rewardUI.GetComponentInChildren<TextMeshProUGUI>(true);
        rewardUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        bool shouldShow = GameManager.Instance.state == GameManager.GameState.WAVEEND ||
                          GameManager.Instance.state == GameManager.GameState.GAMEOVER ||
                          GameManager.Instance.state == GameManager.GameState.VICTORY;

        if (rewardUI.activeSelf != shouldShow)
        {
            rewardUI.SetActive(shouldShow);
        }

        if (shouldShow && buttonText != null)
        {
            buttonText.text = GameManager.Instance.state == GameManager.GameState.WAVEEND ? "Next Wave" : "Return to Start";
        }
    }
}
