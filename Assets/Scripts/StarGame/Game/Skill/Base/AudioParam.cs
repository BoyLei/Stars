
using SkillEditor;

public interface I_AudioParam
{
    float DelayTime { get; set; }
    string SoundName { get; set; }
    uint SoundEventID { get; set; }
    int DurningTime { get; set; }

    int StartTime { get; set; }
    void Reset();
}

public class AudioParam : I_AudioParam
{
    public float DelayTime { get; set; }
    public string SoundName { get; set; }
    public uint SoundEventID { get; set; }

    public int DurningTime { get; set; }

    public int StartTime { get; set; }

    public void Init(SoundJson soundJson, int startTime, float delayTime)
    {
        Reset();
        // delayTime = soundJson.
        SoundName = soundJson.EventName;
        DelayTime = delayTime;
        DurningTime = soundJson.Duration;
        StartTime = startTime;
        SoundEventID = soundJson.SoundEventID;
    }

    public void Reset()
    {
        DelayTime = 0;
        SoundName = "";
        DurningTime = 0;
        StartTime = 0;
    }
}