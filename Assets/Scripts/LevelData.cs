using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject {
    public int par;
    public string title;
    public string creator;
    public Vector2Int boardSize;
    public List<BlockData> blockDataList;
    public List<MobData> mobDataList;

    public static LevelData GenerateTestLevel() {
        LevelData testLevel = ScriptableObject.CreateInstance("LevelData") as LevelData;
        testLevel.par = 5;
        testLevel.title = "test level";
        testLevel.creator = "test";
        testLevel.boardSize = new Vector2Int(20,20);
        testLevel.blockDataList = new List<BlockData>();
        testLevel.mobDataList = new List<MobData>();

        AddBlock(3,3,1,1, BlockTypeEnum.FREE, testLevel);
        AddBlock(3,1,4,1, BlockTypeEnum.FREE, testLevel);
        AddBlock(2,1,4,2, BlockTypeEnum.FREE, testLevel);
        AddBlock(1,19,0,0, BlockTypeEnum.FIXED, testLevel);
        AddBlock(19,1,0,19, BlockTypeEnum.FIXED, testLevel);
        AddBlock(1,19,19,1, BlockTypeEnum.FIXED, testLevel);
        AddBlock(19,1,1,0, BlockTypeEnum.FIXED, testLevel);
        AddBlock(6,1,1,12, BlockTypeEnum.FIXED, testLevel);
        AddBlock(3,2,11,10, BlockTypeEnum.FIXED, testLevel);
        AddBlock(5,1,14,7, BlockTypeEnum.FIXED, testLevel);
        AddBlock(2,3,10,16, BlockTypeEnum.FIXED, testLevel);
        AddBlock(3,1,6,13, BlockTypeEnum.FREE, testLevel);
        AddBlock(3,1,7,14, BlockTypeEnum.FREE, testLevel);
        AddBlock(2,3,15,1, BlockTypeEnum.FREE, testLevel);
        AddBlock(4,1,14,4, BlockTypeEnum.FREE, testLevel);
        AddBlock(2,1,13,5, BlockTypeEnum.FREE, testLevel);
        AddBlock(3,1,14,6, BlockTypeEnum.FREE, testLevel);
        AddBlock(3,1,15,5, BlockTypeEnum.FREE, testLevel);
        AddBlock(3,1,11,4, BlockTypeEnum.FREE, testLevel);
        AddBlock(5,1,8,12, BlockTypeEnum.FREE, testLevel);
        AddBlock(2,1,7,11, BlockTypeEnum.FREE, testLevel);
        AddBlock(1,1,7,10, BlockTypeEnum.FREE, testLevel);
        AddBlock(1,2,7,8, BlockTypeEnum.FREE, testLevel);
        AddBlock(3,1,6,7, BlockTypeEnum.FREE, testLevel);
        AddBlock(2,2,5,8, BlockTypeEnum.FREE, testLevel);
        AddBlock(4,1,5,15, BlockTypeEnum.FREE, testLevel);
        AddBlock(1,1,5,14, BlockTypeEnum.FREE, testLevel);
        AddBlock(1,2,4,10, BlockTypeEnum.FREE, testLevel);
        AddBlock(1,1,5,10, BlockTypeEnum.FREE, testLevel);
        AddPlayer(1, 16, testLevel);

        return testLevel;
    }

    public static void AddBlock(int aWidth, int aHeight, int aX, int aY, BlockTypeEnum aType, LevelData aLevelData) {
        BlockData testBlockData = ScriptableObject.CreateInstance("BlockData") as BlockData;
        Color newColor = RandomColor();
        testBlockData.Init(new Vector2Int(aWidth, aHeight), new Vector2Int(aX, aY), newColor, aType);
        aLevelData.blockDataList.Add(testBlockData);
    }

    public static void AddPlayer(int aX, int aY, LevelData aLevelData) {
        Vector2Int startingPos = new Vector2Int(aX, aY);
        MobData playerData = ScriptableObject.CreateInstance("MobData") as MobData;
        Vector2Int playerSize = new Vector2Int(2, 3);
        playerData.Init(playerSize, startingPos, Vector2Int.right, "Player");
        aLevelData.mobDataList.Add(playerData);
    }
    public static Color RandomColor() {
        return UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }
}
