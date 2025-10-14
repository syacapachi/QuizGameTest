using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> mainThreadActions = new Queue<Action>();
    public static SynchronizationContext _mainContext;
    // メインスレッドに処理を戻したい場合
    //_mainContext.Post(_ => SomethingNextMethod("Fuga"), null);

    private void Start()
    {
        _mainContext = SynchronizationContext.Current;
    }
    void Update()
    {
        while (mainThreadActions.Count > 0)
        {
            var action = mainThreadActions.Dequeue();
            action?.Invoke();
        }
    }

    public static Task RunOnMainThreadAsync(Action action)
    {
        var tcs = new TaskCompletionSource<bool>();
        mainThreadActions.Enqueue(() =>
        {
            try
            {
                action();
                tcs.SetResult(true);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        return tcs.Task;
    }
}
