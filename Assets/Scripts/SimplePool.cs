using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    public static SimplePool Instance;
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary.Add(key, new Queue<GameObject>());
        }

        if (poolDictionary[key].Count > 0)
        {
            GameObject obj = poolDictionary[key].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else 
        {
            GameObject obj = Instantiate(prefab, position, rotation);
            PoolMember member = obj.GetComponent<PoolMember>();
            if (member == null) member = obj.AddComponent<PoolMember>();
            member.poolKey = key;
            return obj;
        }
    }

    public void Despawn(GameObject obj)
    {
        PoolMember member = obj.GetComponent<PoolMember>();
        if (member != null && poolDictionary.ContainsKey(member.poolKey))
        {
            obj.SetActive(false); 
            poolDictionary[member.poolKey].Enqueue(obj); 
        }
        else
        {
            Destroy(obj); 
        }
    }
}