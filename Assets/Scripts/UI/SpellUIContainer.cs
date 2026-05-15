using UnityEngine;

public class SpellUIContainer : MonoBehaviour
{
    public GameObject[] spellUIs;
    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i< spellUIs.Length; i++)
        {
            spellUIs[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || player.spellcaster == null) return;

        for (int i = 0; i < spellUIs.Length; i++)
        {
            bool hasSpell = i < player.spellcaster.spells.Count;
            spellUIs[i].SetActive(hasSpell);

            if (hasSpell)
            {
                SpellUI sui = spellUIs[i].GetComponent<SpellUI>();
                if (sui.spell != player.spellcaster.spells[i])
                    sui.SetSpell(player.spellcaster.spells[i], i);

                // highlight active spell
                sui.highlight.SetActive(i == player.spellcaster.activeSpellIndex);
            }
        }
    }

}
