using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobData : EntityData {
    public Vector2Int facing;
    public float moveSpeed;
    public float turnSpeed;
    public string mobPrefabName;
    
    public void Init(Vector2Int aSize, Vector2Int aPos, Vector2Int aFacing, string aMobPrefabName) {
        this.size = aSize;
        this.pos = aPos;
        this.facing = aFacing;
        this.mobPrefabName = aMobPrefabName;
    }
}
