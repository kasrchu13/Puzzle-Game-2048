using System.Linq;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using System.Runtime.InteropServices.WindowsRuntime;

public class Block : MonoBehaviour
{
    public int Value;
    public Vector2 Pos{get {return transform.position;}}
    public Node LocatedNode;
    public Block blockToMerge;
    public bool UnderGoMerging;
    [SerializeField]private SpriteRenderer _renderer;
    [SerializeField]private TextMeshPro _text;
    public void AssignType(BlockType blockType)
    {
        Value = blockType.Value;
        _renderer.color = blockType.BlockColor;
        _text.text = blockType.Value.ToString();
    }
    public void SetToNewNode(Node newNode)
    {
        //free up the original node and set new node occupied
        if(LocatedNode != null) LocatedNode.OccupiedBlock = null;
        LocatedNode = newNode;
        LocatedNode.OccupiedBlock = this;
    }
    public void MergeToOther(Block otherBlock)
    {
        //set the target merging block
        blockToMerge = otherBlock;
        //free up the node currently siting on 
        LocatedNode.OccupiedBlock = null;
        //guard the target merging block so that other block can not access the same block
        otherBlock.UnderGoMerging = true;
    }
    public bool CanMerge(int value) => value == Value && blockToMerge == null && !UnderGoMerging;
    public bool MergableNearby()
    {
        var neighbour = new List<Node>
        {
            GameManager.Instance.GetNodeByPos(LocatedNode.Pos + Vector2.right),
            GameManager.Instance.GetNodeByPos(LocatedNode.Pos + Vector2.left),
            GameManager.Instance.GetNodeByPos(LocatedNode.Pos + Vector2.up),
            GameManager.Instance.GetNodeByPos(LocatedNode.Pos + Vector2.down)
        };
        var validBlock = neighbour.Where(n => n != null).FirstOrDefault(n => n.OccupiedBlock.Value == Value);
        return validBlock != null ? true : false; 
    }
}