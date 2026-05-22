using UnityEngine;
using System;

public class RelicTrigger : EventBus
{
    public Action OnTriggered;

    public virtual void Register() { }
    public virtual void Unregister() { }
}

// player take damage -> fire
public class TakeDamageTrigger : RelicTrigger
{
    public override void Register()
    {
        EventBus.Instance.OnPlayerDamaged += HandlePlayerDamaged;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnPlayerDamaged -= HandlePlayerDamaged;
    }

    void HandlePlayerDamaged(Vector3 pos, Damage dmg)
    {
        OnTriggered?.Invoke();
    }
}

public class StandStillTrigger : RelicTrigger
{
    float requiredSeconds;
    float stillTime;
    bool isActive;
    //bool wasMoving;

    public Action OnStoppedMoving;

    public StandStillTrigger(float seconds)
    {
        requiredSeconds = seconds;
        stillTime = 0f;
        isActive = false;
        //wasMoving = false;
    }

    public override void Register()
    {
        // EventBus.Instance.OnPlayerMoved += HandlePlayerMoved;
        CoroutineManager.Instance.Run(Tick());
    }

    public override void Unregister()
    {
        // EventBus.Instance.OnPlayerMoved -= HandlePlayerMoved;
    }

    void HandlePlayerMoved()
    {
        stillTime = 0f;
        if (isActive)
        {
            isActive = false;
            OnStoppedMoving?.Invoke();
        }
        //wasMoving = true;
    }

    System.Collections.IEnumerator Tick()
    {
        while (true)
        {
            yield return new UnityEngine.WaitForSeconds(0.1f);
            if (GameManager.Instance.state != GameManager.GameState.INWAVE)
            {
                stillTime = 0f;
                continue;
            }
            stillTime += 0.1f;
            if (stillTime >= requiredSeconds && !isActive)
            {
                isActive = true;
                OnTriggered?.Invoke();
            }
        }
    }
}

public class OnKillTrigger : RelicTrigger
{
    public override void Register()
    {
        EventBus.Instance.OnEnemyKilled += HandleEnemyKilled;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnEnemyKilled -= HandleEnemyKilled;
    }

    void HandleEnemyKilled(GameObject enemy)
    {
        OnTriggered?.Invoke();
    }
}

public class OnCastTrigger : RelicTrigger
{
    public override void Register()
    {
        EventBus.Instance.OnSpellCast += HandleSpellCast;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnSpellCast -= HandleSpellCast;
    }

    void HandleSpellCast()
    {
        OnTriggered?.Invoke();
    }
}

public class OnWaveStartTrigger : RelicTrigger
{
    public override void Register()
    {
        EventBus.Instance.OnWaveStart += HandleWaveStart;
    }

    public override void Unregister()
    {
        EventBus.Instance.OnWaveStart -= HandleWaveStart;
    }

    void HandleWaveStart(int waveNumber)
    {
        OnTriggered?.Invoke();
    }
}