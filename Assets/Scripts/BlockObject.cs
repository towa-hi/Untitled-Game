using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour, EntityInterface {
    public BlockData blockData;
    public Vector2Int pos;
    public Vector2Int ghostPos;
    public BlockStateEnum stateEnum;
    public Color color;
    public bool isChecked;
    //set in editor
    public GameObject studMaster;

    public void Init(BlockData blockData, BlockState blockState) {
        this.isChecked = false;
        this.blockData = blockData;
        this.pos = new Vector2Int(blockState.pos.x, blockState.pos.y);
        this.ghostPos = pos;
        this.stateEnum = blockState.stateEnum;
        Vector3 thiccness = new Vector3(0,0,2f);
        transform.localScale = GameUtil.V2IToV3(blockData.size) + thiccness;
        if (this.blockData.type == BlockTypeEnum.FIXED) {
            this.color = Color.gray;
        } else {
            this.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        GetComponent<Renderer>().material.color = color;
        gameObject.name = "Block (" + this.blockData.size.x + "x" + this.blockData.size.y + ") color " + color.ToString();

        CreateStuds();
    }

    public void SetState(BlockStateEnum newStateEnum) {
        stateEnum = newStateEnum;
        switch (newStateEnum) {
            case BlockStateEnum.ACTIVE:
                GetComponent<Renderer>().material.color = color;
                break;
            case BlockStateEnum.GHOST:
                GetComponent<Renderer>().material.color = Color.green;
                break;
            case BlockStateEnum.NONE:
                break;
        }
    }

    public Vector2Int GetSize() {
        return this.blockData.size;
    }

    public Vector2Int GetPos() {
        return this.pos;
    }

    public bool IsInsideSelf(Vector2Int aPos) {
        return GameUtil.IsInside(aPos, this.pos, this.blockData.size); 
    }

    public void CreateStuds() {
        float distanceBetweenStud = 1f / this.blockData.size.x;
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

    public void MoveV3(Vector3 pos) {
        StartCoroutine(MoveCoroutine(pos));
    }

    public void MoveV2I(Vector2Int pos) {
        StartCoroutine(MoveCoroutine(GameUtil.V2IOffsetV3(pos, this.blockData.size)));
    }

    public void Highlight(Color color) {
        GetComponent<Renderer>().material.color = color;
    }

    public void UnHighlight() {
        GetComponent<Renderer>().material.color = this.color;
    }

    public bool CheckSelfPos(Vector2Int pos) {
        return GameUtil.IsInside(pos, this.pos, this.blockData.size);
    }

    public bool CheckGhostPos(Vector2Int pos) {
        return GameUtil.IsInside(pos, this.ghostPos, this.blockData.size);
    }

    public IEnumerator MoveCoroutine(Vector3 targetPos) {
        Vector3 currentPos = transform.position;
        float t = 0f;
        while (t < 1) {
            t += Time.deltaTime / 1f;
            transform.position = Vector3.Lerp(currentPos, targetPos, t);
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
