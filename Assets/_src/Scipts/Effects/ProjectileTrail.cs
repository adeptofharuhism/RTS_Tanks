using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTrail : MonoBehaviour
{
    [SerializeField] private GameObject _trailPrefab;
    [SerializeField] private TeamColorSetter _teamColorSetter;

    private Transform _trailTransform;

    private void Start() {
        GameObject instantiatedTrail = Instantiate(_trailPrefab, transform.position, Quaternion.identity);

        _trailTransform = instantiatedTrail.transform;

        TrailRenderer trailRenderer = instantiatedTrail.GetComponent<TrailRenderer>();

        trailRenderer.startColor = _teamColorSetter.TeamColor;
        trailRenderer.endColor = _teamColorSetter.TeamColor;

        trailRenderer.startWidth = transform.localScale.x;
    }

    private void Update() {
        _trailTransform.position = transform.position;
    }
}
