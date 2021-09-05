![Unity Pool Manager](https://github.com/MathewHDYT/Unity-Pool-Manager-UPM/blob/main/logo.png/)

[![MIT license](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://lbesson.mit-license.org/)
[![Unity](https://img.shields.io/badge/Unity-2018.1%2B-green.svg?style=flat-square)](https://docs.unity3d.com/2018.1/Documentation/Manual/index.html)
[![GitHub release](https://img.shields.io/github/release/MathewHDYT/Unity-Pool-Manager-UPM/all.svg?style=flat-square)](https://github.com/MathewHDYT/Unity-Pool-Manager-UPM/releases/)
[![GitHub downloads](https://img.shields.io/github/downloads/MathewHDYT/Unity-Pool-Manager-UPM/all.svg?style=flat-square)](https://github.com/MathewHDYT/Unity-Pool-Manager-UPM/releases/)

# Unity Pool Manager (UPM)
Used to easily create and manage prefabs as pools of a given size, that can be easily to save having to utilize destroy and instantiate when wanting to use an instance of that prefab.

## Contents
- [Unity Pool Manager (UPM)](#unity-pool-manager-upm)
  - [Contents](#contents)
  - [Introduction](#introduction)
  - [Installation](#installation)
- [Documentation](#documentation)
  - [Reference to Pool Manager Script](#reference-to-pool-manager-script)
  - [Spawning Pool Objects](#spawning-pool-objects)
  - [Overriding the derived PoolObject Class](#overriding-the-derived-poolobject-class)
  - [Public accesible methods](#public-accesible-methods)
  	- [Create Pool method](#create-pool-method)
  	- [Parent Pool method](#parent-pool-method)
  	- [Reuse Object method](#reuse-object-method)

## Introduction
A lot of games need to instantiate and destroy obstacles or enemies and this small and easily integrated Pool Manager can help you seperate prefabs into pools of a given size to make sure that there are never more instances of that object then you actually need.

**Unity Pool Manager implements the following methods consisting of a way to:**
- Create a pool with a given size (see [Create Pool method](#create-pool-method))
- Parent a pool to a given gameObject (see [Parent Pool method](#parent-pool-method))
- Reuse an Pool Object from the given pool (see [Reuse Object method](#reuse-object-method))

For each method there is a description on how to call it and how to use it correctly for your game in the given section.

## Installation
**Required Software:**
- [Unity](https://unity3d.com/get-unity/download) Ver. 2020.3.17f1

The Pool Manager itself is version independent, as long as .NET5 already exists. Additionally the example project can be opened with Unity itself or the newest release can be downloaded and exectued to test the functionality.

If you prefer the first method, you can simply install the shown Unity version and after installing it you can download the project and open it in Unity (see [Opening a Project in Unity](https://docs.unity3d.com/2021.2/Documentation/Manual/GettingStartedOpeningProjects.html)). Then you can start the game with the play button to test the Pool Managers functionality.

To simply use the Pool Manager in your own project without downloading the Unity project get the ```PoolManager.CS``` and ```PoolObject.CS``` files in the **Example Project/Assets/Scritps/** called or alternatively get them from the newest release (may not include the newest changes) and save them in your own project. Then create a new empty ```gameObject``` and attach the ```PoolManager.CS``` script to it. Now you can easily add pools like shown in [Create Pool method](#create-pool-method).

# Documentation
This documentation strives to explain how to start using the Pool Manager in your project and explains how to call and how to use its publicly accesible methods correctly.

## Reference to Pool Manager Script
To use the Pool Manager to start playing sounds outside of itself you need to reference it. As the Pool Manager is a [Singelton](https://stackoverflow.com/questions/2155688/what-is-a-singleton-in-c) this can be done easily when we get the instance and save it as a private variable in the script that uses the Pool Manager.

```csharp
private PoolManager pm;

void Start() {
    pm = PoolManager.instance;
    // Calling method in PoolManager
    pm.CreatePool(this.gameObject, 10f);
}
```

Alternatively you can directly call the methods this is not advised tough, if it is executed multiple times or you're going to need the instance multiple times in the same file.

```csharp
void Start() {
    PoolManager.CreatePool(this.gameObject, 10f);
}
```

## Spawning Pool Objects
Additionaly the ```WeightedObject.CS``` file can be included to make a lootTable, where certain ```poolObjects``` have a higher change of being spawned then others.

**To add a new ```weightedObject``` to the lootTable simply create a new element in the Weighted Objects array with the properties:**
- ```Game Object``` (The Game ```poolObject``` that should be *instantiated* or *destroyed* when utilizing the pool)
- ```Weight``` (How likely it is that the weight is choosen divide the total weight of all elements by the weight of this element * 100 to get a procentual amount)
- ```Spawn Amount``` (How many ```poolObjects``` we want to have in our pool --> Pool Size)

![Image of WeightedObject Script](https://image.prntscr.com/image/MrIZOJggTeS1xGJKUIGtMw.png)

For this the random ```poolObject``` we want to choose from need to be saved into a list of ```weightedObjects``` and ordered by their value in a descending order.

```csharp
[Header("Spawn Object Settings:")]
[SerializeField]
private List<WeightedObject> weightedObjects;

private int weightTotal = 1;

private void Start() {
    // Create a pool and its parent for each weightedObject.
    foreach (var spawnPrefab in weightedObjects) {
        pm.CreatePool(spawnPrefab.gameObject, spawnPrefab.spawnAmount);
        pm.ParentPool(spawnPrefab.gameObject, this.transform);
        weightTotal += spawnPrefab.weight;
    }
    // Order weightedObjects by their assigned weight.
    weightedObjects = weightedObjects.OrderByDescending(x => x.weight).ToList();
}
```

If we create a pool for each ```weightedObjects```, we can now choose a random one of them while still utilizing the weight we've assigned earlier.

```csharp
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
```

Now that we have a ```gameObject```, we can simply reuse one of these ```gameObjects``` from the pool with the [Reuse Object method](#reuse-object-method).

## Overriding the derived PoolObject Class
**If needed the ```PoolObject``` can implement the derived class to use certain calls like:**
- On Object Reuse (called when the object gets reused from the [Reuse Object method](#reuse-object-method))
- Set Velocity (called when the object gets reused from the [Reuse Object method](#reuse-object-method) with an additional ```Vector2``` that includes the velocity given to the method)
- On Became Invisble (called when the object isn't rendered anymore by the camera see [OnBecameInvisible Docs](https://docs.unity3d.com/2021.2/Documentation/ScriptReference/MonoBehaviour.OnBecameInvisible.html) for more information)
- On Became Visible (called when the object gets rendered again by the camera see [OnBecameVisible Docs](https://docs.unity3d.com/2021.2/Documentation/ScriptReference/MonoBehaviour.OnBecameVisible.html) for more information)

For this you simply need to create a new class that implements the ```PoolObject``` class and then ```override``` the ```virtual``` methods (see [overriding virtual methods](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/virtual)).

## Public accesible methods
This section explains all public accesible methods, especially what they do, how to call them and when using them might be advantageous instead of other methods. We always assume PoolManager instance has been already referenced in the script. If you haven't done that already see [Reference to Pool Manager Script](#reference-to-pool-manager-script).

### Create Pool method
**What it does:**

**How to call it:**

**When to use it:**

### Parent Pool method
**What it does:**

**How to call it:**

**When to use it:**

### Reuse Object method
**What it does:**

**How to call it:**

**When to use it:**

