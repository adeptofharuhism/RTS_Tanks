using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10;

    #region Server
    public override void OnStartServer() {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer() {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    [ServerCallback]
    private void Update() {
        Targetable target = targeter.Target;

        if (target != null) {
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange) {
                agent.SetDestination(target.transform.position);
            } else if (agent.hasPath){
                agent.ResetPath();
            }

            return;
        }

        if (!agent.hasPath)
            return;

        if (agent.remainingDistance > agent.stoppingDistance)
            return;

        agent.ResetPath();
    }

    [Command]
    public void CmdMove(Vector3 position) {
        ServerMove(position);
    }

    [Server]
    public void ServerMove(Vector3 position) {
        targeter.ClearTarget();

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(position, out hit, 1, NavMesh.AllAreas))
            return;

        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver() {
        agent.ResetPath();
    }
    #endregion
}
