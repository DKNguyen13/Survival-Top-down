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
        // Convert screen touch position to joystick local space
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out Vector2 local)) return;

        // Calculate input from the actual center of the joystick
        Vector2 direction = local - _rect.rect.center;
        Vector2 value = Vector2.ClampMagnitude(direction / _radius, 1f);

        // Move handle and send normalized input (-1 to 1)
        _handle.anchoredPosition = value * _radius;
        _input.SetJoystickInput(value);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _handle.anchoredPosition = Vector2.zero;
        _input.ReleaseJoystick();
    }
}