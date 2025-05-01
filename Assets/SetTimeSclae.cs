using UnityEngine;

public class SetTimeSclae : MonoBehaviour
{
    [Range(0f, 1f)]
    public float timeScale = 1f;
    void Update()
    {
        Time.timeScale = timeScale;
    }
}
