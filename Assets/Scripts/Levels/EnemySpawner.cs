using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;

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
    private bool spawning;


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

        string enemyData = File.ReadAllText("./Assets/Resources/enemies.json");
        enemyConfig = JsonConvert.DeserializeObject<List<Enemy>>(enemyData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        StopAllCoroutines();
        spawning = false;
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        GameManager.Instance.player.GetComponent<PlayerController>().StartLevel();
        LevelSelector.Instance.Difficulty = levelname;

        Levels level = LevelSelector.Instance.GetLevel(levelname);
        GameManager.Instance.NewRun(levelname, level == null ? 0 : level.waves);
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        dict["wave"] = 1;

        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        if (GameManager.Instance.state == GameManager.GameState.WAVEEND)
        {
            StartCoroutine(SpawnWave());
            return;
        }

        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER ||
            GameManager.Instance.state == GameManager.GameState.VICTORY)
        {
            ReturnToStart();
        }
    }

    public void ReturnToStart()
    {
        StopAllCoroutines();
        spawning = false;
        GameManager.Instance.ClearEnemies();
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        GameManager.Instance.resultMessage = "";
        level_selector.gameObject.SetActive(true);
    }

    IEnumerator SpawnWave()
    {
        if (spawning) yield break;
        spawning = true;

        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.wave = dict["wave"];
        for (int i = 3; i > 0; i--)
        {
            GameManager.Instance.countdown = i;
            yield return new WaitForSeconds(1);
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;

        List<Spawn> spawns = LevelSelector.Instance.GetSpawn(LevelSelector.Instance.Difficulty);
        if(spawns != null)
        {
            foreach(Spawn mob in spawns)
            {
                if (GameManager.Instance.state != GameManager.GameState.INWAVE) break;
                yield return SpawnEnemies(mob);
            }
        }

        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0 &&
                                        GameManager.Instance.state == GameManager.GameState.INWAVE);
        if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
        {
            spawning = false;
            yield break;
        }

        Levels level = LevelSelector.Instance.GetLevel(LevelSelector.Instance.Difficulty);
        if (level != null && level.waves > 0 && dict["wave"] >= level.waves)
        {
            GameManager.Instance.resultMessage = "You cleared " + level.name + "!";
            GameManager.Instance.state = GameManager.GameState.VICTORY;
        }
        else
        {
            GameManager.Instance.state = GameManager.GameState.WAVEEND;
            dict["wave"]++;
        }
        spawning = false;
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
        if (GameManager.Instance.state != GameManager.GameState.INWAVE) yield break;

        Enemy mobEntity = GetEnemy(mob.enemy);
        if (mobEntity == null)
        {
            Debug.LogWarning("Could not find enemy named " + mob.enemy);
            yield break;
        }

        SpawnPoint spawn_point = ChooseSpawnPoint(mob.location);
        if (spawn_point == null)
        {
            Debug.LogWarning("No spawn points are available.");
            yield break;
        }

        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(mobEntity.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();

        en.hp = new Hittable(EvaluateInt(mob.hp, mobEntity.hp), Hittable.Team.MONSTERS, new_enemy);
        en.speed = EvaluateInt(mob.speed, mobEntity.speed);
        en.damage = EvaluateInt(mob.damage, mobEntity.damage);
        GameManager.Instance.AddEnemy(new_enemy);

        yield return new WaitForSeconds(EvaluateFloat(mob.delay ?? "1", 1));
    }

    IEnumerator SpawnEnemies(Spawn mob)
    {
        int spawnCount = EvaluateInt(mob.count, 0);
        int spawned = 0;

        while (spawned < spawnCount && GameManager.Instance.state == GameManager.GameState.INWAVE)
        {
            if (mob.sequence != null)
            {
                foreach (int amount in mob.sequence)
                {
                    for (int j = 0; j < amount && spawned < spawnCount; ++j)
                    {
                        yield return SpawnEnemy(mob);
                        spawned++;
                    }
                }
            }
            else
            {
                yield return SpawnEnemy(mob);
                spawned++;
            }
        }
    }

    private Enemy GetEnemy(string enemyName)
    {
        foreach(Enemy enemy in enemyConfig)
        {
            if(enemy.name == enemyName) return enemy;
        }
        return null;
    }

    private SpawnPoint ChooseSpawnPoint(string location)
    {
        if (SpawnPoints == null || SpawnPoints.Length == 0) return null;

        List<SpawnPoint> choices = new List<SpawnPoint>();
        foreach (SpawnPoint spawnPoint in SpawnPoints)
        {
            if (SpawnLocationMatches(spawnPoint, location))
            {
                choices.Add(spawnPoint);
            }
        }
        if (choices.Count == 0) choices.AddRange(SpawnPoints);
        return choices[Random.Range(0, choices.Count)];
    }

    private bool SpawnLocationMatches(SpawnPoint spawnPoint, string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return true;

        string lower = location.ToLowerInvariant();
        bool wantsRed = lower.Contains("red");
        bool wantsGreen = lower.Contains("green");
        bool wantsBone = lower.Contains("bone");

        if (!wantsRed && !wantsGreen && !wantsBone) return true;
        if (wantsRed && spawnPoint.kind == SpawnPoint.SpawnName.RED) return true;
        if (wantsGreen && spawnPoint.kind == SpawnPoint.SpawnName.GREEN) return true;
        if (wantsBone && spawnPoint.kind == SpawnPoint.SpawnName.BONE) return true;
        return false;
    }

    private int EvaluateInt(string expression, int baseValue)
    {
        if (string.IsNullOrWhiteSpace(expression)) return baseValue;
        return Mathf.FloorToInt(EvaluateExpression(expression, baseValue));
    }

    private float EvaluateFloat(string expression, int baseValue)
    {
        if (string.IsNullOrWhiteSpace(expression)) return baseValue;
        return EvaluateExpression(expression, baseValue);
    }

    private float EvaluateExpression(string expression, int baseValue)
    {
        Stack<float> values = new Stack<float>();
        string[] tokens = expression.Split(' ');

        foreach (string token in tokens)
        {
            if (token == "") continue;
            if (token == "wave")
            {
                values.Push(dict["wave"]);
            }
            else if (token == "base")
            {
                values.Push(baseValue);
            }
            else if (token == "+" || token == "-" || token == "*" || token == "/" || token == "%")
            {
                float b = values.Pop();
                float a = values.Pop();
                if (token == "+") values.Push(a + b);
                if (token == "-") values.Push(a - b);
                if (token == "*") values.Push(a * b);
                if (token == "/") values.Push(b == 0 ? 0 : a / b);
                if (token == "%") values.Push(b == 0 ? 0 : a % b);
            }
            else
            {
                values.Push(float.Parse(token, CultureInfo.InvariantCulture));
            }
        }

        return values.Count == 0 ? baseValue : values.Pop();
    }
}
