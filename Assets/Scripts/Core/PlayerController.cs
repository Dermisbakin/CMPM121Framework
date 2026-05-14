using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;

    public int speed;

    public Unit unit;
    private Coroutine manaRoutine;

    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        unit.movement = Vector2.zero;
        if (manaRoutine != null)
        {
            StopCoroutine(manaRoutine);
        }

        Dictionary<string, int> d = GameManager.Instance.dict;
        if (!d.ContainsKey("wave")) d.Add("wave", 1);
        else d["wave"] = 1;

        int maxHP = RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +", d);
        int maxMana = RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", d);
        int manaReg = RPNEvaluator.RPNEvaluator.Evaluate("10 wave +", d);
        int spellPower = RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *", d);
        speed = RPNEvaluator.RPNEvaluator.Evaluate("5", d);

        spellcaster = new SpellCaster(maxMana, manaReg, Hittable.Team.PLAYER);
        spellcaster.power = spellPower;
        manaRoutine = StartCoroutine(spellcaster.ManaRegeneration());

        hp = new Hittable(maxHP, Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
        spellui.SetSpell(spellcaster.spell);
    }

    public void ScaleStats(int wave)
    {
        Dictionary<string, int> d = GameManager.Instance.dict;
        d["wave"] = wave;

        int maxHP = RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +", d);
        int maxMana = RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", d);
        int manaReg = RPNEvaluator.RPNEvaluator.Evaluate("10 wave +", d);
        int spellPower = RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *", d);
        speed = RPNEvaluator.RPNEvaluator.Evaluate("5", d);

        hp.SetMaxHP(maxHP);

        spellcaster.max_mana = maxMana;
        spellcaster.mana = Mathf.Min(spellcaster.mana, maxMana);
        spellcaster.mana_reg = manaReg;
        spellcaster.power = spellPower;

        healthui.SetHealth(hp);
    }

    void Update()
    {

    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state != GameManager.GameState.INWAVE) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state != GameManager.GameState.COUNTDOWN &&
            GameManager.Instance.state != GameManager.GameState.INWAVE)
        {
            unit.movement = Vector2.zero;
            return;
        }
        unit.movement = value.Get<Vector2>() * speed;
    }

    void Die()
    {
        unit.movement = Vector2.zero;
        GameManager.Instance.resultMessage = "You were defeated on wave " + GameManager.Instance.wave + ".";
        GameManager.Instance.state = GameManager.GameState.GAMEOVER;
    }
}