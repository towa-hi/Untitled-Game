using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBlockSelector : MonoBehaviour {
    public List<BlockData> blockDatas;
    public RectTransform content;
    public GameObject blockSelectorItemMaster;
    
    void Start() {
        this.blockDatas = GenerateBlockDatas();
        foreach (BlockData blockData in blockDatas) {
            GameObject newBlockSelectorItem = (GameObject)Instantiate(blockSelectorItemMaster, content);
            UIBlockSelectorItem newUIBlockSelectorItem = newBlockSelectorItem.GetComponent<UIBlockSelectorItem>();
            newUIBlockSelectorItem.button.onClick.AddListener(delegate {OnBlockSelectorItemButtonClick(blockData);});
        }
    }

    void OnBlockSelectorItemButtonClick(BlockData aBlockData) {
        print(aBlockData.size);
    }
    List<BlockData> GenerateBlockDatas() {
        List<BlockData> newBlockDatas = new List<BlockData>();
        newBlockDatas.Add(MakeBlock(4,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(3,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(2,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(1,1,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(4,2,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(3,2,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(2,2,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(1,2,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(4,3,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(3,3,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(2,3,0,0,BlockTypeEnum.FREE));
        newBlockDatas.Add(MakeBlock(1,3,0,0,BlockTypeEnum.FREE));
        return newBlockDatas;
    }


    
    BlockData MakeBlock(int aWidth, int aHeight, int aX, int aY, BlockTypeEnum aType) {
        BlockData newBlockData = ScriptableObject.CreateInstance("BlockData") as BlockData;
        Color newColor = Color.magenta;
        newBlockData.Init(new Vector2Int(aWidth, aHeight), new Vector2Int(aX, aY), newColor, aType);
        return newBlockData;
    }
}
