using Mirror;

public class UnitControlPointConqueror : NetworkBehaviour
{
    private RTSPlayer _player;

    public RTSPlayer Player => _player;

    public override void OnStartServer() {
        _player = connectionToClient.identity.GetComponent<RTSPlayer>();
    }
}