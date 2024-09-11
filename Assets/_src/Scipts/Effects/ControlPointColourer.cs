using System.Collections;
using UnityEngine;
using Mirror;

public class ControlPointColourer : NetworkBehaviour
{
    private const string BaseColorMaterialParameter = "_BaseColor";

    [SerializeField] private Color _neutralColor = new Color();
    [SerializeField] private Light _controlPointLight = null;
    [SerializeField] private Renderer _controlPointRenderer = null;
    [SerializeField] private Renderer _controlPointMinimapIconRenderer = null;
    [SerializeField] private Renderer[] _territoryRenderers = new Renderer[0];
    [SerializeField] private ControlPoint _controlPoint = null;

    [SyncVar(hook = nameof(ClientHandleCurrentColorChange))]
    private Color _currentColor;

    #region Server
    public override void OnStartServer() {
        _currentColor = _neutralColor;

        _controlPoint.ServerOnPointColorsChanged += ServerHandleCurrentColorChange;
    }

    public override void OnStopServer() {
        _controlPoint.ServerOnPointColorsChanged -= ServerHandleCurrentColorChange;
    }

    [Server]
    private void ServerHandleCurrentColorChange(RTSPlayer owner) {
        if (owner==null) {
            _currentColor = _neutralColor;
        } else {
            _currentColor = owner.TeamColor;
        }
    }
    #endregion

    #region Client
    private void ClientHandleCurrentColorChange(Color oldValue, Color newValue) {
        _controlPointLight.color = newValue;
        _controlPointRenderer.material.SetColor(BaseColorMaterialParameter, newValue);
        _controlPointMinimapIconRenderer.material.SetColor(BaseColorMaterialParameter, newValue);

        foreach (var renderer in _territoryRenderers) {
            renderer.material.SetColor(BaseColorMaterialParameter, newValue);
        }
    }
    #endregion
}