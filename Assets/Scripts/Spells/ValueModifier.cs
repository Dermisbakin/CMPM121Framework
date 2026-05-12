using System.Collections.Generic;

public class ValueModifier
{
    public enum ModType { ADD, MULTIPLY }

    public ModType type;
    public float value;

    public ValueModifier(ModType type, float value)
    {
        this.type = type;
        this.value = value;
    }

    // apply all modifiers to a base value

    // used formula -> (base + all adds) * all multipliers
    public static float Apply(float baseValue, List<ValueModifier> mods)
    {
        if (mods == null || mods.Count == 0) return baseValue;

        float addTotal = 0f;
        float multTotal = 1f;

        foreach (ValueModifier m in mods)
        {
            if (m.type == ModType.ADD)
                addTotal += m.value;
            else if (m.type == ModType.MULTIPLY)
                multTotal *= m.value;
        }

        return (baseValue + addTotal) * multTotal;
    }
}