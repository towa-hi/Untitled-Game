using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : EntityObject {
    public Vector2Int destinationPos;
    public MobTypeEnum mobType;
    public float turnSpeed;
    public List<IComponent> mobComponents;

    public void Init (MobData aMobData) {
        this.pos = aMobData.pos;
        this.size = aMobData.size;
        this.facing = aMobData.facing;
        this.mobType = aMobData.mobType;
        this.transform.position = GameUtil.V2IOffsetV3(this.size, this.pos);
        Vector3 thiccness = new Vector3(0, 0, 2f);
        transform.localScale = GameUtil.V2IToV3(this.size) + thiccness;
        
        this.name = "Mob: " + this.mobType + " startingpos: " + this.pos;
        this.mobComponents = new List<IComponent>();
        PlayerSetup();
    }

    void PlayerSetup() {
        this.myRenderer.material.color = Color.yellow;
        IFallable fallable = gameObject.AddComponent(typeof(IFallable)) as IFallable;
        fallable.fallTime = 1f;
        mobComponents.Add(fallable);
        // TODO: make this work for all the components and move it out of this function
        // into something more generic
    }
}
