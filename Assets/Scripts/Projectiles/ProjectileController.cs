using UnityEngine;
using System;
using System.Collections;

public class ProjectileController : MonoBehaviour
{
    public float lifetime;
    public event Action<Hittable,Vector3> OnHit;
    public event Action<GameObject, Vector3> OnDestroy;
    public ProjectileMovement movement;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (movement == null) return;
        movement.Movement(transform);
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("projectile")) return;
        if (collision.gameObject.CompareTag("unit"))
        {
            var ec = collision.gameObject.GetComponent<EnemyController>();
            if (ec != null)
            {
                OnHit(ec.hp, transform.position);
            }
            else
            {
                var pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null)
                {
                    OnHit(pc.hp, transform.position);
                }
            }

        }
        var stats = GameManager.Instance.player.GetComponent<PlayerController>()?.spellui?.spell?.stats;
        if (stats == null || stats.isPiercing == false)
        {
            if (OnDestroy != null) OnDestroy(gameObject, transform.position);
            else Destroy(gameObject);
        }
        else SetLifetime(10f);
    }

    public void SetLifetime(float lifetime)
    {
        StartCoroutine(Expire(lifetime));
    }

    IEnumerator Expire(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
