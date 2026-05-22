using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    private List<Relic> relicChoices = new List<Relic>();
    private List<GameObject> relicButtons = new List<GameObject>();
    private TextMeshProUGUI relicHeaderText;

    private List<string> ownedRelicNames = new List<string>();

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

        GameObject relicHeader = new GameObject("Relic Header");
        relicHeader.transform.SetParent(rewardUI.transform, false);
        relicHeaderText = relicHeader.AddComponent<TextMeshProUGUI>();
        relicHeaderText.alignment = TextAlignmentOptions.Center;
        relicHeaderText.fontSize = 24;
        relicHeaderText.color = Color.black;
        relicHeaderText.text = "Choose a Relic:";

        RectTransform relicHeaderRect = relicHeaderText.GetComponent<RectTransform>();
        relicHeaderRect.anchorMin = new Vector2(0.15f, 0.35f);
        relicHeaderRect.anchorMax = new Vector2(0.85f, 0.43f);
        relicHeaderRect.offsetMin = Vector2.zero;
        relicHeaderRect.offsetMax = Vector2.zero;

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
                if (GameManager.Instance.wave % 3 == 0)
                    GenerateRelicReward();
                else
                    HideRelicUI();
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

    void GenerateRelicReward()
    {
        // clear old relic buttons
        foreach (GameObject btn in relicButtons)
            Destroy(btn);
        relicButtons.Clear();
        relicChoices.Clear();

        List<Relic> available = new List<Relic>();
        foreach (Relic r in RelicLibrary.Instance.allRelics)
        {
            if (!ownedRelicNames.Contains(r.name))
                available.Add(r);
        }

        int count = Mathf.Min(3, available.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            relicChoices.Add(available[idx]);
            available.RemoveAt(idx);
        }

        relicHeaderText.gameObject.SetActive(true);
        for (int i = 0; i < relicChoices.Count; i++)
        {
            Relic relic = relicChoices[i];
            float xMin = 0.1f + i * 0.3f;
            float xMax = xMin + 0.25f;

            GameObject btn = CreateButton(relic.name + "\n" + relic.description,
                new Vector2(xMin, 0.10f), new Vector2(xMax, 0.33f));

            int capturedIndex = i;
            btn.GetComponent<Button>().onClick.AddListener(() => PickRelic(capturedIndex));
            relicButtons.Add(btn);
        }
    }
    void HideRelicUI()
    {
        foreach (GameObject btn in relicButtons)
            Destroy(btn);
        relicButtons.Clear();
        relicChoices.Clear();
        if (relicHeaderText != null)
            relicHeaderText.gameObject.SetActive(false);
    }

    void PickRelic(int index)
    {
        if (index >= relicChoices.Count) return;

        Relic chosen = relicChoices[index];
        chosen.Activate();
        ownedRelicNames.Add(chosen.name);

        foreach (GameObject btn in relicButtons)
            Destroy(btn);
        relicButtons.Clear();
        relicChoices.Clear();

        relicHeaderText.text = "Relic acquired: " + chosen.name;
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