using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using Random = UnityEngine.Random;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event Action<GameState> OnStateChanged;

    //References
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private List<BlockType> _blockTypes;
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private GameObject _loseScreen;
    [SerializeField] private GameObject _selectScreen;
    [SerializeField] private GameObject _inGameUI;
    
    //Getter Setter
    public List<BlockType> BlockTypes{get {return _blockTypes;}}

    //Animation parameters
    [SerializeField] private float _travelTime = 0.2f;
    [SerializeField] private float _fadeTime = 0.2f;

    //Data parameters    
    public int Objective;
    private List<Node> _nodes;
    private List<int> _blocksPlacementNew;
    private List<int> _blocksPlacementOld;
    private List<Block> _blocks;
    public GameState State;
    private int _moves;


    void Awake()
    {
        if(Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }


    void Start()
    {
        ChangeState(GameState.SelectObjective);
    }

    void Update()
    {
        if(State == GameState.WaitForInput) GetInput();
    }

    #region Initialize Function
    void PlayBoardSetup()
    {
        _nodes = new List<Node>();
        _blocks = new List<Block>();
        _blocksPlacementNew = new List<int>();
        _blocksPlacementOld = new List<int>();

        _moves = 0;
            
        //Node Generation
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(i,j), Quaternion.identity);
                _nodes.Add(node);
                _blocksPlacementNew.Add(0);
                _blocksPlacementOld.Add(0);
            }
        }

        //Board Generation
        var center = new Vector2(_width / 2 - 0.5f, _height / 2 - 0.5f);
        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(_width + 0.1f, _height + 0.1f);
        

        //Adjust Camera
        Camera.main.transform.position = new Vector3(center.x, center.y, -5);
        

        ChangeState(GameState.BlockSpawning);
    }
    #endregion

    #region Block Spawn Function
    void SpawnBlocks(int amount)
    {
        

        //Get the List<Node> where it is not yet occupied, also randomize it
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        //Take an amount of node from freeNodes list and place block on it 
        foreach(var node in freeNodes.Take(amount))
        {
            SpawningFunc(node, Random.value > 0.9f ? 4 : 2);
        }

        //Lose condition check
        if(freeNodes.Count() == 1)
        {
            var availableMove = _blocks.FirstOrDefault(b => b.MergableNearby() == true);
            if(availableMove == null) ChangeState(GameState.Lose);
        }

        ChangeState(_blocks.Any(b => b.Value == Objective)? GameState.Victory : GameState.WaitForInput);
        
    }

    private void SpawningFunc(Node node, int value)
    {
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
         block.AssignType(GetBlockTypeByValue(value));
         FadeInAnimation(block);
         node.OccupiedBlock = block;
         block.LocatedNode = node;
         _blocks.Add(block);
    }

    //Assign block information according to value from _blockTypes List
    private BlockType GetBlockTypeByValue(int value)
    {
        return _blockTypes.First(b => b.Value == value);
    }
    #endregion

    
    
    void GetInput(){
        if(Input.GetKeyDown(KeyCode.W)) Shift(Vector2.up);
        if(Input.GetKeyDown(KeyCode.A)) Shift(Vector2.left);
        if(Input.GetKeyDown(KeyCode.S)) Shift(Vector2.down);
        if(Input.GetKeyDown(KeyCode.D)) Shift(Vector2.right);
    }
    #region Gameplay mechanics
    void Shift(Vector2 dir)
    {
        ChangeState(GameState.InputEnter);

        GetBlockPlacement(_nodes, _blocksPlacementOld);

        //order the existed block list, ensuring the iteration order is correct
        var orderedBlock = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if(dir == Vector2.right || dir == Vector2.up) orderedBlock.Reverse();

        //moving the block contains 2 part : Data assigning and Visual animation

        //Data assigning part
        foreach(var b in orderedBlock)
        {
            MovingBlock(b, dir);
        }

        //Animation part
        DisplayAnimation(orderedBlock);

        //Get the current block placement
        GetBlockPlacement(_nodes, _blocksPlacementNew);

        //Validity check for new block to spawn
        var valid = ValidMoveCheck(_blocksPlacementNew, _blocksPlacementOld);
        if(valid){
            _moves ++;
            ChangeState( GameState.BlockSpawning);

        }else
        {
            ChangeState(GameState.WaitForInput);
        }
        
    }

    void MovingBlock(Block block, Vector2 direction)
    {
        var nextNode = block.LocatedNode;

        //data assigning part
        do{
            //start searching next node to move from self
            block.SetToNewNode(nextNode);

            var availableNode = GetNodeByPos(block.LocatedNode.Pos + direction);
            //The Node is inside boundary
            if(availableNode != null) 
            {
                
                //The node is occupied with same value and it is not formed in this turn(!Merging)
                if(availableNode.OccupiedBlock != null && availableNode.OccupiedBlock.CanMerge(block.Value))
                {
                    block.MergeToOther(availableNode.OccupiedBlock);
                }
                //The node is not occupied
                else if(availableNode.OccupiedBlock == null) nextNode = availableNode;
            }

        //keep searching if found nextNode
        }while(nextNode != block.LocatedNode);
    }

    void GetBlockPlacement(List<Node> nodes, List<int> container)
    {
        for (int i = 0; i < nodes.Count(); i++)
        {
                int value;
                var block = nodes[i].OccupiedBlock;
                if(block == null)
                {
                    value = 0;
                }else{
                    value = block.Value;
                }
                container[i] = value;
        }
    }

    bool ValidMoveCheck(List<int> currentBlocks, List<int> previousBlocks)
    {
        bool isDiff = false;
        for (int i = 0; i < currentBlocks.Count(); i++)
        {
            var foundDiff = currentBlocks[i] != previousBlocks[i]? true : false;
            if(foundDiff)
            {
                isDiff = foundDiff;
                break;
            }
        }
        return isDiff;
    }

    public Node GetNodeByPos(Vector2 pos)
    {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

    

    void MergeAllBlocks(Block movingBlock, Block baseBlock)
    {
        //remove both blocks then instantiate upgraded block
        RemoveBlock(movingBlock);
        RemoveBlock(baseBlock);

        var newValue = baseBlock.Value * 2;
        SpawningFunc(GetNodeByPos(baseBlock.Pos), newValue);

        
    }

    void RemoveBlock(Block block)
    {
        //free up the corresponding node 
        block.LocatedNode.OccupiedBlock = null;
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }
    #endregion


    #region Animation
    void FadeInAnimation(Block block)
    {
        block.transform.localScale = Vector2.zero;
        block.transform.DOScale(_blockPrefab.transform.localScale, _fadeTime);
    }

    void DisplayAnimation(List<Block> blocksList)
    {
        var sequence = DOTween.Sequence();
        foreach(var b in blocksList)
        {
            //if the block has a target block to merge, go to the target block node position
            var moveToPoint = b.blockToMerge == null? b.LocatedNode.Pos : b.blockToMerge.LocatedNode.Pos;
            sequence.Insert(0, b.transform.DOMove(moveToPoint, _travelTime));
        }

        sequence.OnComplete(() => {
            //finding all block that has a target block
            foreach(var b in blocksList.Where(b => b.blockToMerge != null))
            {
                MergeAllBlocks(b, b.blockToMerge);
            }
        }
        );
        
    }
    #endregion
    #region GameState Manager
    public void ChangeState(GameState newState)
    {
        State = newState;

        switch (newState)
        {
            case GameState.SelectObjective:
                _selectScreen.SetActive(true);
                break;
            case GameState.LevelGeneration:
                _inGameUI.SetActive(true);
                PlayBoardSetup();
                break;
            case GameState.BlockSpawning:
                SpawnBlocks(_moves == 0? 2 : 1);
                break;
            case GameState.WaitForInput:
                break;
            case GameState.InputEnter:
                break;
            case GameState.Victory:
                _victoryScreen.SetActive(true);
                break;
            case GameState.Lose:
                _loseScreen.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        OnStateChanged?.Invoke(newState);
    }
    #endregion

}

public enum GameState
{
SelectObjective,
LevelGeneration,
BlockSpawning,
WaitForInput,
InputEnter,
Victory,
Lose
}