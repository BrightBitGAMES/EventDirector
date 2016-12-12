using UnityEngine;

public class Observer : MonoBehaviour
{
    [SerializeField] Observee observee;

    void Awake()
    {
        observee.Subscribe(this);
    }

    void OnEvent1() { Debug.Log("OnEvent1"); }

    // If you want to get notifications for more events of the observee, just uncomment the following lines.
    
    // void OnEvent2(int value)         { Debug.Log("OnEvent2"); }
    // void OnEvent3Changed(bool value) { Debug.Log("OnEvent3"); }
}
