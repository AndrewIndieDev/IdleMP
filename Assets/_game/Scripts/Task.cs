using System;

public struct Task
{
    public string Name;
    public float Progress;
    public float TimeToComplete;
    public Action OnComplete;

    public Task(string name, float timeToComplete, Action onComplete, float progress = 0f)
    {
        Name = name;
        Progress = progress;
        TimeToComplete = timeToComplete;
        OnComplete = onComplete;
    }
}
