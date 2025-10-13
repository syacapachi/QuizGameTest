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
                Debug.LogError($"âÊëúÉçÅ[Éhé∏îs: {req.error}");
                onLoaded?.Invoke(null);
            }
        }
    }
}
