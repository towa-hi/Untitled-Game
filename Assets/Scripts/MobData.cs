using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobData : ScriptableObject {
    public Vector2Int size;
    
    public void Init(Vector2Int size) {
        this.size = size;
    }
}
