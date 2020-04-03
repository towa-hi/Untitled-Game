using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateEditorPalette : MonoBehaviour
{
    public GameObject prefab;
    public int numberToCreate;

    void Start() {
        Populate();
    }

    void Populate() {
        GameObject newObj;
        for (int i = 0; i < numberToCreate; i++) {
            newObj = (GameObject)Instantiate(prefab, transform);
        }
    }
}
