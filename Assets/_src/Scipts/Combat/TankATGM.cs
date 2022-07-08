using UnityEngine;
using Mirror;

public class TankATGM : TankGun
{
    protected override void Fire() {
        GameObject projectileInstance =
            Instantiate(_projectilePrefab, _gunTransform.position, Quaternion.LookRotation(Vector3.up)/*_gunTransform.rotation*/);

        projectileInstance.GetComponent<UnitProjectileATGM>().SetTarget(_turret.CurrentTarget);

        NetworkServer.Spawn(projectileInstance, connectionToClient);
    }
}