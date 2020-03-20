using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IWalkable : IComponent {
    public float walkSpeed;
    Vector3 startPos;
    Vector3 endPos;
    Vector2Int destination;
    float t;

    void Update() {
        
        if (entity.locomotionState == LocomotionStateEnum.READY) {
            this.destination = this.entity.pos + this.entity.facing;
            print("walking to " + this.destination);
            if (CheckPos(this.entity.facing)) {
                // if (CheckFloor(this.entity.facing)) {
                this.entity.locomotionState = LocomotionStateEnum.WALKING;
                this.startPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                this.entity.pos = this.destination;
                this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                this.t = 0f;
                // }
            }
        }

        if (entity.locomotionState == LocomotionStateEnum.WALKING) {
            t += Time.deltaTime / this.walkSpeed;
            this.entity.gameObject.transform.position = Vector3.Lerp(this.startPos, this.endPos, t);
            if (t >= 1f) {
                this.entity.locomotionState = LocomotionStateEnum.READY;
                this.entity.gameObject.transform.position = endPos;
            }
        }
    }
}
