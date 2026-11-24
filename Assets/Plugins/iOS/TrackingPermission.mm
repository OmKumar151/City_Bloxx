// App Tracking Transparency bridge for TrackingPermission.cs.
#import <AppTrackingTransparency/AppTrackingTransparency.h>

extern "C"
{
    // 0 notDetermined, 1 restricted, 2 denied, 3 authorized.
    int _AttStatus()
    {
        if (@available(iOS 14, *))
            return (int)[ATTrackingManager trackingAuthorizationStatus];

        return 3; // Before iOS 14 no permission is needed.
    }

    void _AttRequest()
    {
        if (@available(iOS 14, *))
        {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:
                ^(ATTrackingManagerAuthorizationStatus status) {}];
        }
    }
}
