using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SpellCaster
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public int power;
    public Hittable.Team team;

    // let player have up to 4 spells
    public List<Spell> spells;
    public int activeSpellIndex;

    public Spell spell; //{ get { return spells.Count > 0 ? spells[activeSpellIndex] : null; } }

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.power = 10;
        this.team = team;
        this.spells = new List<Spell>();
        this.activeSpellIndex = 0;

        spell = new SpellBuilder().Seed(this).Build();
        spells.Add(spell);
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        Spell s = spell;
        if (s != null && mana >= s.GetManaCost() && s.IsReady())
        {
            mana -= s.GetManaCost();
            yield return s.Cast(where, target, team);
        }
        yield break;
    }

    public bool EquipSpell(Spell newSpell)
    {
        if (spells.Count < 4)
        {
            spells.Add(newSpell);
            return true;
        }
        return false;
    }

    public void DropSpell(int index)
    {
        if (index >= 0 && index < spells.Count)
        {
            spells.RemoveAt(index);
            if (activeSpellIndex >= spells.Count)
                activeSpellIndex = spells.Count - 1;
            if (activeSpellIndex < 0)
                activeSpellIndex = 0;
        }
    }

    public void NextSpell()
    {
        if (spells.Count > 0)
            activeSpellIndex = (activeSpellIndex + 1) % spells.Count;
    }

    public void PrevSpell()
    {
        if (spells.Count > 0)
        {
            activeSpellIndex--;
            if (activeSpellIndex < 0)
                activeSpellIndex = spells.Count - 1;
        }
    }
}