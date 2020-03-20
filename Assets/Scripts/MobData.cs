﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobData : EntityData {
    public Vector2Int facing;
    public float moveSpeed;
    public float turnSpeed;
    
    public void Init(Vector2Int aSize, Vector2Int aPos, Vector2Int aFacing, float aMoveSpeed, float aTurnSpeed) {
        this.size = aSize;
        this.pos = aPos;
        this.facing = aFacing;
        this.moveSpeed = aMoveSpeed;
        this.turnSpeed = aTurnSpeed;
    }

    public static MobData GeneratePlayer(Vector2Int aStartingPos) {
        MobData playerData = ScriptableObject.CreateInstance("MobData") as MobData;
        Vector2Int playerSize = new Vector2Int(2, 3);
        playerData.Init(playerSize, aStartingPos, Vector2Int.right, 1f, 1f);
        return playerData;
    }

}
