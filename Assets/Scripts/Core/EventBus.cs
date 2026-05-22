using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EventBus 
{
    private static EventBus theInstance;
    public static EventBus Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new EventBus();
            return theInstance;
        }
    }

    public event Action<Vector3, Damage, Hittable> OnDamage;
    public event Action<Vector3, Damage> OnPlayerDamaged;
    public event Action<GameObject> OnEnemyKilled;
    public event Action OnSpellCast;
    
    public void DoDamage(Vector3 where, Damage dmg, Hittable target)
    {
        OnDamage?.Invoke(where, dmg, target);
        // TODO
        // add OnPlayerDamaged event (Vector3 position, Damage dmg)
        // add OnEnemyKilled event (GameObject enemy)
        //add onSpellCast event()
        // more...
    }

    public void DoPlayerDamaged(Vector3 position, Damage dmg)
    {
        OnPlayerDamaged?.Invoke(position, dmg);
        Debug.Log("invoked player damage!");
    }

    public void DoEnemyKilled(GameObject enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }

    public void DoSpellCast()
    {
        OnSpellCast?.Invoke();
    }
    //  Required for Iron Heart custom relic
    public event Action<int> OnWaveStart;
    public void DoWaveStart(int waveNumber)
    {
        OnWaveStart?.Invoke(waveNumber);
    }
}
