using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHoppable : IComponent {
    public float hopTime;
    public Vector3 startPos;
    public Vector3 endPos;
    Vector2Int destination;
    float t;

    void Update() {
        if (this.entity.locomotionState == LocomotionStateEnum.READY) {
            Vector2Int newOffsetDown = this.entity.facing + Vector2Int.down;
            Vector2Int newOffsetUp = this.entity.facing + Vector2Int.up;
            if (CheckPos(newOffsetDown) && CheckFloor(newOffsetDown)) {
                this.entity.locomotionState = LocomotionStateEnum.HOPPING;
                this.t = 0f;
                this.startPos = this.transform.position;
                this.entity.pos += newOffsetDown;
                this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                print("hopping down to" + GameUtil.V3ToV2I(this.endPos));
            } else if (CheckPos(newOffsetUp) && CheckFloor(newOffsetUp)) {
                this.entity.locomotionState = LocomotionStateEnum.HOPPING;
                this.t = 0f;
                this.startPos = this.transform.position;
                this.entity.pos += newOffsetUp;
                this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                print("hopping up to" + GameUtil.V3ToV2I(this.endPos));
            }

        }

        if (this.entity.locomotionState == LocomotionStateEnum.HOPPING) {
            this.t += Time.deltaTime / this.hopTime;
            this.entity.gameObject.transform.position = Vector3.Lerp(this.startPos, this.endPos, this.t);
            if (this.t >= 1f) {
                this.entity.locomotionState = LocomotionStateEnum.READY;
                this.entity.gameObject.transform.position = endPos;
            }
        }

    }
}
