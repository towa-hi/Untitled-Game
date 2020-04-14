using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IComponent : MonoBehaviour {
    public EntityObject entity;

    void Awake() {
        if (!entity) {
            this.entity = GetComponent<EntityObject>();
        }
    }

    public bool CheckFloor(Vector2Int aOffset) {
        bool hasFloor = false;
        for (int x = this.entity.pos.x + aOffset.x; x < this.entity.pos.x + aOffset.x + this.entity.size.x; x++) {
            int y = this.entity.pos.y + aOffset.y;
            Vector2Int checkPos = new Vector2Int(x, y - 1);
            BlockObject maybeABlock = BoardManager.GetBlockOnPosition(checkPos);
            if (maybeABlock != null) {
                hasFloor = true;
                return hasFloor;
            }
        }
        return hasFloor;
    }

    public bool CheckPos(Vector2Int aOffset) {
        bool isValid = true;
        for (int x = this.entity.pos.x + aOffset.x; x < this.entity.pos.x + aOffset.x + this.entity.size.x; x++) {
            for (int y = this.entity.pos.y +aOffset.y; y < this.entity.pos.y + aOffset.y + this.entity.size.y; y++) {
                Vector2Int checkPos = new Vector2Int(x, y);
                BlockObject maybeABlock = BoardManager.GetBlockOnPosition(checkPos);
                MobObject maybeAMob = BoardManager.GetMobOnPosition(checkPos);
                if (maybeABlock != null) {
                    return false;
                }
                if (maybeAMob != null) {
                    if (maybeAMob == this.entity) {
                    } else {
                        return false;
                    }
                }
            }
        }
        return isValid;
    }

    public abstract void DoFrame();
}
