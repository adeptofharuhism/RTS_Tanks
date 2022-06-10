using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TeamColorSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];

    [SyncVar(hook=nameof(HandleTeamColorUpdated))]
    private Color _teamColor = new Color();

    #region Server
    public override void OnStartServer() {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        _teamColor = player.TeamColor;
    }
    #endregion

    #region Client
    private void HandleTeamColorUpdated(Color oldColor, Color newColor) {
        foreach(var renderer in colorRenderers) {
            renderer.material.SetColor("_Color", newColor);
        }
    }
    #endregion
}
