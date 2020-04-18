using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGridCell : MonoBehaviour {
    public bool active;
    public Color color;
    // Start is called before the first frame update
    void Awake() {
        this.active = false;
        this.color = Color.white;
        GetComponent<Renderer>().enabled = false;
    }

    public void SetColor(Color aColor) {
        this.color = aColor;
        GetComponent<Renderer>().material.color = aColor;
    }

    public void SetActive(bool aActive) {
        this.active = aActive;
        if (aActive) {
            GetComponent<Renderer>().enabled = true;
        } else {
            GetComponent<Renderer>().enabled = false;
        }
    }
}
