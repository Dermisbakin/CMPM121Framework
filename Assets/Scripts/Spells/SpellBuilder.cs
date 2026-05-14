using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
//using UnityEditor.Experimental.GraphView;


public class SpellBuilder
{
    SpellModifier spell = null;
    public SpellModifier Build()
    {
        if (spell.owner != null) return spell;
        else
        {
            throw new System.InvalidOperationException("cannot return a spell with no owner");
        }
    }
    //todo: implement the sum of each mod: delay, angle, damage (add+m), speed(add+m),
    //lifetime(add+m), mana(add+m), cooldown(add+m), and proj. trajectory

    //helper
    private int sort(string str)
    {
        //return 1 if adder. return 2 if multiplier, etc. incrementing per field. return -1 if neither
        int result = -1;
        if (str.Contains("damage")) {
            if (str.Contains("adder")) { result = 1; }
            else { result = 2; }
        }
        if (str.Contains("speed")) {
            if (str.Contains("adder")) { result = 3; }
            else { result = 4; }
        }
        if (str.Contains("lifetime")) {
            if (str.Contains("adder")) { result = 5; }
            else { result = 6; }
        }
        if (str.Contains("mana")) {
            if (str.Contains("adder")) { result = 7; }
            else { result = 8; }
        }
        if (str.Contains("cooldown")) {
            if (str.Contains("adder")) { result = 9; }
            else { result = 10; }
        }
        return result;
    }
    public SpellBuilder Seed(SpellCaster owner, string spellName = "Arcane Bolt")
    {
        spell = new SpellModifier(owner);
        spell.SetAttributes(spellName);
        return this;
    }
    public SpellModifier AutoBuild(List<SpellModifier> modList)
    {
        int dmg = 0, mana = 0;
        float speed = spell.GetProjectile().speed, lifetime = spell.GetProjectile().lifetime, cooldown = spell.GetCooldown(), dmgf = 1f, speedf = 1f, lifetimef = 1f, manaf = 1f, cooldownf = 1f;
        string delay = null, angle = null, trajectory = null;

        //combine each applicable modifier from the list
        foreach (SpellModifier mod in modList)
        {
            IEnumerable<FieldInfo> fields = mod.GetType().GetFields().Where(x => x.GetValue(mod) != null);
            foreach (FieldInfo field in fields)
            {
                if (field.Name == "doubler") spell.IsDoubled++;
                if (field.Name == "splitter") spell.IsSplit++;
                //modify spell name
                if (field.Name == "name") spell.SetName(field.Name + spell.GetName());
                if (field.Name != "name" && field.Name != "description")
                {
                    switch (sort(field.Name))
                    {
                        case 1:
                            dmg += (int)field.GetValue(mod);
                            break;
                        case 2:
                            dmgf *= (float)field.GetValue(mod);
                            break;
                        case 3:
                            speed += (float)field.GetValue(mod);
                            break;
                        case 4:
                            speedf *= (float)field.GetValue(mod);
                            break;
                        case 5:
                            lifetime += (float)field.GetValue(mod);
                            break;
                        case 6:
                            lifetimef *= (float)field.GetValue(mod);
                            break;
                        case 7:
                            mana += (int)field.GetValue(mod);
                            break;
                        case 8:
                            manaf *= (float)field.GetValue(mod);
                            break;
                        case 9:
                            cooldown = (float)field.GetValue(mod);
                            break;
                        case 10:
                            cooldownf *= (float)field.GetValue(mod);
                            break;
                        default:
                            if (field.Name == "delay") delay = field.GetValue(mod).ToString();
                            else if (field.Name == "angle") angle = field.GetValue(mod).ToString();
                            else if (field.Name == "projectile_trajectory") trajectory = field.GetValue(mod).ToString();
                            break;
                    }
                }
            }
        }

        //value initialization
        WithDelay(delay);
        WithAngle(angle);
        DmgMod(dmg, dmgf);
        SpeedMod((int)speed, speedf);
        LifetimeMod(lifetime, lifetimef);
        ManaMod(mana, manaf);
        CDMod(cooldown*cooldownf);
        TrajectoryMod(trajectory);

        return spell;
    }

    //builder components, parameters (adderVal,multiplierVal)
    public SpellBuilder WithDelay(string delay)
    {
        if (spell.owner != null) spell.delay = delay;
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }

    public SpellBuilder WithAngle(string angle)
    {
        int newAngle = (int)Evalf(angle);
        if (spell.owner != null) spell.SetSpray(newAngle);
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }
    public SpellBuilder DmgMod(int add, float multi = 1f)
    {
        int total = (int)((spell.GetDamage() + add) * multi);
        if (spell.owner != null) spell.SetDamage(total);
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }

    public SpellBuilder SpeedMod(float speed1, float speed2 = 0f)
    {
        if (spell.owner != null) spell.SetSpeed(speed1, speed2);
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }

    public SpellBuilder LifetimeMod(float lifetime1, float lifetime2 = 0f)
    {
        if (spell.owner != null) spell.SetLifetime(lifetime1, lifetime2);
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }

    public SpellBuilder ManaMod(int manaAdder, float manaMulti = 1f)
    {
        if (spell.owner != null) spell.SetMana((int)((spell.GetManaCost() + manaAdder) * manaMulti));
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }

    public SpellBuilder CDMod(float cooldown)
    {
        if (spell.owner != null) spell.SetCooldown(cooldown);
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
    }

    public SpellBuilder TrajectoryMod(string traj1, string traj2 = "")
    {
        if (spell.owner != null) spell.SetTrajectory(traj1, traj2);
        else throw new System.InvalidOperationException("No spell owner: start with \".Build()\" first.");
        return this;
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
        if (Grimoire.Instance.spells.Count == 0) return Build();

        int idx = Random.Range(0, baseSpells.Count);
        JToken basePage = baseSpells[idx];

        Spell spell = new Spell(owner);
        spell.SetAttributes(basePage["name"].ToString());

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