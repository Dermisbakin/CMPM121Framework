using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    private TextMeshProUGUI buttonText;
    private TextMeshProUGUI messageText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Spell pendingSpell;
    private GameObject takeButton;
    private GameObject skipButton;
    private TextMeshProUGUI spellInfoText;

    void Start()
    {
        buttonText = rewardUI.GetComponentInChildren<TextMeshProUGUI>(true);

        GameObject message = new GameObject("Reward Message");
        message.transform.SetParent(rewardUI.transform, false);
        messageText = message.AddComponent<TextMeshProUGUI>();
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.fontSize = 32;
        messageText.color = Color.black;

        RectTransform rect = messageText.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.15f, 0.55f);
        rect.anchorMax = new Vector2(0.85f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject spellInfo = new GameObject("Spell Info");
        spellInfo.transform.SetParent(rewardUI.transform, false);
        spellInfoText = spellInfo.AddComponent<TextMeshProUGUI>();
        spellInfoText.alignment = TextAlignmentOptions.Center;
        spellInfoText.fontSize = 24;
        spellInfoText.color = Color.black;

        RectTransform spellRect = spellInfoText.GetComponent<RectTransform>();
        spellRect.anchorMin = new Vector2(0.15f, 0.30f);
        spellRect.anchorMax = new Vector2(0.85f, 0.55f);
        spellRect.offsetMin = Vector2.zero;
        spellRect.offsetMax = Vector2.zero;

        takeButton = CreateButton("Take Spell", new Vector2(0.25f, 0.15f), new Vector2(0.45f, 0.28f));
        takeButton.GetComponent<Button>().onClick.AddListener(TakeSpell);

        skipButton = CreateButton("Skip", new Vector2(0.55f, 0.15f), new Vector2(0.75f, 0.28f));
        skipButton.GetComponent<Button>().onClick.AddListener(SkipSpell);

        rewardUI.SetActive(false);
    }

    // Update is called once per frame
    GameObject CreateButton(string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject btn = new GameObject(label);
        btn.transform.SetParent(rewardUI.transform, false);

        Image img = btn.AddComponent<Image>();
        img.color = new Color(0.8f, 0.8f, 0.8f);
        btn.AddComponent<Button>();

        RectTransform rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btn.transform, false);
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;

        return btn;
    }

    void Update()
    {
        bool shouldShow = GameManager.Instance.state == GameManager.GameState.WAVEEND ||
                          GameManager.Instance.state == GameManager.GameState.GAMEOVER ||
                          GameManager.Instance.state == GameManager.GameState.VICTORY;

        if (rewardUI.activeSelf != shouldShow)
        {
            rewardUI.SetActive(shouldShow);
            if (shouldShow && GameManager.Instance.state == GameManager.GameState.WAVEEND)
            {
                GenerateReward();
            }
        }

        if (shouldShow && buttonText != null)
        {
            bool isWaveEnd = GameManager.Instance.state == GameManager.GameState.WAVEEND;
            buttonText.text = isWaveEnd ? "Next Wave" : "Return to Start";
            messageText.text = GetMessage();

            takeButton.SetActive(isWaveEnd && pendingSpell != null);
            skipButton.SetActive(isWaveEnd && pendingSpell != null);
            spellInfoText.gameObject.SetActive(isWaveEnd);
        }
    }

    void GenerateReward()
    {
        SpellCaster sc = GameManager.Instance.player.GetComponent<PlayerController>().spellcaster;
        pendingSpell = new SpellBuilder().BuildRandom(sc);

        spellInfoText.text = "New Spell: " + pendingSpell.GetName() +
                             "\nDamage: " + pendingSpell.GetDamage() +
                             "  Mana: " + pendingSpell.GetManaCost() +
                             "  CD: " + pendingSpell.GetCooldown().ToString("F1") + "s";
    }

    void TakeSpell()
    {
        if (pendingSpell == null) return;

        SpellCaster sc = GameManager.Instance.player.GetComponent<PlayerController>().spellcaster;

        if (!sc.EquipSpell(pendingSpell))
        {
            sc.DropSpell(0);
            sc.EquipSpell(pendingSpell);
        }

        pendingSpell = null;
        spellInfoText.text = "Spell equipped!";
        takeButton.SetActive(false);
        skipButton.SetActive(false);
    }

    void SkipSpell()
    {
        pendingSpell = null;
        spellInfoText.text = "Spell skipped.";
        takeButton.SetActive(false);
        skipButton.SetActive(false);
    }

    string GetMessage()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            return "Wave " + GameManager.Instance.wave + " complete\nEnemies defeated: " +
                   GameManager.Instance.enemiesDefeated + "/" + GameManager.Instance.enemiesSpawned;
        }
        return GameManager.Instance.resultMessage + "\nEnemies defeated: " +
               GameManager.Instance.enemiesDefeated + "/" + GameManager.Instance.enemiesSpawned;
    }
}