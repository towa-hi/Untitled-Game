using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : EntityObject {
    // set by Init()
    public BlockStateEnum state;
    public BlockTypeEnum type;
    public bool isUnderEntity;
    // move this to its own component later
    public Vector2Int ghostPos;

    // set by editor
    public GameObject studMaster;
    public float smoothMoveSpeed;

    public void Init (BlockData aBlockData) {
        this.size = aBlockData.size;
        this.pos = aBlockData.pos;
        this.ghostPos = aBlockData.pos;
        this.type = aBlockData.type;
        this.color = aBlockData.color;
        this.isUnderEntity = false;
        this.isHighlighted = false;

        this.name = "Block size: " + this.size + " startingpos: " + this.pos;

        Vector3 thiccness = new Vector3(0, 0, 2f);
        this.transform.localScale = GameUtil.V2IToV3(this.size) + thiccness;

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

    public void InitPreview (BlockData aBlockData) {
        this.size = aBlockData.size;
        this.pos = Vector2Int.zero;
        this.ghostPos = Vector2Int.zero;
        this.type = aBlockData.type;
        this.color = aBlockData.color;
        this.isUnderEntity = false;

        this.name = "Block size: " + this.size + " not yet added";

        Vector3 thiccness = new Vector3(0, 0, 2f);
        Transform oldParent = this.transform.parent;
        this.transform.parent = null;
        this.transform.localScale = GameUtil.V2IToV3(this.size) + thiccness;
        
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
        this.transform.parent = oldParent;
        this.transform.localPosition = Vector3.zero;
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



    public void SmoothMove(Vector3 aDestination) {
        StartCoroutine(MoveCoroutine(aDestination));
    }

    public IEnumerator MoveCoroutine(Vector3 aDestination) {
        Vector3 currentPos = transform.position;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / this.smoothMoveSpeed;
            transform.position = Vector3.Lerp(currentPos, aDestination, t);
            yield return null;
        }
    }


}
