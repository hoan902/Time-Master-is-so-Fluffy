using UnityEngine;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif

using System.Runtime.InteropServices;

#if UNITY_IOS

namespace AudienceNetwork
{
public static class AdSettings
{
[DllImport("__Internal")] 
private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
{
FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
}
}
}

#endif

public class AttPermissionRequest : MonoBehaviour {
    void Awake() {
#if UNITY_IOS
        // Check the user's consent status.
        // If the status is undetermined, display the request request:
        if(ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }

        AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
#endif
    }
}