using HR.Object.Skill;
using UnityEngine;

namespace HR.Object.Minion{
public class MinionAnimationMethod : MonoBehaviour
{
    public Transform Target;
    [SerializeField] GameObject Ball_In_GameObject;
    [SerializeField] TowerBall Attack_Ball;
    public void Spawn_Ball()
    {
        TowerBall ball = Instantiate(Attack_Ball,Ball_In_GameObject.transform.position,Quaternion.identity);
        ball.Target = Target;
    }
}

}