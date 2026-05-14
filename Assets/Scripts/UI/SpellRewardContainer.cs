using TMPro;
using UnityEngine;

public class SpellRewardContainer : MonoBehaviour
{
    public GameObject spellDropUI;
    private TextMeshProUGUI buttonText;
    private TextMeshProUGUI messageText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonText = spellDropUI.GetComponentInChildren<TextMeshProUGUI>(true);
        GameObject message = new GameObject("Reward Message");
        message.transform.SetParent(spellDropUI.transform, false);
        messageText = message.AddComponent<TextMeshProUGUI>();
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 16;
        messageText.color = Color.black;
    }

    // Update is called once per frame
    void Update()
    {
        spellDropUI.SetActive(GameManager.Instance.state == GameManager.GameState.WAVEEND);

    }
}