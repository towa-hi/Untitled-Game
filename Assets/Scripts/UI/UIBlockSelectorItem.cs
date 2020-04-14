using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIBlockSelectorItem : EventTrigger {
    public BlockData blockData;
    public GameObject blockSelectorItemImage;
    public GameObject blockSelectorItemText;
    public Button button;
    public bool selected;
    public bool clicked;

    public void Init(BlockData aBlockData) {
        this.blockData = aBlockData;
        this.blockSelectorItemText.GetComponent<TMP_Text>().text = this.blockData.size.ToString() + " Block";
        this.selected = false;
        this.clicked = false;
    }

    public override void OnBeginDrag(PointerEventData data) {
        print("this button clicked" + data);
        
    }

    public override void OnCancel(BaseEventData data)
    {
        Debug.Log("OnCancel called.");
    }

    public override void OnDeselect(BaseEventData data)
    {
        Debug.Log("OnDeselect called.");
    }

    public override void OnDrag(PointerEventData data)
    {
        Debug.Log("OnDrag called.");
    }

    public override void OnDrop(PointerEventData data)
    {
        Debug.Log("OnDrop called.");
    }

    public override void OnPointerDown(PointerEventData data)
    {
        this.clicked = true;
        if (this.clicked) {
            this.selected = true;
            EditorManager.Instance.BlockSelectorItemOnPointerDown(this.blockData);
        }
    }

    public override void OnPointerUp(PointerEventData data)
    {
       EditorManager.Instance.BlockSelectorItemOnPointerUp();
    }

}
