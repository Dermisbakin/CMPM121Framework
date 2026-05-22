using UnityEngine;


public class Relic
{
    public string name;
    public int sprite;
    public string description;

    RelicTrigger trigger;
    RelicEffect effect;

    RelicTrigger untilTrigger;

    public Relic(string name, int sprite, string description, RelicTrigger trigger, RelicEffect effect, RelicTrigger untilTrigger = null)
    {
        this.name = name;
        this.sprite = sprite;
        this.description = description;
        this.trigger = trigger;
        this.effect = effect;
        this.untilTrigger = untilTrigger;
    }

    public void Activate()
    {
        trigger.OnTriggered += effect.Apply;
        trigger.Register();

        if (untilTrigger != null)
        {
            untilTrigger.OnTriggered += effect.Remove;
            untilTrigger.Register();
        }
        if (trigger is StandStillTrigger standStill)
        {
            standStill.OnStoppedMoving += effect.Remove;
        }
    }

    public void Deactivate()
    {
        trigger.OnTriggered -= effect.Apply;
        trigger.Unregister();

        if (untilTrigger != null)
        {
            untilTrigger.OnTriggered -= effect.Remove;
            untilTrigger.Unregister();
        }
    }

    public string GetLabel()
    {
        return name;
    }
}