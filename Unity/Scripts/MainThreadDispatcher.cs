using System;
using System.Collections.Generic;
using UnityEngine;

/// Ensures that actions from background threads (e.g., network threads) are safely executed on Unity's main thread.
/// Unity APIs are not thread-safe, so this is crucial for UI updates, transforms, etc.
public class MainThreadDispatcher : MonoBehaviour
{
    // Thread-safe queue of actions to run on the main thread
    private static readonly Queue<Action> actions = new();

    /// Enqueues an action to be executed on the main thread.
    /// Safe to call from any background thread.
    public static void Run(Action action)
    {
        lock (actions)
        {
            actions.Enqueue(action);
        }
    }

    /// Executes all queued actions in the Unity main thread context.
    /// Called automatically by Unity once per frame.
    void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue()?.Invoke();
            }
        }
    }
}
