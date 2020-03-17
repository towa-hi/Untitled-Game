using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : EntityObject {
    public Vector2Int destinationPos;
    public Vector2Int facing;
    public float moveSpeed;
    public float turnSpeed;
    public MobStateEnum state;

    public void Init (MobData aMobData) {
        this.pos = aMobData.pos;
        this.size = aMobData.size;
        this.transform.position = GameUtil.V2IOffsetV3(this.size, this.pos);
        PlayerSetup();
        this.state = MobStateEnum.READY;
    }

    void PlayerSetup() {
        this.myRenderer.material.color = Color.yellow;
    }

    public void DoNext() {
        switch (this.state) {
            case MobStateEnum.READY:
                break;
            case MobStateEnum.JUMPING:
                break;
            case MobStateEnum.TURNING:
                break;
            case MobStateEnum.WAITING:
                break;
            case MobStateEnum.WALKING:
                break;
        }
    }

    
    // IEnumerator MoveCoroutine(Vector3 targetPos) {

    // }
}
