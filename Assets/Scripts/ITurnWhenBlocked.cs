using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITurnWhenBlocked : IComponent {
    public float turnTime;
    float t;
    Quaternion oldRotation;

    void Update() {
        
        if (this.entity.locomotionState == LocomotionStateEnum.READY) {
            Vector2Int offsetUp = this.entity.facing + Vector2Int.up;
            Vector2Int offsetDown = this.entity.facing + Vector2Int.down;
            if (!CheckPos(this.entity.facing) && !CheckPos(offsetUp) && !CheckPos(offsetDown)) {
                this.entity.locomotionState = LocomotionStateEnum.TURNING;
                this.entity.facing = new Vector2Int(this.entity.facing.x * -1, 0);
                this.oldRotation = this.transform.rotation;
                this.t = 0f;
                print("path blocked now turning");
            }
        }

        if (this.entity.locomotionState == LocomotionStateEnum.TURNING) {
            this.t += Time.deltaTime / this.turnTime;
            Quaternion newRotation = Quaternion.AngleAxis(180, Vector3.up) * this.oldRotation;
            this.entity.gameObject.transform.rotation = Quaternion.Lerp(this.oldRotation, newRotation, this.t);
            if (this.t >= 1f) {
                this.entity.locomotionState = LocomotionStateEnum.READY;
                print("done turning");
            }
        }
    }
}
