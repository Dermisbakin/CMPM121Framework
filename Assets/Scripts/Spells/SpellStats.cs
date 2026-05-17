using System.Collections.Generic;

// holds all the modifier lists and behavior flags for a spell

public class SpellStats
{
    // value modifier lists
    public List<ValueModifier> damageMods = new List<ValueModifier>();
    public List<ValueModifier> manaCostMods = new List<ValueModifier>();
    public List<ValueModifier> cooldownMods = new List<ValueModifier>();
    public List<ValueModifier> speedMods = new List<ValueModifier>();
    public List<ValueModifier> lifetimeMods = new List<ValueModifier>();

    // behavior flags
    public int isSplitter = 0;
    public float splitAngle = 10f;

    public int isDoubler = 0;
    public float doubleDelay = 0.5f;
     

    // trajectory override
    public string trajectoryOverride = null;
    //public string secondaryTrajectoryOverride = null;


    public bool isVampiric = false;
    public bool isPiercing = false;

    // names of all modifiers applied
    public List<string> modifierNames = new List<string>();

}