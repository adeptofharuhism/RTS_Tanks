using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelectionBlocker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action OnUnitSelectionBlocked;
    public static event Action OnUnitSelectionAllowed;

    public void OnPointerEnter(PointerEventData eventData) {
        OnUnitSelectionBlocked?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnUnitSelectionAllowed?.Invoke();
    }
}