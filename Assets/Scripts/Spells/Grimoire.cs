using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;

public class Parser
{
    public Spell spell {  get; set; }
    public SpellModifier modifier { get; set; }
}

public class Grimoire
{
    public List<JToken> spells = new();
    public List<JToken> modifiers = new();

    public enum Chapter
    {
        SPELL,
        MODIFIER
    }

    private static Grimoire theInstance;

    public static Grimoire Instance { get
        {
            if (theInstance == null)
                theInstance = new Grimoire();
            return theInstance;
        }
    }

    public JToken GetPage(Chapter chapter, string name)
    {
        JToken page = null;
        List<JToken> section;
        switch (chapter)
        {
            case Chapter.SPELL:
                section = spells;
                break;
            case Chapter.MODIFIER:
                section = modifiers;
                break;
            default:
                throw new InvalidOperationException("Invalid chapter: use SPELL or MODIFIER - ex. GetPage(Grimoire.Instance.SPELL, \"Arcane Bolt\");");
        }
        section.ForEach(p => { if (p["name"].ToString() == name) page = p; });
        return page;
    }

    private Grimoire()
    {
        IEnumerable<JToken> spellTokens = JToken.Parse(File.ReadAllText("./Assets/Resources/Spells.json"))
            .Children().Children()
            .ToList()
            .Where(s => s["mana_cost"] != null);
        IEnumerable<JToken> modTokens = JToken.Parse(File.ReadAllText("./Assets/Resources/Spells.json"))
            .Children().Children()
            .ToList()
            .Where(s => s["mana_cost"] == null);
        //add corresponding class to their respective lists (spells, modifiers)
        foreach (JToken token in spellTokens)
            spells.Add(token);
        foreach (JToken token in modTokens)
            modifiers.Add(token);
    }
}