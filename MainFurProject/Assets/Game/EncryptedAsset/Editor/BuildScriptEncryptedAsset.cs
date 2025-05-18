using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEngine;

namespace EncryptedAsset.Editor
{
    [CreateAssetMenu(fileName = "BuildScriptEncryptedAsset.asset", menuName = "Addressables/Custom Build/Encrypted Asset")]
    public class BuildScriptEncryptedAsset : BuildScriptPackedMode
    {
        public override string Name
        {
            get { return "Encrypted Asset"; }
        }
        
        protected override TResult DoBuild<TResult>(AddressablesDataBuilderInput builderInput, AddressableAssetsBuildContext aaContext)
        {
            Debug.Log($"BuildScriptEncryptedAsset - Begin encrypt....");
            TResult result = base.DoBuild<TResult>(builderInput, aaContext);
            foreach (string path in builderInput.Registry.GetFilePaths())
            {
                if(Path.GetExtension(path).ToLower().Equals(".bundle"))
                    EncodeAsset(path);
            }
            Debug.Log($"BuildScriptEncryptedAsset - finish encrypt....");
            return result;
        }
        
        void EncodeAsset(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            File.WriteAllBytes(path, EncryptXOR(data));
        }
    
        byte[] EncryptXOR(byte[] data)
        {
            string key = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(Application.identifier))).Replace("-", "");
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            int keyLength = keyBytes.Length;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ keyBytes[i % keyLength]);
            }
            return data;
        }
    }
}
