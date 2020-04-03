using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBlockSelector : MonoBehaviour {
    public List<BlockData> blockDatas;
    public RectTransform content;
    // Start is called before the first frame update
    void Awake() {
        this.blockDatas = GenerateBlockDatas();
        this.content = transform.GetComponent<ScrollRect>().content;

    }

    List<BlockData> GenerateBlockDatas() {
        List<BlockData> newBlockDatas = new List<BlockData>();
        newBlockDatas.Add(MakeBlock(4,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(3,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(2,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(1,1,0,0,BlockTypeEnum.FREE));
        return newBlockDatas;
    }


    
    BlockData MakeBlock(int aWidth, int aHeight, int aX, int aY, BlockTypeEnum aType) {
        BlockData newBlockData = ScriptableObject.CreateInstance("BlockData") as BlockData;
        Color newColor = Color.magenta;
        newBlockData.Init(new Vector2Int(aWidth, aHeight), new Vector2Int(aX, aY), newColor, aType);
        return newBlockData;
    }
}
