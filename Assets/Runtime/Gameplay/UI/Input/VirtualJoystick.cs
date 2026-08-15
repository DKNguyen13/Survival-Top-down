using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform _handle;
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private float _radius = 70f;
    private RectTransform _rect;

    private void Awake() => _rect = (RectTransform)transform;

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        // Convert screen touch/mouse position to the joystick's local UI position
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out Vector2 local)) return;
        
        // Normalize input and keep the handle inside the joystick radius
        Vector2 value = Vector2.ClampMagnitude(local / _radius, 1f);
        _handle.anchoredPosition = value * _radius;
        _input.SetJoystickInput(value);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _handle.anchoredPosition = Vector2.zero;
        _input.ReleaseJoystick();
    }
}