using Mirror;
using UnityEngine;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];

    [SyncVar(hook=nameof(HandleTeamColorUpdated))]
    private Color _teamColor = new Color();

    public Color TeamColor => _teamColor;

    #region Server
    public override void OnStartServer() {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        _teamColor = player.TeamColor;
    }
    #endregion

    #region Client
    private void HandleTeamColorUpdated(Color oldColor, Color newColor) {
        foreach(var renderer in colorRenderers) {
            renderer.material.SetColor("_GlowColor", newColor*2);
        }
    }
    #endregion
}
