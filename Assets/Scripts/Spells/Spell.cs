using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RPNEvaluator;
using System.Linq;
using Unity.VisualScripting;
using System;
using System.Reflection;

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

    protected string name;
    protected string description;
    private int icon;
    private string N;
    private Damage damage;
    [JsonProperty("secondary_damage")]
    private Damage secondaryDamage;
    [JsonProperty("mana_cost")]
    private string manaCost;
    private string cooldown;
    private SpellProjectile projectile;
    [JsonProperty("secondary_projectile")]
    private SpellProjectile secondaryProjectile;

    //keep json for future referral
    private static JToken spellPage;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
        //add owner power to both dictionaries
        if(!GameManager.Instance.dict.TryAdd("power", owner.power)) GameManager.Instance.dict["power"] = owner.power;
        if(!GameManager.Instance.dictf.TryAdd("power", owner.power)) GameManager.Instance.dictf["power"] = owner.power;
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

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);
        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
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
    public string damage_multiplier;
    public string speed_multiplier;
    public string lifetime_multiplier;
    public string mana_multiplier;
    public string mana_adder;
    public string cooldown_multiplier;
    public string projectile_trajectory;

    public SpellModifier(SpellCaster owner, string name) : base(owner)
    {
        this.owner = owner;
        decoratee = new Spell(owner);
    }

    public override void SetAttributes(string name)
    {
        //get modifier of same name
        JToken modPage = Grimoire.Instance.GetPage(Grimoire.Chapter.MODIFIER, name);
        this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .ToList()
            .ForEach(p => { if (modPage[p.Name] != null) p.SetValue(this, modPage[p.Name].ToObject(p.FieldType)); });
    }
}
