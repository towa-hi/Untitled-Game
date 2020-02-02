using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour
{
    public BlockData blockData;
    public Vector2Int pos;
    public BlockStateEnum stateEnum;
    public Color color;

    public void Init(BlockData blockData, BlockState blockState) {
        this.blockData = blockData;
        this.pos = new Vector2Int(blockState.pos.x, blockState.pos.y);
        this.stateEnum = blockState.stateEnum;
        Vector3 thiccness = new Vector3(0,0,1f);
        transform.localScale = GameUtil.V2IToV3(blockData.size) + thiccness;
        if (blockData.type == BlockTypeEnum.FIXED) {
            this.color = Color.black;
        } else {
            this.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        GetComponent<Renderer>().material.color = color;
        gameObject.name = "Block (" + blockData.size.x + "x" + blockData.size.y + ")";
    }

    //debug
    // void OnMouseEnter() {
    //     Highlight();
    // }

    // void OnMouseExit() {
    //     UnHighlight();
    // }

    public void Move(Vector2Int pos) {
        StartCoroutine(MoveCoroutine(pos));
    }

    public void Highlight() {
        print("highlighted");
        GetComponent<Renderer>().material.color = Color.red;
    }

    public void UnHighlight() {
        GetComponent<Renderer>().material.color = color;
    }

    public bool CheckSelfPos(Vector2Int pos) {
        if (pos.x >= this.pos.x && pos.x < this.pos.x + this.blockData.size.x && pos.y >= this.pos.y && pos.y < this.pos.y + this.blockData.size.y) {
            return true;
        } else {
            return false;
        }
    }

    public IEnumerator MoveCoroutine(Vector2Int pos) {
        Vector3 targetPos = GameUtil.V2IOffsetV3(blockData.size, pos);
        Vector3 currentPos = transform.position;
        this.pos = pos;
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
