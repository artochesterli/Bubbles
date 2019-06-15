using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task
{
    public enum TaskState : byte
    {
        Pending, // Task has not been initialized
        Working, // Task has been initialized
        Success, // Task completed successfully
        Failed, // Task completed unsuccessfully
        Aborted // Task was aborted
    }

    public TaskState State = TaskState.Pending;

    // Convenience status checking
    public bool IsPending { get { return State == TaskState.Pending; } }
    public bool IsWorking { get { return State == TaskState.Working; } }
    public bool IsSuccessful { get { return State == TaskState.Success; } }
    public bool IsFailed { get { return State == TaskState.Failed; } }
    public bool IsAborted { get { return State == TaskState.Aborted; } }
    public bool IsFinished { get { return (State == TaskState.Failed || State == TaskState.Success || State == TaskState.Aborted); } }

    public void Abort()
    {
        SetState(TaskState.Aborted);
    }

    internal void SetState(TaskState newState)
    {
        if (State == newState) return;

        State = newState;

        switch (newState)
        {
            case TaskState.Working:
                Init();
                break;

            case TaskState.Success:
                OnSuccess();
                CleanUp();
                break;

            case TaskState.Aborted:
                OnAbort();
                CleanUp();
                break;

            case TaskState.Failed:
                OnFail();
                CleanUp();
                break;

            case TaskState.Pending:
                break;

            default:
                throw new ArgumentOutOfRangeException(newState.ToString(), newState, null);
        }
    }

    protected virtual void OnAbort() { }
    protected virtual void OnSuccess() { }
    protected virtual void OnFail() { }
    protected virtual void Init() { }
    protected virtual void CleanUp() { }

    internal virtual void Update() { }
}

public abstract class TaskRunner : Task
{
    protected readonly List<Task> Tasks = new List<Task>();

    private readonly List<Task> PendingAdd = new List<Task>();

    private readonly List<Task> PendingRemove = new List<Task>();

    public T GetTask<T>() where T : Task
    {
        foreach (Task task in Tasks)
        {
            if (task.GetType() == typeof(T)) return (T)task;
        }
        return null;
    }

    public bool HasTask<T>() where T : Task
    {
        return GetTask<T>() != null;
    }

    public void Add(Task task)
    {
        SetState(TaskState.Working);
        task.SetState(TaskState.Pending);
        PendingAdd.Add(task);
    }

    private void HandleCompletion(Task task)
    {
        PendingRemove.Add(task);
        task.SetState(TaskState.Pending);
        if (Tasks.Count == 0)
        {
            SetState(TaskState.Success);
        }
    }

    protected void PostUpdate()
    {
        
        foreach (Task task in PendingRemove)
        {
            Tasks.Remove(task);
        }
        PendingRemove.Clear();

        foreach (Task task in PendingAdd)
        {
            Tasks.Add(task);
        }
        PendingAdd.Clear();

        if (Tasks.Count == 0)
        {
            SetState(TaskState.Success);
        }

    }

    public void Clear()
    {
        foreach (Task t in Tasks)
        {
            t.Abort();
        }
    }

    protected void ProcessTask(Task task)
    {
        if (task.IsPending)
        {
            task.SetState(TaskState.Working);
        }

        if (task.IsFinished)
        {
            HandleCompletion(task);
        }
        else
        {
            task.Update();
            if (task.IsFinished)
            {
                HandleCompletion(task);
            }
        }
    }

    protected override void OnAbort()
    {
        foreach (Task task in Tasks)
        {
            task.Abort();
        }
    }
}

public class SerialTasks : TaskRunner
{
    internal override void Update()
    {
        if (Tasks.Count > 0)
        {
            Task first = Tasks[0];
            ProcessTask(first);
        }
        
        PostUpdate();
    }
}

public class ParallelTasks : TaskRunner
{
    internal override void Update()
    {
        foreach (Task task in Tasks)
        {
            ProcessTask(task);
        }
        PostUpdate();
    }
}
