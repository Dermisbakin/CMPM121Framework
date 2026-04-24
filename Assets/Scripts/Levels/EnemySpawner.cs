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
        GameObject selector = Instantiate(button, level_selector.transform);
        selector.transform.localPosition = new Vector3(0, 130);
        selector.GetComponent<MenuSelectorController>().spawner = this;
        selector.GetComponent<MenuSelectorController>().SetLevel("Start");
        dict = new Dictionary<string, int>();
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
        //add wave to index
        int wave = dict.TryGetValue("wave", out wave) ? wave : 1;
        int spawnCount = (RPNEvaluator.RPNEvaluator.Evaluate(LevelSelector.Instance.levelConfig[0].spawns[1].count, dict) == 10) ? 12:10;
        for (int i = 0; i < spawnCount; ++i)
        {
            yield return SpawnEnemy();
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
        if (!dict.TryAdd("wave", wave)) dict["wave"]++;
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

    IEnumerator SpawnEnemy()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;

        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(enemyConfig[1].sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(enemyConfig[1].hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = enemyConfig[1].speed;
        GameManager.Instance.AddEnemy(new_enemy);
        
        float delay = RPNEvaluator.RPNEvaluator.Evaluatef(LevelSelector.Instance.levelConfig[0].spawns[1].delay ?? "1.0", dict);
        yield return new WaitForSeconds(delay);
    }


}