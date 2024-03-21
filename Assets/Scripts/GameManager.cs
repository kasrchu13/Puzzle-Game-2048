using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public event Action<GameState> OnStateChanged;


    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private List<BlockType> _blockTypes;
    private List<Node> _nodes;
    private List<Block> _blocks;


    private GameState _state;
    private int _moves;

    void Awake()
    {
        if(Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }


    void Start()
    {
        ChangeState(GameState.LevelGeneration);
    }

    #region Initialize Function
    void PlayBoardSetup()
    {
        _nodes = new List<Node>();
        _blocks = new List<Block>();
        _moves = 0;
            
        #region Node Generation
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(i,j), Quaternion.identity);
                _nodes.Add(node);
            }
        }
        #endregion

        #region Board Generation
        var center = new Vector2(_width / 2 - 0.5f, _height / 2 - 0.5f);
        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        board.size = new Vector2(_width + 0.1f, _height + 0.1f);
        #endregion

        #region Adjust Camera
        Camera.main.transform.position = new Vector3(center.x, center.y, -5);
        #endregion

        ChangeState(GameState.BlockSpawning);
    }
    #endregion

    #region Block Spawn Function
    void SpawnBlocks(int amount)
    {
        //Get the List<Node> where it is not yet occupied, also randomize the 
        var freeNodes = _nodes.Where(n => n.Occupied == null).OrderBy(b => Random.value).ToList();

        //Take an amount of node from freeNodes list and place block on it 
        foreach(var node in freeNodes.Take(amount))
        {
            var block = Instantiate(_blockPrefab, node.Center, Quaternion.identity);
            block.AssignType(GetBLockTypeByValue(Random.value > 0.8f ? 4 : 2));
            node.Occupied = block;
        }

        ChangeState(GameState.WaitForInput);
    }

    //Assign block information according to value from _blockTypes List
    private BlockType GetBLockTypeByValue(int value)
    {
        return _blockTypes.First(b => b.Value == value);
    }
    #endregion

    public void ChangeState(GameState newState){
        _state = newState;

        switch (newState)
        {
            case GameState.LevelGeneration:
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
                break;
            case GameState.Lose:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
        OnStateChanged?.Invoke(newState);
    }

}
public enum GameState{
LevelGeneration,
BlockSpawning,
WaitForInput,
InputEnter,
Victory,
Lose

}