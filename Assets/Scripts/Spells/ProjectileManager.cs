using System;
using Unity.VisualScripting;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public GameObject[] projectiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.projectileManager = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3> onHit, Action<GameObject, Vector3> onDestroy, float lifetime, int N, float spray = 0)
    {
        if (lifetime == 0) N = 1;
        for (int i = 0; i < N; i++)
        {
            if (spray != 0)
            {
                spray = UnityEngine.Random.Range(-spray/2, spray/2);
                Quaternion randomdir = Quaternion.AngleAxis(spray, Vector3.forward);
                direction = randomdir * direction;
            }
            GameObject new_projectile = Instantiate(projectiles[which], where + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            new_projectile.GetComponent<ProjectileController>().movement = MakeMovement(trajectory, speed);
            new_projectile.GetComponent<ProjectileController>().OnHit += onHit;
            new_projectile.GetComponent<ProjectileController>().OnDestroy += onDestroy;
            if(lifetime != 0) new_projectile.GetComponent<ProjectileController>().SetLifetime(lifetime);
        }
    }

    public void CreateProjectile(int which, string trajectory, Vector3 where, Vector3 direction, float speed, Action<Hittable, Vector3> onHit,Action<GameObject, Vector3> onDestroy, float lifetime)
    {
        GameObject new_projectile = Instantiate(projectiles[which], where + direction.normalized * 1.1f, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        new_projectile.GetComponent<ProjectileController>().movement = MakeMovement(trajectory, speed);
        new_projectile.GetComponent<ProjectileController>().OnHit += onHit;
        if(onDestroy != null) new_projectile.GetComponent<ProjectileController>().OnDestroy += onDestroy;
        new_projectile.GetComponent<ProjectileController>().SetLifetime(lifetime);
    }

    public ProjectileMovement MakeMovement(string name, float speed)
    {
        if (name == "straight")
        {
            return new StraightProjectileMovement(speed);
        }
        if (name == "homing")
        {
            return new HomingProjectileMovement(speed);
        }

        if (name == "spiraling")
        {
            return new SpiralingProjectileMovement(speed);
        }
        // fallback so projectiles never get null movement
        return new StraightProjectileMovement(speed);
    }
}