using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    public Vector3 tilePos;
    public BlockData block;

    // Start is called before the first frame update
    void Start() {
        tilePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
