using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}