using UnityEngine;

public class RelicEffect
{
    public virtual void Apply() { }
    public virtual void Remove() { }
}

public class GainManaEffect : RelicEffect
{
    int amount;

    public GainManaEffect(int amount)
    {
        this.amount = amount;
    }

    public override void Apply()
    {
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null || pc.spellcaster == null) return;
        pc.spellcaster.mana = Mathf.Min(pc.spellcaster.mana + amount, pc.spellcaster.max_mana);
    }
}


public class GainSpellPowerEffect : RelicEffect
{
    string amountExpr;
    int lastAmount;
    bool isTemporary;

    public GainSpellPowerEffect(string amountExpr, bool isTemporary = false)
    {
        this.amountExpr = amountExpr;
        this.isTemporary = isTemporary;
        this.lastAmount = 0;
    }

    public override void Apply()
    {
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null || pc.spellcaster == null) return;

        // if still active
        if (lastAmount > 0)
            pc.spellcaster.power -= lastAmount;

        lastAmount = RPNEvaluator.RPNEvaluator.Evaluate(amountExpr, GameManager.Instance.dict);
        pc.spellcaster.power += lastAmount;
    }

    public override void Remove()
    {
        if (!isTemporary) return;
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null || pc.spellcaster == null) return;
        pc.spellcaster.power -= lastAmount;
        lastAmount = 0;
    }
}

public class GainHPEffect : RelicEffect
{
    int amount;

    public GainHPEffect(int amount)
    {
        this.amount = amount;
    }

    public override void Apply()
    {
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null || pc.hp == null) return;
        pc.hp.hp = Mathf.Min(pc.hp.hp + amount, pc.hp.max_hp);
    }
}

public class GainMaxHPEffect : RelicEffect
{
    int amount;

    public GainMaxHPEffect(int amount)
    {
        this.amount = amount;
    }

    public override void Apply()
    {
        PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
        if (pc == null || pc.hp == null) return;
        pc.hp.SetMaxHP(pc.hp.max_hp + amount);
    }
}