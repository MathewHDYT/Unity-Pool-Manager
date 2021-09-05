using UnityEngine;

[System.Serializable]
public class WeightedObject { // Creating WeightedObject Class with variables
    public GameObject gameObject;
    [Range(0f, 100f)]
    public int weight;
    public int spawnAmount;
}
