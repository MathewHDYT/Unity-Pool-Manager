using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObstacleManager : MonoBehaviour {

    [Header("Spawn Object Settings:")]
    [SerializeField]
    private List<WeightedObject> weightedObjects;

    private int weightTotal = 1;
    private PoolManager pm;

    private void Start() {
        pm = PoolManager.instance;
        // Create a pool and its parent for each weightedObject.
        foreach (var spawnPrefab in weightedObjects) {
            pm.CreatePool(spawnPrefab.gameObject, spawnPrefab.spawnAmount);
            pm.ParentPool(spawnPrefab.gameObject, this.transform);
            weightTotal += spawnPrefab.weight;
        }
        // Order weightedObjects by their assigned weight.
        weightedObjects = weightedObjects.OrderByDescending(x => x.weight).ToList();
    }

    public void SpawnObjects() {
        GameObject randomSpawnPrefab = GetRandomSpawnPrefab();

        if (randomSpawnPrefab != null) {
            Vector3 spawnPos = GetRandomPosition();
            pm.ReuseObject(randomSpawnPrefab, spawnPos, Quaternion.identity, Vector2.zero);
        }
    }

    private GameObject GetRandomSpawnPrefab() {
        int randomNumber = Random.Range(0, weightTotal);
        int weightSum = 0;

        foreach (var spawnPrefab in weightedObjects) {
            weightSum += spawnPrefab.weight;
            if (randomNumber < weightSum) {
                return spawnPrefab.gameObject;
            }
        }

        return null;
    }

    private Vector3 GetRandomPosition() {
        float randomXPos = Random.Range(-15f, 15f);
        float randomYPos = Random.Range(-7.5f, 10f);
        return new Vector3(randomXPos, randomYPos, 5f);
    }
}
