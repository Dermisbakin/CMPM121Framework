using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpellUI : MonoBehaviour
{
    public GameObject icon;
    public RectTransform cooldown;
    public TextMeshProUGUI manacost;
    public TextMeshProUGUI damage;
    public GameObject highlight;
    public Spell spell;
    float last_text_update;
    const float UPDATE_DELAY = 1;
    public GameObject dropbutton;

    public int index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        last_text_update = 0;
    }

    public void SetSpell(Spell spell, int index)
    {
        this.spell = spell;
        this.index = index;
        GameManager.Instance.spellIconManager.PlaceSprite(spell.GetIcon(), icon.GetComponent<Image>());

        if (dropbutton != null)
        {
            UnityEngine.UI.Button btn = dropbutton.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
                pc.spellcaster.DropSpell(this.index);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spell == null) return;
        //if(GameManager.Instance.state == GameManager.GameState.WAVEEND) spell.SetAttributes(spell.GetName());
        if (Time.time > last_text_update + UPDATE_DELAY)
        {
            manacost.text = spell.GetManaCost().ToString();
            damage.text = spell.GetDamage().ToString();
            last_text_update = Time.time;
        }
        
        float since_last = Time.time - spell.last_cast;
        float perc;
        if (since_last > spell.GetCooldown())
        {
            perc = 0;
        }
        else
        {
            perc = 1-since_last / spell.GetCooldown();
        }
        cooldown.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 48 * perc);
    }
}
