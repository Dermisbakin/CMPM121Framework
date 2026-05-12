using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;


public class SpellBuilder 
{
    private SpellModifier spell = new SpellModifier(null);
    //default formula shall be (base + adds) * multipliers
    public Spell Build()
    {
        return spell;
    }

    //always start builder with this
    public SpellBuilder Seed(SpellCaster owner, string spellName = "Arcane Bolt")
    {
        spell.owner = owner;
        spell.SetAttributes(spellName);
        return this;
    }

    public SpellBuilder WithDelay(string delay)
    {
        if (spell.owner != null) spell.delay = delay;
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
            return this;
    }

    public SpellBuilder DmgMod(string add, string multi = "1")
    {
        int total = (int)((spell.GetDamage() + Eval(add)) * Evalf(multi));
        if (spell.owner != null) spell.SetDamage(total);
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
        return this;
    }

    public SpellBuilder SpeedMod(int speed1, int speed2 = 0)
    {
        if (spell.owner != null) spell.SetSpeed(speed1, speed2);
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
        return this;
    }

    public SpellBuilder LifetimeMod(float lifetime1, float lifetime2 = 0f)
    {
        if (spell.owner != null) spell.SetLifetime(lifetime1, lifetime2);
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
        return this;
    }

    public SpellBuilder ManaMod(string manaCost)
    {
        if (spell.owner != null) spell.SetMana(manaCost);
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
        return this;
    }

    public SpellBuilder CDMod(string cooldown)
    {
        if (spell.owner != null) spell.SetCooldown(cooldown);
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
        return this;
    }

    public SpellBuilder TrajectoryMod(string traj1, string traj2 = "")
    {
        if (spell.owner != null) spell.SetTrajectory(traj1, traj2);
        else throw new InvalidOperationException("No spell owner: start with \".Seed()\" first.");
        return this;
    }

    public Spell GenerateRandomSpell(SpellCaster owner)
    {
        System.Random rng = new System.Random();
        int idx = rng.Next(Grimoire.Instance.spells.Count);
        JToken randSpell = Grimoire.Instance.spells[idx];
        return randSpell.ToObject<Spell>();
    }

    public SpellModifier GiveModifier(Spell spell)
    {
        System.Random rng = new System.Random();
        int idx = rng.Next(Grimoire.Instance.modifiers.Count);
        JToken randMod = Grimoire.Instance.modifiers[idx];
        
        return randMod.ToObject<SpellModifier>();
    }

    //todo: input modifier list and create/update a spell with the mods
    public SpellModifier Load(JToken array)
    {
        return spell;
    }

    //helper shortcuts
    public int Eval(string str)
    {
        return (int)RPNEvaluator.RPNEvaluator.Evaluatef(str, GameManager.Instance.dictf);
    }

    public float Evalf(string str)
    {
        return RPNEvaluator.RPNEvaluator.Evaluatef(str, GameManager.Instance.dictf);
    }

    public SpellBuilder()
    {        
    }

}
