using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    [SerializeField]private SpriteRenderer _renderer;
    [SerializeField]private TextMeshPro _text;
    public void AssignType(BlockType blockType)
    {
        Value = blockType.Value;
        _renderer.color = blockType.BlockColor;
        _text.text = blockType.Value.ToString();
    }
}