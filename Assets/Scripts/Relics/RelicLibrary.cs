using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

public class RelicLibrary
{
    public List<Relic> allRelics = new List<Relic>();

    private static RelicLibrary theInstance;
    public static RelicLibrary Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new RelicLibrary();
            return theInstance;
        }
    }

    private RelicLibrary()
    {
        string data = File.ReadAllText("./Assets/Resources/relics.json");
        JArray relicsJson = JArray.Parse(data);

        foreach (JToken relicToken in relicsJson)
        {
            Relic r = BuildRelic(relicToken);
            if (r != null)
                allRelics.Add(r);
        }
    }

    Relic BuildRelic(JToken token)
    {
        string name = token["name"]?.ToString() ?? "Unknown";
        int sprite = token["sprite"]?.ToObject<int>() ?? 0;

        JToken triggerToken = token["trigger"];
        JToken effectToken = token["effect"];

        if (triggerToken == null || effectToken == null) return null;

        string triggerType = triggerToken["type"]?.ToString() ?? "";
        string effectType = effectToken["type"]?.ToString() ?? "";
        string effectAmount = effectToken["amount"]?.ToString() ?? "0";
        string triggerAmount = triggerToken["amount"]?.ToString();
        string until = effectToken["until"]?.ToString();

        RelicTrigger trigger = BuildTrigger(triggerType, triggerAmount);
        if (trigger == null) return null;

        // RelicTrigger untilTrigger = null;
        // if (until != null)
        //     untilTrigger = BuildTrigger(until, null);

        RelicTrigger untilTrigger = null;
        if (until != null && !(trigger is StandStillTrigger))
            untilTrigger = BuildTrigger(until, null);


        bool isTemporary = until != null;
        RelicEffect effect = BuildEffect(effectType, effectAmount, isTemporary);
        if (effect == null) return null;

        string description = triggerToken["description"]?.ToString() + ", " + effectToken["description"]?.ToString();

        return new Relic(name, sprite, description, trigger, effect, untilTrigger);
    }

    RelicTrigger BuildTrigger(string type, string amount)
    {
        switch (type)
        {
            case "take-damage":
                return new TakeDamageTrigger();
            case "stand-still":
                float seconds = 3f;
                if (amount != null)
                    seconds = RPNEvaluator.RPNEvaluator.Evaluatef(amount, GameManager.Instance.dictf);
                return new StandStillTrigger(seconds);
            case "on-kill":
                return new OnKillTrigger();
            case "cast-spell":
                return new OnCastTrigger();
            case "move":
                return new OnMoveTrigger();
            case "wave-start":
                return new OnWaveStartTrigger();
            default:
                return null;
        }
    }

    RelicEffect BuildEffect(string type, string amount, bool isTemporary)
    {
        switch (type)
        {
            case "gain-mana":
                return new GainManaEffect(RPNEvaluator.RPNEvaluator.Evaluate(amount, GameManager.Instance.dict));
            case "gain-spellpower":
                return new GainSpellPowerEffect(amount, isTemporary);
            case "gain-hp":
                return new GainHPEffect(RPNEvaluator.RPNEvaluator.Evaluate(amount, GameManager.Instance.dict));
            case "gain-max-hp":
                return new GainMaxHPEffect(RPNEvaluator.RPNEvaluator.Evaluate(amount, GameManager.Instance.dict));
            default:
                return null;
        }
    }
}