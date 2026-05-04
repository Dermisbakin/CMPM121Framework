using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{

    public Spell Build(SpellCaster owner, string spellName = "arcane_bolt")
    {
        Spell spell = new Spell(owner);
        spell.SetAttributes(spellName);
        return spell;
    }

   
    public SpellBuilder()
    {        
    }

}
