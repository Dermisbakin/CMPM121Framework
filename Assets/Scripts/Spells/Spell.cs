using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    public SpellStats stats;

    public string name;
    public string description;
    public int icon;
    public string damageAmount;
    public string damageType;
    public string manaCost;
    public string cooldown;
    public string projectileTrajectory;
    public string projectileSpeed;
    public string projectileLifetime;
    public int projectileSprite;

    public Spell(SpellCaster owner)
    {
        this.owner = owner;
        this.stats = new SpellStats();
    }

    public void SetAttributes(JToken page)
    {
        if (page == null) return;

        name = page["name"]?.ToString() ?? "Unknown";
        description = page["description"]?.ToString() ?? "";
        icon = page["icon"]?.ToObject<int>() ?? 0;

        JToken dmg = page["damage"];
        if (dmg != null)
        {
            damageAmount = dmg["amount"]?.ToString() ?? "10";
            damageType = dmg["type"]?.ToString() ?? "arcane";
        }
        else
        {
            damageAmount = "10";
            damageType = "arcane";
        }

        manaCost = page["mana_cost"]?.ToString() ?? "10";
        cooldown = page["cooldown"]?.ToString() ?? "2";

        JToken proj = page["projectile"];
        if (proj != null)
        {
            projectileTrajectory = proj["trajectory"]?.ToString() ?? "straight";
            projectileSpeed = proj["speed"]?.ToString() ?? "8";
            projectileLifetime = proj["lifetime"]?.ToString();
            projectileSprite = proj["sprite"]?.ToObject<int>() ?? 0;
        }
        else
        {
            projectileTrajectory = "straight";
            projectileSpeed = "8";
        }
    }

    public string GetName()
    {
        string result = name;
        foreach (string mod in stats.modifierNames)
        {
            result = mod + " " + result;
        }
        return result;
    }

    public string GetDescription()
    {
        return description;
    }

    public int GetIcon()
    {
        return icon;
    }

    public int GetManaCost()
    {
        float baseCost = EvalFloat(manaCost ?? "10");
        return (int)ValueModifier.Apply(baseCost, stats.manaCostMods);
    }

    public int GetDamage()
    {
        float baseDmg = EvalFloat(damageAmount ?? "10");
        return (int)ValueModifier.Apply(baseDmg, stats.damageMods);
    }

    public float GetCooldown()
    {
        float baseCd = EvalFloat(cooldown ?? "2");
        return ValueModifier.Apply(baseCd, stats.cooldownMods);
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

   // updated this method  to handle behavior flags and then fires projectles
    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        last_cast = Time.time;

        Vector3 direction = target - where;

        if (stats.isSplitter)
        {
            Vector3 dir1 = Quaternion.Euler(0, 0, stats.splitAngle) * direction;
            Vector3 dir2 = Quaternion.Euler(0, 0, -stats.splitAngle) * direction;
            FireProjectile(where, dir1);
            FireProjectile(where, dir2);
        }
        else if (stats.isDoubler)
        {
            FireProjectile(where, direction);
            yield return new WaitForSeconds(stats.doubleDelay);
            FireProjectile(where, direction);
        }
        else
        {
            FireProjectile(where, direction);
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

    protected void FireProjectile(Vector3 where, Vector3 direction)
    {
        float speed = EvalFloat(projectileSpeed ?? "8");
        speed = ValueModifier.Apply(speed, stats.speedMods);

        string trajectory = stats.trajectoryOverride ?? projectileTrajectory ?? "straight";

        int dmg = GetDamage();
        Damage.Type dtype = Damage.TypeFromString(damageType ?? "arcane");

        if (projectileLifetime != null)
        {
            float lt = EvalFloat(projectileLifetime);
            lt = ValueModifier.Apply(lt, stats.lifetimeMods);
            GameManager.Instance.projectileManager.CreateProjectile(
                projectileSprite, trajectory, where, direction, speed,
                (other, pos) => { if (other.team != team) other.Damage(new Damage(dmg, dtype)); },
                lt
            );
        }
        else
        {
            GameManager.Instance.projectileManager.CreateProjectile(
                projectileSprite, trajectory, where, direction, speed,
                (other, pos) => { if (other.team != team) other.Damage(new Damage(dmg, dtype)); }
            );
        }
    }

    protected float EvalFloat(string expr)
    {
        Dictionary<string, int> d = GameManager.Instance.dict;
        if (owner != null)
        {
            if (!d.ContainsKey("power")) d.Add("power", owner.power);
            else d["power"] = owner.power;
        }
        return RPNEvaluator.RPNEvaluator.Evaluatef(expr, d);
    }
}