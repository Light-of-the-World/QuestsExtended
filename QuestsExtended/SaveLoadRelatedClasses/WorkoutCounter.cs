using UnityEngine;

public class WorkoutCounter : MonoBehaviour
{
    public int counter;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
