using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : EntityObject {
    // set by Init()
    public BlockStateEnum state;
    public BlockTypeEnum type;
    public Color color;

    // move this to its own component later
    public Vector2Int ghostPos;

    // set by editor
    public GameObject studMaster;

    public void Init (BlockData aBlockData) {
        this.size = aBlockData.size;
        this.pos = aBlockData.pos;
        this.ghostPos = aBlockData.pos;
        this.type = aBlockData.type;
        this.color = aBlockData.color;

        this.name = "Block size: " + this.size + " startingpos: " + this.pos;
        myRenderer = GetComponent<Renderer>();

        Vector3 thiccness = new Vector3(0,0,2f);
        transform.localScale = GameUtil.V2IToV3(this.size) + thiccness;
        myRenderer.material.color = this.color;

        switch (this.type) {
            case BlockTypeEnum.FREE:
                SetState(BlockStateEnum.ACTIVE);
                break;
            case BlockTypeEnum.FIXED:
                this.color = Color.gray;
                SetState(BlockStateEnum.FIXED);
                break;
        }
        myRenderer.material.color = this.color;
        CreateStuds();
    }

    public void SetState(BlockStateEnum aState) {
        this.state = aState;
        switch (this.state) {
            case BlockStateEnum.ACTIVE:
                SetColor(this.color);
                break;
            case BlockStateEnum.GHOST:
                SetColor(Color.green);
                break;
            case BlockStateEnum.INVALID:
                SetColor(Color.red);
                break;
            case BlockStateEnum.FIXED:
                break;
        }
    }

    void CreateStuds() {
        float distanceBetweenStud = 1f / this.size.x;
        float y = 0.5f;
        for (float x = -0.5f; x < 0.5f; x = x + distanceBetweenStud) {
            for (float z = -0.25f; z <= 0.25f; z += 0.5f) {
                GameObject childStud = Instantiate(studMaster, new Vector3(0,0,0), Quaternion.identity);
                childStud.transform.parent = this.transform;
                float xOffset = x + distanceBetweenStud/2;
                childStud.transform.localPosition = new Vector3(xOffset ,y, z);
                childStud.GetComponent<Renderer>().material.color = this.color;
            }
        }
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
}
