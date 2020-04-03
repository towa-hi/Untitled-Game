using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBlockSelectorItem : MonoBehaviour {
    public BlockData blockData;
    public GameObject blockSelectorItemImage;
    public GameObject blockSelectorItemText;
    public Button button;

    public void Init(BlockData aBlockData) {
        this.blockData = aBlockData;
        this.blockSelectorItemText.GetComponent<TMP_Text>().text = this.blockData.size.ToString() + " Block";
    }
    
}
