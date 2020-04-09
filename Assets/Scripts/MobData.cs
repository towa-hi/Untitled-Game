using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobData : EntityData {
    public Vector2Int facing;
    public float moveSpeed;
    public float turnSpeed;
    public MobTypeEnum mobType;
    
    public void Init(Vector2Int aSize, Vector2Int aPos, Vector2Int aFacing, MobTypeEnum aMobType) {
        this.size = aSize;
        this.pos = aPos;
        this.facing = aFacing;
        this.mobType = aMobType;
        switch (this.mobType) {
            case MobTypeEnum.PLAYER:
                this.moveSpeed = 1f;
                this.turnSpeed = 1f;
                break;
            case MobTypeEnum.SHUFFLEBOT:
                this.moveSpeed = 1f;
                this.turnSpeed = 0f;
                break;
        }
    }
}
