using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public Transform target;
    public int speed;
    public int damage = 5;
    public Hittable hp;
    public HealthBar healthui;
    public bool dead;

    public float last_attack;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameManager.Instance.player.transform;
        if (hp == null)
        {
            hp = new Hittable(50, Hittable.Team.MONSTERS, gameObject);
        }
        hp.OnDeath += Die;
        healthui.SetHealth(hp);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state != GameManager.GameState.INWAVE || target == null)
        {
            GetComponent<Unit>().movement = Vector2.zero;
            return;
        }

        Vector3 direction = target.position - transform.position;
        if (direction.magnitude < 2f)
        {
            GetComponent<Unit>().movement = Vector2.zero;
            DoAttack();
        }
        else
        {
            GetComponent<Unit>().movement = direction.normalized * speed;
        }
    }
    
    void DoAttack()
    {
        if (last_attack + 2 < Time.time)
        {
            last_attack = Time.time;
            PlayerController player = target.gameObject.GetComponent<PlayerController>();
            if (player.hp != null)
            {
                player.hp.Damage(new Damage(damage, Damage.Type.PHYSICAL));
            }
        }
    }


    void Die()
    {
        if (!dead)
        {
            dead = true;
            GameManager.Instance.RemoveEnemy(gameObject);
            Destroy(gameObject);
        }
    }
}
