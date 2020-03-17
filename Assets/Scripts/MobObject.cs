using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : EntityObject {
    public Vector2Int destinationPos;
    public Vector2Int facing;
    public float moveSpeed;
    public float turnSpeed;
    public MobStateEnum state;

    public void Init (MobData mobData) {
        this.pos = mobData.pos;
        this.size = mobData.size;
        this.transform.position = GameUtil.V2IOffsetV3(this.size, this.pos);
        PlayerSetup();
        this.state = MobStateEnum.READY;
    }

    void PlayerSetup() {
        this.myRenderer.material.color = Color.yellow;
    }
}
