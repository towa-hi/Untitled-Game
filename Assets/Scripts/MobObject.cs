using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : EntityObject {
    public Vector2Int destinationPos;
    
    public float turnSpeed;

    public void Init (MobData aMobData) {
        this.pos = aMobData.pos;
        this.size = aMobData.size;
        this.facing = aMobData.facing;
        this.transform.position = GameUtil.V2IOffsetV3(this.size, this.pos);
        PlayerSetup();
    }

    void PlayerSetup() {
        this.myRenderer.material.color = Color.yellow;
    }

    
    // IEnumerator MoveCoroutine(Vector3 targetPos) {

    // }
}
