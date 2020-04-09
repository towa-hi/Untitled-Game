using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFallable : IComponent {
    public float fallTime;
    Vector3 startPos;
    Vector3 endPos;
    float t;

    void Update() {
        if (PlayingManager.Instance.timeState == TimeStateEnum.NORMAL) {
            if (this.entity.locomotionState == LocomotionStateEnum.READY) {
                if (!CheckFloor(Vector2Int.zero)) {
                    this.entity.locomotionState = LocomotionStateEnum.FALLING;
                    this.startPos = this.transform.position;
                    this.entity.pos += Vector2Int.down;
                    this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                    this.t = 0f;
                }
            }

            if (entity.locomotionState == LocomotionStateEnum.FALLING) {
                this.t += Time.deltaTime / this.fallTime;
                this.entity.gameObject.transform.position = Vector3.Lerp(this.startPos, this.endPos, this.t);
                if (this.t >= 1f) {
                    this.entity.locomotionState = LocomotionStateEnum.READY;
                    this.entity.gameObject.transform.position = endPos;
                    //check if fall finished and then do it again if not
                    if (!CheckFloor(Vector2Int.zero)) {
                        this.entity.locomotionState = LocomotionStateEnum.FALLING;
                        this.startPos = this.transform.position;
                        this.entity.pos += Vector2Int.down;
                        this.endPos = GameUtil.V2IOffsetV3(this.entity.size, entity.pos);
                        this.t = 0f;
                    }
                    
                }
            }
        }
    }
}
