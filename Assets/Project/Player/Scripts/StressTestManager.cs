using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Towers.Scripts;
using TMPro;
using UnityEngine;

public class StressTestManager : MonoBehaviour
{
    public TextMeshProUGUI counter;
    public TextMeshProUGUI FPScounter;

    public int currentQuantity = 1;

    public float timeBetweenSpawns = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        _SetQuantityText();
    }

    // Update is called once per frame
    private LinkedList<int> rollingAverageFPS = new LinkedList<int>();
    public int rollingAvgCount = 50;
    private int sum = 0;
    void Update()
    {
        int fps = (int)(1f / Time.smoothDeltaTime);
        sum += fps;
        rollingAverageFPS.AddLast(fps);
        if (rollingAverageFPS.Count > rollingAvgCount)
        {
            int first = rollingAverageFPS.FirstOrDefault();
            sum -= first;
            rollingAverageFPS.RemoveFirst();
        }
        int avg = sum / rollingAverageFPS.Count;
        FPScounter.text = $"FPS: {avg}";
    }

    void CountFrame(int fps)
    {
        
    }

    public void ChangeQuantity(int delta)
    {
        currentQuantity += delta;
        if (currentQuantity < 1)
            currentQuantity = 1;
        _SetQuantityText();
    }

    void _SetQuantityText()
    {
        counter.text = $"{currentQuantity}";
    }

    public void SpawnCurrent()
    {
        StartCoroutine(_DelaySpawnCurrent());
    }

    IEnumerator _DelaySpawnCurrent()
    {
        int left = currentQuantity;
        while (left > 0)
        {
            counter.text = $"{currentQuantity}\nLeft to spawn: {left}";
            yield return new WaitForSeconds(timeBetweenSpawns);
            EnemyManager.SpawnRandom();
            left--;
        }
        _SetQuantityText();
    }

    public void ClearAllEnemies()
    {
        HealthController[] hcs = EnemyManager.instance.GetComponentsInChildren<HealthController>();
        foreach (var hc in hcs)
        {
            Destroy(hc.gameObject);
        }

        EnemyManager.instance.enemies = new Dictionary<BasicEnemy, Transform>();
    }

    public void ClearAllTowers()
    {
        TowerSpawnManager.ClearAllTowers();
    }
}
