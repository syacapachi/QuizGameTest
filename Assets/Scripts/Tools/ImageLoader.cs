using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public static class ImageLoader
{
    /// <summary>
    /// URLから画像を読み込み、Spriteを返す。
    /// 本番想定,分岐後の関数はアセットで使おう
    /// WebGLではコルーチン方式、その他ではasync/await方式で動作。
    /// </summary>
    public static Task<Sprite> LoadSpriteAuto(MonoBehaviour context, string url)
    {
#if UNITY_WEBGL||UNITY_EDITOR
        // WebGLでは Coroutine にフォールバック
        var tcs = new TaskCompletionSource<Sprite>();
        context.StartCoroutine(LoadSpriteFromURLCoroutine(url, sp => tcs.SetResult(sp)));
        return tcs.Task;
#else
        // 他プラットフォームでは async/await を使用
        var tcs = new TaskCompletionSource<Sprite>();
        return LoadSpriteFromURL(url, sp => tcs.SetResult(sp));
#endif
    }
    public static async Task<Sprite> LoadSpriteFromURL(string url, Action<Sprite> onLoaded)
    {
        using (var req = UnityWebRequestTexture.GetTexture(url))
        {
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"画像ロード失敗: {req.error}");
                onLoaded?.Invoke(null);
                return null;
            }
            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            onLoaded?.Invoke(sp);
            
            return sp;
            
        }
    }
    public static IEnumerator LoadSpriteFromURLCoroutine(string url, System.Action<Sprite> onLoaded)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load image from URL: {url}\n{uwr.error}");
                onLoaded?.Invoke(null);
                yield break;
            }
            
            Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            onLoaded?.Invoke(sp);
            
        }
    }
    public static Sprite SpriteFromByteArray(byte[] bytes)
    {
        Debug.Log("SpriteFromByteArray Call");
        Texture2D loadTexture = new Texture2D(2, 2);
        if (!loadTexture.LoadImage(bytes))
        {
            Debug.LogWarning("⚠ LoadImage failed");
            return null;
        }

        Debug.Log("Create Sprite Data");
        Sprite Image = Sprite.Create(
            loadTexture,
            new Rect(0, 0, loadTexture.width, loadTexture.height),
            Vector2.zero
        );
        Debug.Log("Task End ");
        return Image;
    }
}
