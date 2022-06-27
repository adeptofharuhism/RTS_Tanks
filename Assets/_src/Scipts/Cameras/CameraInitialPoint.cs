using System;
using UnityEngine;
using Mirror;

public class CameraInitialPoint : NetworkBehaviour
{
    public static event Action<Vector3> OnInitialPointReady;

    public override void OnStartAuthority() {
        OnInitialPointReady?.Invoke(transform.position);
    }
}