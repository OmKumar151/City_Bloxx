// Native iPhone haptics (Taptic Engine) for Haptics.cs.
// Unity includes .mm files in Assets/Plugins/iOS in iOS builds only.

#import <UIKit/UIKit.h>

extern "C"
{
    // style: 0 = light, 1 = medium, 2 = heavy, 3 = rigid, 4 = soft.
    // intensity: 0-1.
    void CityBloxx_HapticImpact(int style, float intensity)
    {
        UIImpactFeedbackStyle feedbackStyle;

        switch (style)
        {
            case 0: feedbackStyle = UIImpactFeedbackStyleLight; break;
            case 2: feedbackStyle = UIImpactFeedbackStyleHeavy; break;
            case 3: feedbackStyle = UIImpactFeedbackStyleRigid; break;
            case 4: feedbackStyle = UIImpactFeedbackStyleSoft; break;
            default: feedbackStyle = UIImpactFeedbackStyleMedium; break;
        }

        UIImpactFeedbackGenerator *generator =
            [[UIImpactFeedbackGenerator alloc] initWithStyle:feedbackStyle];

        [generator prepare];
        [generator impactOccurredWithIntensity:(CGFloat)MAX(0.0f, MIN(1.0f, intensity))];
    }

    // type: 0 = success, 1 = warning, 2 = error.
    void CityBloxx_HapticNotification(int type)
    {
        UINotificationFeedbackType feedbackType;

        switch (type)
        {
            case 0: feedbackType = UINotificationFeedbackTypeSuccess; break;
            case 1: feedbackType = UINotificationFeedbackTypeWarning; break;
            default: feedbackType = UINotificationFeedbackTypeError; break;
        }

        UINotificationFeedbackGenerator *generator =
            [[UINotificationFeedbackGenerator alloc] init];

        [generator prepare];
        [generator notificationOccurred:feedbackType];
    }
}
