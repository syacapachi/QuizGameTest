using System;
using UnityEngine.Networking;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public static class ImageLoader
{
    public static async Task LoadSpriteFromURL(string url, Action<Sprite> onLoaded)
    {
        using (var req = UnityWebRequestTexture.GetTexture(url))
        {
            await req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D tex = DownloadHandlerTexture.GetContent(req);
                Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
                onLoaded?.Invoke(sp);
            }
            else
            {
                Debug.LogError($"画像ロード失敗: {req.error}");
                onLoaded?.Invoke(null);
            }
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
