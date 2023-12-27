using UnityEngine;

public class SpawnerFruits : MonoBehaviour
{
    [SerializeField] private GameObject[] _fruits;

    private float timer = 0;
    private float timeBtwSpawn = 0;

    private void Start()
    {
        timeBtwSpawn = Random.Range(0.2f, 0.5f);
    }

    private void Update()
    {
        if (timer >= timeBtwSpawn)
        {
            Spawn();
            timer = 0;
            timeBtwSpawn = Random.Range(0.2f, 0.5f);
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private void Spawn()
    {
        GameObject obj = _fruits[Random.Range(0, _fruits.Length)];
        Vector2 position = new Vector2(Random.Range(-2f, 2f), transform.position.y);
        obj = Instantiate(obj, position, Quaternion.Euler(0, 0, Random.Range(0, 360f)));
        Destroy(obj, 2f);
    }
}
