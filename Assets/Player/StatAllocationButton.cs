using UnityEngine;

/// <summary>게임플레이 씬 HUD의 스탯창 버튼에 붙인다.</summary>
public class StatAllocationButton : MonoBehaviour
{
    public void OnClick()
    {
        var playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        PopupManager.Instance.Open(StatAllocationPopup.Instance, p => p.Setup(playerStats));
    }
}
