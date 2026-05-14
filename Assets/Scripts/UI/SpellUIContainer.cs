using UnityEngine;

public class SpellUIContainer : MonoBehaviour
{
    public GameObject[] spellUIs;
    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 1; i< spellUIs.Length; ++i)
        {
            spellUIs[i].SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
