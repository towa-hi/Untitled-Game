using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IWalkable : IComponent {
    public float walkTime;
    Vector3 startPos;
    Vector3 endPos;
    Vector2Int destination;
    float t;
    Quaternion oldRotation;

    void Update() {
        if (BoardManager.Instance.timeState == TimeStateEnum.NORMAL) {
            if (this.entity.locomotionState == LocomotionStateEnum.READY) {
                if (CheckPos(this.entity.facing) && CheckFloor(this.entity.facing)) {
                    this.destination = this.entity.pos + this.entity.facing;
                    // print("walking to " + this.destination);
                    this.entity.locomotionState = LocomotionStateEnum.WALKING;
                    this.startPos = this.transform.position;
                    this.entity.pos = this.destination;
                    this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                    this.t = 0f;
                } else if (CheckPos(this.entity.facing + Vector2Int.up) && CheckFloor(this.entity.facing + Vector2Int.up)) {
                    this.destination = this.entity.pos + this.entity.facing + Vector2Int.up;
                    // print("hopping up to " + this.destination);
                    this.entity.locomotionState = LocomotionStateEnum.WALKING;
                    this.startPos = this.transform.position;
                    this.entity.pos = this.destination;
                    this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                    this.t = 0f;
                } else if (CheckPos(this.entity.facing + Vector2Int.down) && CheckFloor(this.entity.facing + Vector2Int.down)) {
                    this.destination = this.entity.pos + this.entity.facing + Vector2Int.down;
                    // print("hopping down to " + this.destination);
                    this.entity.locomotionState = LocomotionStateEnum.WALKING;
                    this.startPos = this.transform.position;
                    this.entity.pos = this.destination;
                    this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                    this.t = 0f;
                } else {
                    this.entity.locomotionState = LocomotionStateEnum.TURNING;
                    this.entity.facing = new Vector2Int(this.entity.facing.x * -1, 0);
                    this.oldRotation = this.transform.rotation;
                    this.t = 0f;
                }
            }

            if (this.entity.locomotionState == LocomotionStateEnum.WALKING) {
                this.t += Time.deltaTime / this.walkTime;
                this.entity.gameObject.transform.position = Vector3.Lerp(this.startPos, this.endPos, this.t);
                if (this.t >= 1f) {
                    this.entity.gameObject.transform.position = endPos;
                    this.entity.locomotionState = LocomotionStateEnum.READY;
                }
            }

            if (this.entity.locomotionState == LocomotionStateEnum.TURNING) {
                this.t += Time.deltaTime / this.walkTime;
                Quaternion newRotation = Quaternion.AngleAxis(180, Vector3.up) * this.oldRotation;
                this.entity.gameObject.transform.rotation = Quaternion.Lerp(this.oldRotation, newRotation, this.t);
                if (this.t >= 1f) {
                    this.entity.locomotionState = LocomotionStateEnum.READY;
                    // print("done turning");
                }
            }
        }
        
    }
}
