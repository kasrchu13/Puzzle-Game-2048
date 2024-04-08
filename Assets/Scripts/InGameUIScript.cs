using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InGameUIScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _TarScore;
    void Update()
    {
        _TarScore.text = "Target Score: " + GameManager.Instance.Objective.ToString();
    }

}
