using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using RPNEvaluator;
using System.Linq.Expressions;
public class Enemy
{
    public string name { get; set; }
    public int sprite { get; set; }
    public int hp { get; set; }
    public int speed { get; set; }
    public int damage { get; set; }
}

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector;
    public GameObject button;
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    public Dictionary<string, int> dict;
    private List<Enemy> enemyConfig;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int y = 40;
        int i = 0;
        foreach(Levels level in LevelSelector.Instance.levelConfig)
        {
            GameObject selector = Instantiate(button, level_selector.transform);
            selector.transform.localPosition = new Vector3(0, y-i);
            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(level.name);
            i += 40;
        }
        dict = new Dictionary<string, int>();
        dict.TryAdd("wave", 1); //initialize dict
        //store enemy info
        string enemyData = File.ReadAllText("./Assets/Resources/enemies.json");
        enemyConfig = JsonConvert.DeserializeObject<List<Enemy>>(enemyData);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        LevelSelector.Instance.Difficulty = levelname;
        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        //get level difficulty
        List<Spawn> spawns = null;
        foreach (Levels level in LevelSelector.Instance.levelConfig)
        {
            if (level.name == LevelSelector.Instance.Difficulty) { spawns = level.spawns; break; }
        }
        //loop through each enemy type
        if(spawns != null)
        {
            foreach(Spawn mob in spawns)
            {
                yield return SpawnEnemies(mob);
            }
        }
        
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        dict["wave"]++;
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);
        
        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator SpawnEnemy(Spawn mob)
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        //get mob of matching name
        Enemy mobEntity = null;
        foreach(Enemy e in enemyConfig)
        {
            if(e.name == mob.enemy) { mobEntity = e; break; }
        }

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(mobEntity.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        //update hp
        dict.TryAdd("base", mobEntity.hp);
        int mobHP = RPNEvaluator.RPNEvaluator.Evaluate(mob.hp ?? mobEntity.hp.ToString(),dict);
        en.hp = new Hittable(mobEntity.hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = mobEntity.speed;
        GameManager.Instance.AddEnemy(new_enemy);
        
        yield return new WaitForSeconds(RPNEvaluator.RPNEvaluator.Evaluatef(mob.delay ?? "1",dict));
    }

    IEnumerator SpawnEnemies(Spawn mob)
    {
        int spawnCount = RPNEvaluator.RPNEvaluator.Evaluate(mob.count, dict);
        for (int i = 0; i < spawnCount; ++i)
        {
            if (mob.sequence != null)
            {
                foreach (int amount in mob.sequence)
                {
                    for (int j = 0; j < amount && GameManager.Instance.enemy_count < spawnCount; ++j)
                    {
                        yield return SpawnEnemy(mob);
                    }
                }
            }
            else
                yield return SpawnEnemy(mob);
        }
    }

}