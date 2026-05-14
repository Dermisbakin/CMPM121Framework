using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPNEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public class SpellProjectile
{
    public string trajectory { get; set; }
    public float speed { get; set; }
    public float lifetime { get; set; }
    public int sprite { get; set; }

    public SpellProjectile(string speed, string lifetime = null)
    {
        this.speed = RPNEvaluator.RPNEvaluator.Evaluatef(speed, GameManager.Instance.dictf);
        if (lifetime != null) this.lifetime = RPNEvaluator.RPNEvaluator.Evaluatef(lifetime, GameManager.Instance.dictf);
    }
}

[JsonObject(MemberSerialization = MemberSerialization.Fields)]
public class Spell
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;
    public SpellStats stats;

    private string name;
    private string description;
    private int icon;
    protected string N;
    protected string spray;
    protected Damage damage;
    protected Damage secondary_damage;
    protected string mana_cost;
    protected string cooldown;
    protected SpellProjectile projectile;
    protected SpellProjectile secondary_projectile;

    protected static JToken spellPage;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
        this.stats = new SpellStats();
        if (owner != null)
        {
            GameManager.Instance.dict["power"] = owner.power;
            GameManager.Instance.dictf["power"] = owner.power;
        }
    }

    public virtual void SetAttributes(string name)
    {
        spellPage ??= Grimoire.Instance.GetPage(Grimoire.Chapter.SPELL, name);
        if (spellPage != null)
        {
            this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .ToList()
                .ForEach(p => { if (spellPage[p.Name] != null) p.SetValue(this, spellPage[p.Name].ToObject(p.FieldType)); });
        }
        else throw new ArgumentException($"Could not find a spell of name \"{name}\".");
    }

    public string GetName()
    {
        string result = this.name;
        foreach (string mod in stats.modifierNames)
        {
            result = mod + " " + result;
        }
        return result;
    }

    public int GetManaCost()
    {
        float baseCost = RPNEvaluator.RPNEvaluator.Evaluatef(this.mana_cost, GameManager.Instance.dict);
        return (int)ValueModifier.Apply(baseCost, stats.manaCostMods);
        //return (int)baseCost;
    }

    public int GetDamage()
    {
        float baseDmg = this.damage.amount;
        return (int)ValueModifier.Apply(baseDmg, stats.damageMods);
        //return (int)baseDmg;
    }

    public int GetSecondaryDamage()
    {
        float baseDmg = this.secondary_damage.amount;
        return (int)ValueModifier.Apply(baseDmg, stats.damageMods);
        //return (int)baseDmg;
    }

    public SpellProjectile GetProjectile()
    {
        return this.projectile;
    }

    public float GetCooldown()
    {
        float baseCd = RPNEvaluator.RPNEvaluator.Evaluatef(this.cooldown, GameManager.Instance.dict);
        return ValueModifier.Apply(baseCd, stats.cooldownMods);
        //return baseCd;
    }

    public virtual int GetIcon()
    {
        return this.icon;
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public void SetDamage(int damage, int secondaryDamage = 0)
    {
        this.damage.amount = damage;
        this.secondary_damage.amount = secondaryDamage;
    }

    public void SetSpeed(float speed, float secondarySpeed = 0)
    {
        this.projectile.speed = speed;
        this.secondary_projectile.speed = secondarySpeed;
    }

    public void SetTrajectory(string trajectory, string secondTrajectory = "")
    {
        this.projectile.trajectory = trajectory;
        this.secondary_projectile.trajectory = secondTrajectory;
    }

    public void SetLifetime(float lifetime, float secondLifetime = 0f)
    {
        this.projectile.lifetime = lifetime;
        this.secondary_projectile.lifetime = secondLifetime;
    }

    public void SetMana(int manaCost)
    {
        this.mana_cost = manaCost.ToString();
    }

    public void SetCooldown(float cooldown)
    {
        this.cooldown = cooldown.ToString();
    }

    public void SetSpray(int spray)
    {
        this.spray = spray.ToString();
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        last_cast = Time.time;

        Vector3 direction = target - where;

        float finalSpeed = ValueModifier.Apply(projectile.speed, stats.speedMods);

        string traj = stats.trajectoryOverride ?? projectile.trajectory ?? "straight";

        if (stats.isSplitter)
        {
            Vector3 dir1 = Quaternion.Euler(0, 0, stats.splitAngle) * direction;
            Vector3 dir2 = Quaternion.Euler(0, 0, -stats.splitAngle) * direction;
            FireProjectile(where, dir1, traj, finalSpeed);
            FireProjectile(where, dir2, traj, finalSpeed);
        }
        else if (stats.isDoubler)
        {
            FireProjectile(where, direction, traj, finalSpeed);
            yield return new WaitForSeconds(stats.doubleDelay);
            FireProjectile(where, direction, traj, finalSpeed);
        }
        else
        {
            FireProjectile(where, direction, traj, finalSpeed);
        }

        if (stats.isVampiric)
        {
            PlayerController pc = GameManager.Instance.player.GetComponent<PlayerController>();
            if (pc != null && pc.hp != null)
            {
                int healAmount = (int)(GetDamage() * 0.2f);
                pc.hp.hp = Mathf.Min(pc.hp.hp + healAmount, pc.hp.max_hp);
            }
        }

        yield return new WaitForEndOfFrame();
    }

    protected void FireProjectile(Vector3 where, Vector3 direction, string trajectory, float speed)
    {
        if (projectile.lifetime > 0)
        {
            float lt = ValueModifier.Apply(projectile.lifetime, stats.lifetimeMods);
            GameManager.Instance.projectileManager.CreateProjectile(
                projectile.sprite, trajectory, where, direction, speed, OnHit, lt);
        }
        else
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                projectile.sprite, trajectory, where, direction, speed, OnHit);
        }
    }

    protected void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), damage.type));
        }
    }

}

public class SpellModifier : Spell
{
    private Spell decoratee;

    public string delay;
    public string angle;
    public string damage_multiplier;
    public string speed_multiplier;
    public string lifetime_multiplier;
    public string mana_multiplier;
    public string mana_adder;
    public string cooldown_multiplier;
    public string projectile_trajectory;

    public int IsDoubled;
    public int IsSplit;
    public SpellModifier(SpellCaster owner) : base(owner)
    {
        decoratee = new Spell(owner);
        this.owner = owner;
    }

    public override void SetAttributes(string name)
    {
        JToken modPage = Grimoire.Instance.GetPage(Grimoire.Chapter.MODIFIER, name);
        if (modPage != null)
        {
            this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .ToList()
                .ForEach(p => { if (modPage[p.Name] != null) p.SetValue(this, modPage[p.Name].ToObject(p.FieldType)); });
        }
        else
        {
            try { decoratee.SetAttributes(name); }
            catch { throw new ArgumentException($"Could not find a modifier of name \"{name}\"."); }
            //copy its values into SpellModifier/ set ownership
            IEnumerable<FieldInfo> fields = decoratee.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.GetValue(decoratee) != null);
            foreach (FieldInfo field in fields)
            {
                this.GetType().BaseType.GetField(field.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                   .SetValue(this, field.GetValue(decoratee));
            }
        }
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, projectile_trajectory, where, target - where, projectile.speed, OnHit);
        yield return new WaitForEndOfFrame();
    }
}