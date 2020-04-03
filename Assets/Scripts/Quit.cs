using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quit : MonoBehaviour
{
    public void doExitGame() {
        print("quitted game");
        Application.Quit();
    }
}
