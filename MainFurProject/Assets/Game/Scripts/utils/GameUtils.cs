using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CleverCrow.Fluid.FSMs;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public static class GameUtils
{
    public static Vector3 TransformPointOnly(this Transform transform, Vector3 position)
    {
        var localToWorldMatrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one);
        return localToWorldMatrix.MultiplyPoint3x4(position);
    }

    public static Vector3 InverseTransformPointOnly(this Transform transform, Vector3 position)
    {
        var worldToLocalMatrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one).inverse;
        return worldToLocalMatrix.MultiplyPoint3x4(position);
    }

    public static string CoinToString(long coin)
    {
        return coin.ToString("N0", new CultureInfo(GetLanguageString(Application.systemLanguage)));
    }

    public static string GetLanguageString(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Vietnamese:
                return "vi";
            default:
                return "en-US";
        }
    }

    public static Transform RecursiveFindChild(Transform parent, string childName)
    {
        Transform child = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
            else
            {
                child = RecursiveFindChild(child, childName);
                if (child != null)
                {
                    return child;
                }
            }
        }
        return child;
    }

    public static void DecryptXOR(byte[] data, string baseKey, Action<byte[]> complete)
    {
        Thread thread = new Thread(() =>
        {
            string key = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(baseKey))).Replace("-", "");
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int keyLength = keyBytes.Length;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ keyBytes[i % keyLength]);
            }
            complete?.Invoke(data);
        });
        thread.Start();
    }

    public static async Task<byte[]> DecryptAsync(byte[] data, byte[] key)
    {
        using (var aes = Aes.Create())
        {
            string hash = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(Application.identifier))).Replace("-", "");
            byte[] iv = Encoding.UTF8.GetBytes(hash.Substring(0, 16));
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Padding = PaddingMode.Zeros;

            aes.Key = key;
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] result = await PerformCryptographyAsync(data, decryptor);
                return result;
            }
        }
    }

    private static async Task<byte[]> PerformCryptographyAsync(byte[] data, ICryptoTransform cryptoTransform)
    {
        using (var ms = new MemoryStream())
        using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
        {
            await cryptoStream.WriteAsync(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return ms.ToArray();
        }
    }
    
    public static void SetTransition(this StateBuilder from, Enum to)
    {
        from.SetTransition($"{from.Id}{to}", to);
    }
    
    public static void Transition(this IState from, Enum to)
    {
        from.Transition($"{from.Id}{to}");
    }
}

public static class GameExtension
{
    public static void SetSkin(this SkeletonGraphic spine, string skinName)
    {
        Skin skin = new Skin("new-skin");
        skin.AddSkin(spine.SkeletonData.FindSkin(skinName));
        spine.Skeleton.SetSkin(skin);
        spine.Skeleton.SetSlotsToSetupPose();
        spine.AnimationState.Apply(spine.Skeleton);
    }

    public static void SetSkin(this SkeletonAnimation spine, string skinName)
    {
        Skin skin = new Skin("character-base");
        skin.AddSkin(spine.skeletonDataAsset.GetSkeletonData(false).FindSkin(skinName));
        spine.Skeleton.SetSkin(skin);
        spine.Skeleton.SetSlotsToSetupPose();
        spine.AnimationState.Apply(spine.Skeleton);
    }

    public static void SetMixSkin(this SkeletonAnimation spine, string skinName, string weapon)
    {
        Skin skin = new Skin("character-base");
        SkeletonData data = spine.skeletonDataAsset.GetSkeletonData(false);
        skin.AddSkin(data.FindSkin(skinName));
        skin.AddSkin(data.FindSkin(weapon));
        spine.Skeleton.SetSkin(skin);
        spine.Skeleton.SetSlotsToSetupPose();
        spine.AnimationState.Apply(spine.Skeleton);
    }

    public static void SetMixSkin(this SkeletonGraphic spine, string skinName, string weapon)
    {
        Skin skin = new Skin("character-base");
        SkeletonData data = spine.skeletonDataAsset.GetSkeletonData(false);
        skin.AddSkin(data.FindSkin(skinName));
        skin.AddSkin(data.FindSkin(weapon));
        spine.Skeleton.SetSkin(skin);
        spine.Skeleton.SetSlotsToSetupPose();
        spine.AnimationState.Apply(spine.Skeleton);
    }

    public static Transform GetTransformBinding(PlayableDirector director, TrackAsset track)
    {
        if (director == null)
            return null;

        var binding = director.GetGenericBinding(track);
        if (binding is Component)
            return ((Component) binding).transform;

        if (binding is GameObject)
            return ((GameObject) binding).transform;

        return null;
    }

    public static Vector3[] ToVector3Array(this Vector2[] v2)
    {
        return System.Array.ConvertAll<Vector2, Vector3>(v2, GetV2FromV3);
    }
    public static Vector3 GetV2FromV3 (Vector2 v2)
	{
		return new Vector3 (v2.x, v2.y, 0);
	}
}
