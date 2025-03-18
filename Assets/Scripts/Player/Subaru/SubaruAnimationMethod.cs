using UnityEngine;
using Mirror;
using HR.Object.Skill;

namespace HR.Object.Player{

public class SubaruAnimationMethod : NetworkBehaviour
{
    public Transform Target;
    [SerializeField] GameObject Ball_In_GameObject;
    [SerializeField] Baseball Ball_Prefab;

    public void Show_Ball()
    {
        Ball_In_GameObject.SetActive(true);
    }
    public void Hide_Ball()
    {
        Ball_In_GameObject.SetActive(false);
    }
    public void Spawn_Ball()
    {
        if (!isOwned) return;
        CmdSpawn_Ball();
    }
    [Command]
    void CmdSpawn_Ball()
    {
        Baseball ball = Instantiate(Ball_Prefab,Ball_In_GameObject.transform.position,Quaternion.identity);
        ball.Target = Target;
        ball.BallOwner = transform.root.GetComponent<CharacterBase>();
        ball.Set_AttackDamage();
        NetworkServer.Spawn(ball.gameObject);
    }
}

}