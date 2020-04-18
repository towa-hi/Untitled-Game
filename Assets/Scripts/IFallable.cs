using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IFallable : IComponent {
    public float fallTime;
    Vector3 startPos;
    Vector3 endPos;
    float t;

    public override void DoFrame() {
        if (PlayingManager.Instance.timeState == TimeStateEnum.NORMAL) {
            if (FanBelowMe()) {
                if (CheckPos(Vector2Int.up)) {
                    this.entity.locomotionState = LocomotionStateEnum.RISING;
                    this.startPos = this.transform.position;
                    this.entity.pos += Vector2Int.up;
                    this.endPos = GameUtil.V2IOffsetV3(this.entity.size, this.entity.pos);
                    this.t = 0f;
                }
            }

            if (this.entity.locomotionState == LocomotionStateEnum.FLOATING) {
                if (!FanBelowMe()) {
                    this.entity.locomotionState = LocomotionStateEnum.READY;
                }
            }
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
                    // if no floor, keep falling
                    if (!CheckFloor(Vector2Int.zero)) {
                        this.entity.locomotionState = LocomotionStateEnum.FALLING;
                        this.startPos = this.transform.position;
                        this.entity.pos += Vector2Int.down;
                        this.endPos = GameUtil.V2IOffsetV3(this.entity.size, entity.pos);
                        this.t = 0f;
                    } else {
                        this.entity.locomotionState = LocomotionStateEnum.READY;
                        this.entity.gameObject.transform.position = endPos;
                    }
                }
            }

            if (entity.locomotionState == LocomotionStateEnum.RISING) {
                this.t += Time.deltaTime / this.fallTime;
                this.entity.gameObject.transform.position = Vector3.Lerp(this.startPos, this.endPos, this.t);
                if (this.t >= 1f) {
                    // if no ceiling, keep rising
                    if (CheckPos(Vector2Int.up)) {
                        this.entity.locomotionState = LocomotionStateEnum.RISING;
                        this.startPos = this.transform.position;
                        this.entity.pos += Vector2Int.up;
                        this.endPos = GameUtil.V2IOffsetV3(this.entity.size, entity.pos);
                        this.t = 0f;
                    } else {
                        this.entity.locomotionState = LocomotionStateEnum.FLOATING;
                        this.entity.gameObject.transform.position = endPos;
                    }
                }
            }
            
        }
    }

    public bool FanBelowMe() {
        // TODO check if above a fan
        // print("IFallable - checking if above a fan");
        for (int x = this.entity.pos.x; x < this.entity.pos.x + this.entity.size.x; x++) {
            bool colIsBlocked = false;
            for (int y = this.entity.pos.y - 1; y >= 0; y--) {
                if (colIsBlocked == false) {
                    Vector2Int checkPos = new Vector2Int(x, y);
                    EntityObject maybeAEntity = BoardManager.GetEntityOnPosition(checkPos);
                    if (maybeAEntity != null) {
                        // print("IFallable - encountered entity. checking if it's a fan or not" + checkPos);
                        if (maybeAEntity is BlockObject) {
                            print("IFallable - entity is a block" + checkPos);
                            BlockObject maybeABlock = maybeAEntity as BlockObject;
                            if (maybeABlock.type == BlockTypeEnum.FAN) {
                                print("IFallable - entity is a fan" + checkPos);
                                return true;
                            } else {
                                print("IFallable - entity is not a fan. colIsBlocked" + checkPos);
                                colIsBlocked = true;
                            }
                        } else {
                            print("IFallable - entity is not a block. colIsBlocked" + checkPos);
                            colIsBlocked = true;
                        }
                        
                    } else {
                        // print("IFallable - nothing at this location");
                    }
                } else {
                    // print("Ifallable - col is already blocked");
                }
            }
        }
        return false;
    }
}
