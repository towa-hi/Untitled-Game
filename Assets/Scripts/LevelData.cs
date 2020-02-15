using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject {
    public int par;
    public string title;
    public string creator;
    public Vector2Int boardSize;
    public Dictionary<BlockData, BlockState> blockDataDict;
    //Dictionary<EntityData, EntityState> entityDataDict;

    public static LevelData GenerateTestLevel() {
        LevelData testLevel = ScriptableObject.CreateInstance("LevelData") as LevelData;
        testLevel.par = 5;
        testLevel.title = "test level";
        testLevel.creator = "test";
        testLevel.blockDataDict = new Dictionary<BlockData, BlockState>();
        testLevel.boardSize = new Vector2Int(20,20);
        // AddTestBlock(4,1,0,0, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,2,2,1, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(3,3,4,4, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(3,1,3,3, BlockTypeEnum.FIXED, testLevel);
        // AddTestBlock(2,1,3,7, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(3,2,5,7, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,1,3,8, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(3,1,4,9, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,1,7,9, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,2,2,5, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,1,8,8, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(1,1,9,9, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,1,1,7, BlockTypeEnum.FREE, testLevel);
        // AddTestBlock(2,1,0,8, BlockTypeEnum.FIXED, testLevel);
        // AddTestBlock(1,1,2,4, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,3,1,1, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,1,4,1, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(2,1,4,2, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(1,19,0,0, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(19,1,0,19, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(1,19,19,1, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(19,1,1,0, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(6,1,1,12, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(3,2,11,10, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(5,1,14,7, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(2,3,10,16, BlockTypeEnum.FIXED, testLevel);
        AddTestBlock(3,1,6,13, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,1,7,14, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(2,3,15,1, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(4,1,14,4, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(2,1,13,5, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,1,14,6, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,1,15,5, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,1,11,4, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(5,1,8,12, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(2,1,7,11, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(1,1,7,10, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(1,2,7,8, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(3,1,6,7, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(2,2,5,8, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(4,1,5,15, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(1,1,5,14, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(1,2,4,10, BlockTypeEnum.FREE, testLevel);
        AddTestBlock(1,1,5,10, BlockTypeEnum.FREE, testLevel);
        return testLevel;
    }

    public static void AddTestBlock(int width, int height, int x, int y, BlockTypeEnum type, LevelData levelData) {
        BlockData testBlockData = ScriptableObject.CreateInstance("BlockData") as BlockData;
        testBlockData.Init(width, height, type);
        BlockState testBlockState = new BlockState(x, y);
        levelData.blockDataDict.Add(testBlockData, testBlockState);
    }
}
