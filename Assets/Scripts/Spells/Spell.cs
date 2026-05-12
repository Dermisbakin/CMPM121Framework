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
    public string trajectory {  get; set; }
    public float speed { get; set; }
    public float lifetime { get; set; }
    public int sprite {  get; set; }

    public SpellProjectile(string speed, string lifetime = null)
    {
        this.speed = RPNEvaluator.RPNEvaluator.Evaluatef(speed, GameManager.Instance.dictf);
        if(lifetime != null) this.lifetime = RPNEvaluator.RPNEvaluator.Evaluatef(lifetime, GameManager.Instance.dictf);
    }
}

[JsonObject(MemberSerialization = MemberSerialization.Fields)]
public class Spell 
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

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
        return this.name;
    }

    public int GetManaCost()
    {
        return RPNEvaluator.RPNEvaluator.Evaluate(this.manaCost ?? "5", GameManager.Instance.dict);
    }

    public int GetDamage()
    {
        return this.damage.amount;
    }

    public float GetCooldown()
    {
        return RPNEvaluator.RPNEvaluator.Evaluatef(this.cooldown, GameManager.Instance.dictf);
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
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, projectile.speed, OnHit);
        yield return new WaitForEndOfFrame();
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
