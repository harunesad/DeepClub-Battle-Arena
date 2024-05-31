using UnityEngine;
using Photon.Pun;
using TMPro;
public class CameraFollow : MonoBehaviour
{
    public Transform target, newTarget; // Oyuncu karakteri
    public Vector3 playerOffset, desiredPosition;
    public float smoothSpeed = 0.125f;
    public int playerIndex;
    void LateUpdate()
    {
        //if (ServerControl.server.cMFreeLook.activeSelf && ServerControl.server.cMFreeLook.GetComponent<CinemachineFreeLook>().m_XAxis.m_InputAxisValue == 0)
        //{
        //    float value = ServerControl.server.cMFreeLook.GetComponent<CinemachineFreeLook>().m_XAxis.Value;
        //    value = Mathf.Lerp(value, 0, Time.deltaTime * 25);
        //    ServerControl.server.cMFreeLook.GetComponent<CinemachineFreeLook>().m_XAxis.Value = value;
        //}
        TargetFollow();
        TargetChange();
    }
    void TargetFollow()
    {
        if (target != null)
        {
            desiredPosition = target.position + playerOffset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * 10);
            transform.position = smoothedPosition;
            transform.LookAt(target);
        }
    }
    void TargetChange()
    {
        if (target == null && GameObject.FindGameObjectsWithTag("Player").Length >= 1)
        {
            if (PhotonNetwork.PlayerListOthers[playerIndex] != null && GameObject.Find(PhotonNetwork.PlayerListOthers[playerIndex].NickName) != null)
            {
                target = GameObject.Find(PhotonNetwork.PlayerListOthers[playerIndex].NickName).transform;
                target.GetChild(0).localRotation = Quaternion.identity;
                UIManager.uIManager.playersCam.GetComponentInChildren<TextMeshProUGUI>().text = target.gameObject.GetPhotonView().Owner.NickName;
            }
            else
            {
                playerIndex++;
            }
        }
    }
}
