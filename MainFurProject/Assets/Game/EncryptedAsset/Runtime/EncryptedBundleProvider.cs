using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace EncryptedAsset.Runtime
{
    [DisplayName("Encrypted Bundle Provider")]
    public class EncryptedBundleProvider : AssetBundleProvider
    {
        public override void Provide(ProvideHandle providerInterface)
        {
#if UNLOAD_BUNDLE_ASYNC
            if (m_UnloadingBundles.TryGetValue(providerInterface.Location.InternalId, out var unloadOp))
            {
                if (unloadOp.isDone)
                    unloadOp = null;
            }
            new EncryptedBundleResource().Start(providerInterface, unloadOp);
#else
            new EncryptedBundleResource().Start(providerInterface);
#endif
        }
    }
}
