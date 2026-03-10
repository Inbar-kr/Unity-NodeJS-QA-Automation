using UnityEngine;
using System.Collections.Generic;
using System;

/**
 * Ensures that actions triggered from background threads 
 * (like WebSocket events) are executed on Unity's Main Thread.
 * This is necessary because Unity's API is not thread-safe.
 */
public class UnityMainThreadDispatcher : MonoBehaviour {
    
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    
    /**
     * Update is called every frame by Unity on the Main Thread.
     * It checks if there are any pending actions in the queue and executes them.
     */
    public void Update() {
        lock (_executionQueue) {
            while (_executionQueue.Count > 0) 
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
    
    /**
     * Adds an Action to the execution queue.
     * This can be called safely from any background thread.
     * @param action - The function expression to be executed.
     */
    public void Enqueue(Action action) {
        lock (_executionQueue) 
        {
            _executionQueue.Enqueue(action);
        }
    }
    
    // --- Singleton Implementation ---

    private static UnityMainThreadDispatcher _instance = null;

    /**
     * Provides global access to the dispatcher instance.
     * @returns The active UnityMainThreadDispatcher instance.
     */
    public static UnityMainThreadDispatcher Instance() { 
        return _instance; 
    }
    
    /**
     * Awake is called when the script instance is being loaded.
     * Ensures only one instance exists (Singleton) and persists across scenes.
     */
    void Awake() {
        if (_instance == null) 
        { 
            _instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
    }
}