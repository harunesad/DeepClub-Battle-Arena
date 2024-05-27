using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI info;
    [SerializeField] List<string> infos;
    public void InfoText()
    {
        info.text = infos[Random.Range(0, infos.Count)];
    }
}
