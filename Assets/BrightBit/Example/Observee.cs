using BrightBit.EventSystem;

public class Observee : EventDirector
{
    public Message      Event1;
    public Message<int> Event2;
    public Value<bool>  Event3;

    void Start()
    {
        Event1.Send();
        Event2.Send(5);
        Event3.Set(true);
    }
}
