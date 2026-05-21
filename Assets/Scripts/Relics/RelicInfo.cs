using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class RelicInfo
{
    public List<Relics> relics = new();
    private static RelicInfo theInstance;

    public static RelicInfo Instance {  get
        {
            if (theInstance == null)
                theInstance = new RelicInfo();
            return theInstance;
        }
    }

    private RelicInfo()
    {
        relics = JsonConvert.DeserializeObject<List<Relics>>(File.ReadAllText("./Assets/Resources/relics.json"));
    }
}

public class Relics
{
    public string name { get; set; }
    public int sprite {  get; set; }
    public Trigger trigger { get; set; }
    public Effect effect { get; set; }
}

public class Trigger
{
    public string description { get; set; }
    public string type { get; set; }
    public string amount;

    public int GetAmount()
    {
        return RPNEvaluator.RPNEvaluator.Evaluate(amount, GameManager.Instance.dict);
    }

    public Trigger(string description, string type, string amount)
    {
        this.description = description;
        this.type = type;
        this.amount = amount;
    }
}

public class Effect
{
    public string description { get; set; }
    public string type { get; set; }
    public string amount;
    public string until { get; set; }

    public int GetAmount()
    {
        return RPNEvaluator.RPNEvaluator.Evaluate(amount, GameManager.Instance.dict);
    }

    public Effect(string description, string type, string amount, string until)
    {
        this.description = description;
        this.type = type;
        this.amount = amount;
        this.until = until;
    }
}