using System.Collections.Generic;
using UnityEngine;

public class Pool {
    public bool DynamicPooling { get; set; }
    public Queue<ObjectInstance> ObjectQueue { get; private set; }

    public Pool(bool dynamicPooling = false, Queue<ObjectInstance> objectQueue = null) {
        DynamicPooling = dynamicPooling;
        ObjectQueue = objectQueue;
    }
}
