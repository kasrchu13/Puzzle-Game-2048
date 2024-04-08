using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Selection : MonoBehaviour
{
    [SerializeField]private Vector2 _displayPos;
    [SerializeField]private Block _blockPrefab;
    [SerializeField]private GameObject _demoHolder;
    private Block _demoBlock;
    private int _index = 0;
    private int[] _validTarScore = new int[] {16,32,64,128,256,512,1024,2048};
    void Start()
    {
        ShowBlock(_validTarScore[_index]);
    }

    //use _demoHolder to hold the instantiated object(create the reference), allowed deletion upon game start 
    private void ShowBlock(int value){
        if(_demoBlock == null)
        {
            _demoBlock = Instantiate(_blockPrefab, _displayPos, Quaternion.identity, _demoHolder.transform);
            _demoBlock.AssignType(GetBlockTypeByValue(value));
        }else
        {
            _demoBlock.AssignType(GetBlockTypeByValue(value));
        }
        
    }

    private BlockType GetBlockTypeByValue(int value)
    {
        return GameManager.Instance.BlockTypes.First(b => b.Value == value);
    }

    public void SelectNext(){
        if(_index == _validTarScore.Length - 1)
        {
            _index = 0;
        }else
        {
            _index++;
        }
        ShowBlock(_validTarScore[_index]);
    }

    public void SelectPrevious(){
        if(_index == 0)
        {
            _index = _validTarScore.Length - 1;
        }else
        {
            _index--;
        }
        ShowBlock(_validTarScore[_index]);
    }

    public void ConfirmButton()
    {
        Destroy(_demoHolder);
        Destroy(_demoBlock);
        gameObject.SetActive(false);
        GameManager.Instance.Objective = _validTarScore[_index];
        GameManager.Instance.ChangeState(GameState.LevelGeneration);
    }
}