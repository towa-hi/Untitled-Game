using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerComponentOrder", menuName = "SciptableObjects/ComponentOrder")]
public class PlayerComponentOrder : ScriptableObject {
    List<Type> typeList = new List<Type>();

}
