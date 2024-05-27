using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;

public class GrassInside : MonoBehaviour
{
    public bool inGrass;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 13 && transform.parent.gameObject.GetPhotonView().IsMine)
        {
            Color color = other.GetComponent<Renderer>().material.color;
            other.GetComponent<Renderer>().material.DOColor(new Color(color.r, color.g, color.b, .25f), .25f).SetEase(Ease.Linear);
            //other.GetComponent<Renderer>().material.color = new Color(0, 1, 0, .25f);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 12 && other.gameObject != transform.parent.gameObject && transform.parent.gameObject.GetPhotonView().IsMine)
        {
            for (int i = 0; i < other.GetComponent<Player>().meshes.Count; i++)
            {
                //Color color = other.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].color;
                //other.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].DOColor(new Color(color.r, color.g, color.b, 1), .25f).SetEase(Ease.Linear);
                other.GetComponent<Player>().meshes[i].SetActive(true);
            }
            other.GetComponent<Player>().canvas.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 13 && transform.parent.gameObject.GetPhotonView().IsMine)
        {
            Color color = other.GetComponent<Renderer>().material.color;
            other.GetComponent<Renderer>().material.DOColor(new Color(color.r, color.g, color.b, 1), .25f).SetEase(Ease.Linear);
            //other.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 1);
        }
        if (other.gameObject.layer == 12 && other.gameObject != transform.parent.gameObject && other.GetComponent<Player>().inGrass && transform.parent.gameObject.GetPhotonView().IsMine)
        {
            for (int i = 0; i < other.GetComponent<Player>().meshes.Count; i++)
            {
                //Color color = other.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].color;
                //other.GetComponent<Avatar>().meshes[i].GetComponent<SkinnedMeshRenderer>().materials[0].DOColor(new Color(color.r, color.g, color.b, 0), .25f).SetEase(Ease.Linear);
                other.GetComponent<Player>().meshes[i].SetActive(false);
            }
            other.GetComponent<Player>().canvas.SetActive(false);
        }
    }
}
