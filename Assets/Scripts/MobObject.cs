using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobObject : EntityObject {
    public Vector2Int destinationPos;
    public float turnSpeed;
    public bool canSelectUnder;

    public void Init (MobData aMobData) {
        this.pos = aMobData.pos;
        this.size = aMobData.size;
        this.facing = aMobData.facing;
        this.transform.position = GameUtil.V2IOffsetV3(this.size, this.pos);
        Vector3 thiccness = new Vector3(0, 0, 2f);
        transform.localScale = GameUtil.V2IToV3(this.size) + thiccness;
        this.name = "Mob: " + aMobData.mobPrefabName + " startingpos: " + this.pos;


        // foreach(System.Type componentType in aMobData.components) {
        //     IComponent component = gameObject.AddComponent(componentType) as IComponent;
        //     switch (componentType) {
        //         case System.Type IFallableType when IFallableType == typeof(IFallable):
        //             IFallable newIFallable = component as IFallable;
        //             newIFallable.fallTime = 1f;
        //             break;
        //         case System.Type IWalkableType when IWalkableType == typeof(IWalkable):
        //             IWalkable newIWalkable = component as IWalkable;
        //             newIWalkable.walkTime = 1f;
        //             break;
        //     }
        // }
        // PlayerSetup();
    }

    // void PlayerSetup() {
    //     this.myRenderer.material.color = Color.yellow;
    //     IFallable fallable = gameObject.AddComponent(typeof(IFallable)) as IFallable;
    //     fallable.fallTime = 1f;
    //     mobComponents.Add(fallable);
    //     IWalkable walkable = gameObject.AddComponent(typeof(IWalkable)) as IWalkable;
    //     walkable.walkTime = 1f;
    //     mobComponents.Add(walkable);
    //     // TODO: make this work for all the components and move it out of this function
    //     // into something more generic
    // }
}
