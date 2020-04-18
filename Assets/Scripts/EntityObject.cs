using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityObject : MonoBehaviour {
    public Vector2Int pos;
    public Vector2Int size;
    public Vector2Int facing;

    public Renderer myRenderer;
    public LocomotionStateEnum locomotionState;
    public List<IComponent> componentList;
    
    public Color color;
    public bool isHighlighted;

    void Awake() {
        this.myRenderer = GetComponent<Renderer>();
        this.locomotionState = LocomotionStateEnum.READY;
        this.isHighlighted = false;
        componentList = new List<IComponent>();
        foreach (IComponent component in GetComponents(typeof(IComponent))) {
            componentList.Add(component);
        }
    }

    public Vector2Int GetSize() {
        return this.size;
    }
    
    public Vector2Int GetPos() {
        return this.pos;
    }

    public List<Vector2Int> GetOccupiedPos() {
        List<Vector2Int> occupiedPosList = new List<Vector2Int>();
        for (int x = this.pos.x; x < this.pos.x + this.size.x; x++) {
            for (int y = this.pos.y; y < this.pos.y + this.size.y; y++) {
                Vector2Int currentPos = new Vector2Int(x, y);
                occupiedPosList.Add(currentPos);
            }
        }
        return occupiedPosList;
    }
    
    public bool IsInsideSelf(Vector2Int aPos) {
        return GameUtil.IsInside(aPos, this.pos, this.size);
    }

    public void SetColor(Color aColor) {
        this.myRenderer.material.color = aColor;
        foreach (Transform child in this.transform) {
            child.GetComponent<Renderer>().material.color = aColor;
        }
    }

    public void ResetColor() {
        this.myRenderer.material.color = this.color;
        foreach (Transform child in this.transform) {
            child.GetComponent<Renderer>().material.color = this.color;
        }
    }

    public void Highlight(bool aHighlight) {
        this.isHighlighted = aHighlight;
        if (aHighlight) {
            SetColor(Color.red);
        } else {
            SetColor(this.color);
        }
    }

}
