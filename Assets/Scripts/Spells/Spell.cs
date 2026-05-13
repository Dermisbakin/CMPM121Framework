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
    [JsonProperty("secondary_damage")]
    protected Damage secondaryDamage;
    [JsonProperty("mana_cost")]
    protected string manaCost;
    protected string cooldown;
    protected SpellProjectile projectile;
    [JsonProperty("secondary_projectile")]
    protected SpellProjectile secondaryProjectile;

    //keep json for future referral
    protected static JToken spellPage;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
        this.stats = new SpellStats();
        //add owner power to both dictionaries
        GameManager.Instance.dict["power"] = owner.power;
        GameManager.Instance.dictf["power"] = owner.power;
    }

    public virtual void SetAttributes(string name) //can(?) be used to update values per wave
    {
        //get spell of same name
        spellPage ??= Grimoire.Instance.GetPage(Grimoire.Chapter.SPELL, name);
        //dynamically get each field and set their values
        this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .ToList()
            .ForEach(p => { if (spellPage[p.Name] != null) p.SetValue(this, spellPage[p.Name].ToObject(p.FieldType)); });
    }

    //getters
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
        float baseCost = RPNEvaluator.RPNEvaluator.Evaluatef(this.manaCost, GameManager.Instance.dict);
        return (int)ValueModifier.Apply(baseCost, stats.manaCostMods);
    }

    public int GetDamage()
    {
        float baseDmg = this.damage.amount;
        return (int)ValueModifier.Apply(baseDmg, stats.damageMods);
    }

    public float GetCooldown()
    {
        float baseCd = RPNEvaluator.RPNEvaluator.Evaluatef(this.cooldown, GameManager.Instance.dict);
        return ValueModifier.Apply(baseCd, stats.cooldownMods);
    }

    public virtual int GetIcon()
    {
        return this.icon;
    }

    //setters

    public void SetDamage(int damage, int secondaryDamage = 0)
    {
        this.damage.amount = damage;
        this.secondaryDamage.amount = secondaryDamage;
    }

    public void SetSpeed(int speed, int secondarySpeed = 0)
    {
        this.projectile.speed = speed;
        this.secondaryProjectile.speed = secondarySpeed;
    }

    public void SetTrajectory(string trajectory, string secondTrajectory = "")
    {
        this.projectile.trajectory = trajectory;
        this.secondaryProjectile.trajectory = secondTrajectory;
    }

    public void SetLifetime(float lifetime, float secondLifetime = 0f)
    {
        this.projectile.lifetime = lifetime;
        this.secondaryProjectile.lifetime = secondLifetime;
    }

    public void SetMana(string manaCost)
    {
        this.manaCost = manaCost;
    }

    public void SetCooldown(string cooldown)
    {
        this.cooldown = cooldown;
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
    public string delay;
    public string angle;
    public string damage_multiplier;
    public string speed_multiplier;
    public string lifetime_multiplier;
    public string mana_multiplier;
    public string mana_adder;
    public string cooldown_multiplier;
    public string projectile_trajectory;

    public SpellModifier(SpellCaster owner) : base(owner)
    {
        this.owner = owner;
    }

    public override void SetAttributes(string name)
    {
        //get modifier of same name
        JToken modPage = Grimoire.Instance.GetPage(Grimoire.Chapter.MODIFIER, name);
        this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .ToList()
            .ForEach(p => { if (modPage[p.Name] != null) p.SetValue(this, modPage[p.Name].ToObject(p.FieldType)); });
    }

    public override IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, projectile_trajectory, where, target - where, projectile.speed, OnHit);
        yield return new WaitForEndOfFrame();
    }
}