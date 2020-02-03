using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject {
    public int par;
    public string title;
    public string creator;
    public Dictionary<BlockData, BlockState> blockDataDict;
    //Dictionary<EntityData, EntityState> entityDataDict;

    public static LevelData GenerateTestLevel() {
        LevelData testLevel = ScriptableObject.CreateInstance("LevelData") as LevelData;
        testLevel.par = 5;
        testLevel.title = "test level";
        testLevel.creator = "test";
        testLevel.blockDataDict = new Dictionary<BlockData, BlockState>();
        AddTestBlock(4,1,0,0, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,2,2,1, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(3,3,4,4, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(3,1,3,3, BlockTypeEnum.FIXED, testLevel.blockDataDict);
        AddTestBlock(2,1,3,7, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(3,2,5,7, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,1,3,8, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(3,1,4,9, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,1,7,9, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,2,2,5, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,1,8,8, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(1,1,9,9, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,1,1,7, BlockTypeEnum.FREE, testLevel.blockDataDict);
        AddTestBlock(2,1,0,8, BlockTypeEnum.FIXED, testLevel.blockDataDict);
        return testLevel;
    }

    public static void AddTestBlock(int width, int height, int x, int y, BlockTypeEnum type, Dictionary<BlockData, BlockState> blockDataDict) {
        BlockData testBlockData = ScriptableObject.CreateInstance("BlockData") as BlockData;
        testBlockData.Init(width, height, type);
        BlockState testBlockState = new BlockState(x, y);
        blockDataDict.Add(testBlockData, testBlockState);
    }
}
