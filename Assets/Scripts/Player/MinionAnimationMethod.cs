using HR.Object.Skill;
using UnityEngine;
using Mirror;

namespace HR.Object.Minion{
public class MinionAnimationMethod : NetworkBehaviour
{
    public Transform Target;
    [SerializeField] GameObject Ball_In_GameObject;
    [SerializeField] TowerBall Attack_Ball;
    public void Spawn_Ball()
    {
        if (!NetworkServer.active) return;
        CmdSpawn_Ball();
    }
    [Server]
    void CmdSpawn_Ball()
    {
        TowerBall ball = Instantiate(Attack_Ball,Ball_In_GameObject.transform.position,Quaternion.identity);
        ball.Target = Target;
        NetworkServer.Spawn(ball.gameObject);
    }
}

}