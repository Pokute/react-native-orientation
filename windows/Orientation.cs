using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.Collections;
using ReactNative.Modules.Core;
using ReactNative.UIManager;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

namespace ReactNative.Modules.Orientation
{
    public class Orientation : ReactContextNativeModuleBase
    {
        private IReadOnlyDictionary<string, object> _Constants;

        public override IReadOnlyDictionary<string, object> Constants
        {
            get
            {
                return _Constants;
            }
        }

        public Orientation(ReactContext reactContext)
            : base(reactContext)
        {
            var TempConstants = new Dictionary<string, object>();
            TempConstants.Add("initialOrientation", CreateDeviceOrientationString(Windows.Graphics.Display.DisplayInformation.GetForCurrentView().CurrentOrientation));
            _Constants = TempConstants;

            Windows.Graphics.Display.DisplayInformation.GetForCurrentView().OrientationChanged += TryingToWork;
        }

        public void TryingToWork (Windows.Graphics.Display.DisplayInformation di, object args)
        {
            JObject newOrientation = new JObject();
            newOrientation.Add("orientation", CreateDeviceOrientationString(di.CurrentOrientation));
            Context.GetJavaScriptModule<RCTDeviceEventEmitter>().emit("orientationDidChange", newOrientation);
            Context.GetJavaScriptModule<RCTDeviceEventEmitter>().emit("specificOrientationDidChange", newOrientation);
        }

        public override string Name
        {
            get
            {
                return "Orientation";
            }
        }

        // This return user interface orientation. getSpecificOrientation returns device orientation.
        // Does not support PortraitUpsideDown yet.
        [ReactMethod]
        async public void getOrientation(ICallback callback)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var orientation = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Orientation;
                callback.Invoke(null, CreateOrientationString(orientation));
            });
        }

        [ReactMethod]
        async public void getSpecificOrientation(ICallback callback)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var deviceorientation = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().CurrentOrientation;
                callback.Invoke(null, CreateDeviceOrientationString(deviceorientation));
            });
        }

        public static JValue CreateOrientationString(Windows.UI.ViewManagement.ApplicationViewOrientation orientation)
        {
            switch (orientation)
            {
                case Windows.UI.ViewManagement.ApplicationViewOrientation.Landscape:
                    return new JValue("LANDSCAPE");
                case Windows.UI.ViewManagement.ApplicationViewOrientation.Portrait:
                    return new JValue("PORTRAIT");
                default:
                    return new JValue("UNKNOWN");
            }
        }

        public static JValue CreateDeviceOrientationString(Windows.Graphics.Display.DisplayOrientations deviceorientation)
        {
            switch (deviceorientation)
            {
                case Windows.Graphics.Display.DisplayOrientations.Landscape:
                    return new JValue("LANDSCAPE-LEFT");
                case Windows.Graphics.Display.DisplayOrientations.Portrait:
                    return new JValue("PORTRAIT");
                case Windows.Graphics.Display.DisplayOrientations.LandscapeFlipped:
                    return new JValue("LANDSCAPE-RIGHT");
                case Windows.Graphics.Display.DisplayOrientations.PortraitFlipped:
                    return new JValue("PORTRAITUPSIDEDOWN");
                default:
                    return new JValue("UNKNOWN");
            }
        }

        [ReactMethod]
        public void lockToPortrait()
        {
            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Portrait |
                                                                                    Windows.Graphics.Display.DisplayOrientations.PortraitFlipped;
        }

        [ReactMethod]
        public void lockToLandscape()
        {
            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Landscape |
                                                                                    Windows.Graphics.Display.DisplayOrientations.LandscapeFlipped;
        }

        [ReactMethod]
        public void lockToLandscapeRight()
        {
            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.Landscape;
        }

        [ReactMethod]
        public void lockToLandscapeLeft()
        {
            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.LandscapeFlipped;
        }

        [ReactMethod]
        public void unlockAllOrientations()
        {
            Windows.Graphics.Display.DisplayInformation.AutoRotationPreferences = Windows.Graphics.Display.DisplayOrientations.None;
        }
    }
}
