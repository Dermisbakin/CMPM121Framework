using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;


public class SpellBuilder
{
    public Spell Build(SpellCaster owner, string spellName = "Arcane Bolt")
    {
        Spell spell = new Spell(owner);
        // spell.SetAttributes(spellName);
        JToken page = Grimoire.Instance.GetPage(Grimoire.Chapter.SPELL, spellName);
        if (page != null)
        {
            spell.SetAttributes(page);
        }
        return spell;
    }

    public void ApplyModifier(Spell spell, JToken modPage)
    {
        if (modPage == null) return;

        string modName = modPage["name"]?.ToString() ?? "";
        spell.stats.modifierNames.Add(modName);

        Dictionary<string, int> d = GameManager.Instance.dict;
        if (spell.owner != null)
        {
            if (!d.ContainsKey("power")) d.Add("power", spell.owner.power);
            else d["power"] = spell.owner.power;
        }

        // modifiers
        if (modPage["damage_multiplier"] != null)
        {
            float val = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["damage_multiplier"].ToString(), d);
            spell.stats.damageMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, val));
        }
        if (modPage["speed_multiplier"] != null)
        {
            float val = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["speed_multiplier"].ToString(), d);
            spell.stats.speedMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, val));
        }
        if (modPage["mana_multiplier"] != null)
        {
            float val = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["mana_multiplier"].ToString(), d);
            spell.stats.manaCostMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, val));
        }
        if (modPage["mana_adder"] != null)
        {
            float val = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["mana_adder"].ToString(), d);
            spell.stats.manaCostMods.Add(new ValueModifier(ValueModifier.ModType.ADD, val));
        }
        if (modPage["cooldown_multiplier"] != null)
        {
            float val = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["cooldown_multiplier"].ToString(), d);
            spell.stats.cooldownMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, val));
        }

        // behavior modifiers
        if (modPage["projectile_trajectory"] != null)
        {
            spell.stats.trajectoryOverride = modPage["projectile_trajectory"].ToString();
        }
        if (modPage["angle"] != null)
        {
            spell.stats.isSplitter = true;
            spell.stats.splitAngle = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["angle"].ToString(), d);
        }
        if (modPage["delay"] != null)
        {
            spell.stats.isDoubler = true;
            spell.stats.doubleDelay = RPNEvaluator.RPNEvaluator.Evaluatef(modPage["delay"].ToString(), d);
        }
    }

    // custom modifiers
    // as required, 3
    public void ApplyCustomModifier(Spell spell, string modName)
    {
        spell.stats.modifierNames.Add(modName);

        if (modName == "vampiric")
        {
            spell.stats.manaCostMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, 1.3f));
            spell.stats.isVampiric = true;
        }
        else if (modName == "piercing")
        {
            spell.stats.damageMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, 0.85f));
            spell.stats.manaCostMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, 1.2f));
            spell.stats.isPiercing = true;
        }
        else if (modName == "rapid")
        {
            spell.stats.cooldownMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, 0.4f));
            spell.stats.damageMods.Add(new ValueModifier(ValueModifier.ModType.MULTIPLY, 0.6f));
        }
    }

    // generate a completely random spell
    public Spell BuildRandom(SpellCaster owner)
    {
        List<JToken> baseSpells = Grimoire.Instance.spells;
        if (baseSpells.Count == 0) return Build(owner);

        int idx = Random.Range(0, baseSpells.Count);
        JToken basePage = baseSpells[idx];

        Spell spell = new Spell(owner);
        spell.SetAttributes(basePage);

        List<JToken> jsonMods = Grimoire.Instance.modifiers;
        string[] customMods = { "vampiric", "piercing", "rapid" };

        int modCount = 0;
        while (Random.value < 0.6f && modCount < 3)     // 60% chance to add each one
        {
            if (Random.value < 0.7f && jsonMods.Count > 0)      // 30% custom, 70% form JSON
            {
                int mi = Random.Range(0, jsonMods.Count);
                ApplyModifier(spell, jsonMods[mi]);
            }
            else
            {
                int ci = Random.Range(0, customMods.Length);
                ApplyCustomModifier(spell, customMods[ci]);
            }
            modCount++;
        }

        return spell;
    }

    public SpellBuilder()
    {
    }
}