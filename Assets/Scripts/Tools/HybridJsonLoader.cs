using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class HybridJsonLoader<T> where T : class
{
    /// <summary>
    /// StreamingAssets内のJSONファイルをロードしてT型として返す。
    /// WebGLビルド時は自動でCoroutine方式に切り替える。
    /// </summary>
    public static async Task<T> LoadJsonAsync(string jsonFileName, MonoBehaviour host)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);

#if UNITY_WEBGL || !UNITY_EDITOR
        // 🔹 WebGL用：コルーチンに切り替え
        var tcs = new TaskCompletionSource<T>();
        host.StartCoroutine(LoadJsonCoroutine(filePath, tcs));
        return await tcs.Task;
#else
        // 🔹 通常環境：async/awaitで直接読み込み
        if (!File.Exists(filePath))
        {
            Debug.LogError($"❌ File not found: {filePath}");
            return null;
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            T data = JsonUtility.FromJson<T>(json);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Failed to load JSON ({filePath}): {e.Message}");
            return null;
        }
#endif
    }

#if UNITY_WEBGL || !UNITY_EDITOR
    /// <summary>
    /// WebGLではFile IOが使えないためUnityWebRequestを使用。
    /// </summary>
    private static IEnumerator LoadJsonCoroutine(string filePath, TaskCompletionSource<T> tcs)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ Failed to load JSON (WebGL): {request.error}");
                tcs.SetResult(null);
                yield break;
            }

            try
            {
                T data = JsonUtility.FromJson<T>(request.downloadHandler.text);
                tcs.SetResult(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ JSON parse failed: {e.Message}");
                tcs.SetResult(null);
            }
        }
    }
#endif
}
