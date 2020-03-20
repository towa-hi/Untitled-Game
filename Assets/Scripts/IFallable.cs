using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFallable : IComponent {
    public float fallTime;
    Vector3 startPos;
    Vector3 endPos;
    float t;

    void Update() {
        if (entity.locomotionState == LocomotionStateEnum.READY) {
            Init();
        }

        if (entity.locomotionState == LocomotionStateEnum.FALLING) {
            t += Time.deltaTime / fallTime;
            entity.gameObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            if (t >= 1f) {
                this.entity.locomotionState = LocomotionStateEnum.READY;
                this.entity.gameObject.transform.position = endPos;
                Init();
            }
        }
    }

    void Init() {
        if (!CheckFloor(Vector2Int.zero)) {
            this.entity.locomotionState = LocomotionStateEnum.FALLING;
            this.startPos = GameUtil.V2IOffsetV3(entity.size, entity.pos);
            this.entity.pos += Vector2Int.down;
            this.endPos = GameUtil.V2IOffsetV3(entity.size, entity.pos);
            this.t = 0f;
            // entity.locomotationState = LocomotionStateEnum.READY;
        }
    }
}
