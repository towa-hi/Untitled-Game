using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityObject : MonoBehaviour {
    public Vector2Int pos;
    public Vector2Int size;
    public Renderer myRenderer;

    void Awake() {
        this.myRenderer = GetComponent<Renderer>();
    }

    public Vector2Int GetSize() {
        return this.size;
    }
    
    public Vector2Int GetPos() {
        return this.pos;
    }

    public bool IsInsideSelf(Vector2Int aPos) {
        return GameUtil.IsInside(aPos, this.pos, this.size);
    }
}
