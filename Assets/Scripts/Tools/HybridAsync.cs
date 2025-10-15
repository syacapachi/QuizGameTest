using System.Threading.Tasks;
using System;
using UnityEngine;
using System.Collections;

public class HybrittAsync : MonoBehaviour
{
    public static class HybridAsync
    {
        public static Task Run(MonoBehaviour context, Func<Task> asyncFunc, Func<IEnumerator> coroutineFunc)
        {
#if UNITY_WEBGL
        var tcs = new TaskCompletionSource<bool>();
        context.StartCoroutine(WrapCoroutine(coroutineFunc(), tcs));
        return tcs.Task;
#else
            return asyncFunc();
#endif
        }

        private static IEnumerator WrapCoroutine(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
        {
            yield return coroutine;
            tcs.SetResult(true);
        }
    }
}
