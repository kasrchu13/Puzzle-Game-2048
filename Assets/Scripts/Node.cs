using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector2 Pos{get {return transform.position;}}
    public Block OccupiedBlock;
}
