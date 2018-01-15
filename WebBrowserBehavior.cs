using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NetRadio
{
    class WebBrowserBehavior
    {
        private static readonly Type OwnerType = typeof(WebBrowserBehavior);

        public static int GetEmbVersion()
        {
            int ieVer = GetBrowserVersion();

            if (ieVer > 9)
                return ieVer * 1000 + 1;

            if (ieVer > 7)
                return ieVer * 1111;

            return 7000;
        } // End Function GetEmbVersion

        public static void FixBrowserVersion()
        {
            string appName = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
            FixBrowserVersion(appName);
        }

        public static void FixBrowserVersion(string appName)
        {
            FixBrowserVersion(appName, GetEmbVersion());
        } // End Sub FixBrowserVersion

        // FixBrowserVersion("<YourAppName>", 9000);
        public static void FixBrowserVersion(string appName, int ieVer)
        {
            FixBrowserVersion_Internal("HKEY_LOCAL_MACHINE", appName + ".exe", ieVer);
            FixBrowserVersion_Internal("HKEY_CURRENT_USER", appName + ".exe", ieVer);
            FixBrowserVersion_Internal("HKEY_LOCAL_MACHINE", appName + ".vshost.exe", ieVer);
            FixBrowserVersion_Internal("HKEY_CURRENT_USER", appName + ".vshost.exe", ieVer);
        } // End Sub FixBrowserVersion 

        private static void FixBrowserVersion_Internal(string root, string appName, int ieVer)
        {
            try
            {
                
                if (Environment.Is64BitOperatingSystem)     //For 64 bit Machine 
                {
                    if ((int)Microsoft.Win32.Registry.GetValue(root + @"\Software\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", appName, -1) == -1)
                        Microsoft.Win32.Registry.SetValue(root + @"\Software\Wow6432Node\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", appName, ieVer);
                }
                else  //For 32 bit Machine 
                {
                    if ((int)Microsoft.Win32.Registry.GetValue(root + @"\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", appName, -1) == -1)
                        Microsoft.Win32.Registry.SetValue(root + @"\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", appName, ieVer);
                } 

            }
            catch (System.Security.SecurityException e)
            {
                Console.WriteLine(e.StackTrace);
                // some config will hit access rights exceptions
                // this is why we try with both LOCAL_MACHINE and CURRENT_USER
            }
        } // End Sub FixBrowserVersion_Internal 

        public static int GetBrowserVersion()
        {
            // string strKeyPath = @"HKLM\SOFTWARE\Microsoft\Internet Explorer";
            string strKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Internet Explorer";
            string[] ls = new string[] { "svcVersion", "svcUpdateVersion", "Version", "W2kVersion" };

            int maxVer = 0;
            for (int i = 0; i < ls.Length; ++i)
            {
                object objVal = Microsoft.Win32.Registry.GetValue(strKeyPath, ls[i], "0");
                string strVal = System.Convert.ToString(objVal);
                if (strVal != null)
                {
                    int iPos = strVal.IndexOf('.');
                    if (iPos > 0)
                        strVal = strVal.Substring(0, iPos);

                    int res = 0;
                    if (int.TryParse(strVal, out res))
                        maxVer = Math.Max(maxVer, res);
                } // End if (strVal != null)

            } // Next i

            return maxVer;
        } // End Function GetBrowserVersion 

        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached(
                "BindableSource",
                typeof(string),
                OwnerType,
                new UIPropertyMetadata(OnBindableSourcePropertyChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetBindableSource(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableSourceProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

        public static void OnBindableSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var browser = d as WebBrowser;
            if (browser == null) return;

            if(e.NewValue != null)
            {
                var uri = new Uri(e.NewValue.ToString());
                if (uri.IsAbsoluteUri)
                    browser.Source = uri;
                else
                    browser.Source = null;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   
            }
            else 
                browser.Source =  null;
        }

        private static readonly DependencyPropertyKey SilentJavascriptErrorsContextKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "SilentJavascriptErrorsContext",
                typeof(SilentJavascriptErrorsContext),
                OwnerType,
                new FrameworkPropertyMetadata(null));

        private static void SetSilentJavascriptErrorsContext(DependencyObject depObj, SilentJavascriptErrorsContext value)
        {
            depObj.SetValue(SilentJavascriptErrorsContextKey, value);
        }

        private static SilentJavascriptErrorsContext GetSilentJavascriptErrorsContext(DependencyObject depObj)
        {
            return (SilentJavascriptErrorsContext)depObj.GetValue(SilentJavascriptErrorsContextKey.DependencyProperty);
        }

        public static readonly DependencyProperty DisableJavascriptErrorsProperty =
            DependencyProperty.RegisterAttached(
                "DisableJavascriptErrors",
                typeof(bool),
                OwnerType,
                new FrameworkPropertyMetadata(OnDisableJavascriptErrorsChangedCallback));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static void SetDisableJavascriptErrors(DependencyObject depObj, bool value)
        {
            depObj.SetValue(DisableJavascriptErrorsProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static bool GetDisableJavascriptErrors(DependencyObject depObj)
        {
            return (bool)depObj.GetValue(DisableJavascriptErrorsProperty);
        }

        private static void OnDisableJavascriptErrorsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webBrowser = d as WebBrowser;
            if (webBrowser == null) return;
            if (Equals(e.OldValue, e.NewValue)) return;

            var context = GetSilentJavascriptErrorsContext(webBrowser);
            if (context != null)
            {
                context.Dispose();
            }

            if (e.NewValue != null)
            {
                context = new SilentJavascriptErrorsContext(webBrowser);
                SetSilentJavascriptErrorsContext(webBrowser, context);
            }
            else
            {
                SetSilentJavascriptErrorsContext(webBrowser, null);
            }
        }

        private class SilentJavascriptErrorsContext : IDisposable
        {
            private bool? _silent;
            private readonly WebBrowser _webBrowser;


            public SilentJavascriptErrorsContext(WebBrowser webBrowser)
            {
                _silent = new bool?();

                _webBrowser = webBrowser;
                _webBrowser.Loaded += OnWebBrowserLoaded;
                _webBrowser.Navigated += OnWebBrowserNavigated;
            }

            private void OnWebBrowserLoaded(object sender, RoutedEventArgs e)
            {
                if (!_silent.HasValue) return;

                SetSilent();
            }

            private void OnWebBrowserNavigated(object sender, NavigationEventArgs e)
            {
                var webBrowser = (WebBrowser)sender;

                if (!_silent.HasValue)
                {
                    _silent = GetDisableJavascriptErrors(webBrowser);
                }

                if (!webBrowser.IsLoaded) return;

                SetSilent();
            }

            /// <summary>
            /// Solution by Simon Mourier on StackOverflow
            /// http://stackoverflow.com/a/6198700/741414
            /// </summary>
            private void SetSilent()
            {
                _webBrowser.Loaded -= OnWebBrowserLoaded;
                _webBrowser.Navigated -= OnWebBrowserNavigated;

                // get an IWebBrowser2 from the document
                var sp = _webBrowser.Document as IOleServiceProvider;
                if (sp != null)
                {
                    var IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                    var IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                    object webBrowser2;
                    sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser2);
                    if (webBrowser2 != null)
                    {
                        webBrowser2.GetType().InvokeMember(
                            "Silent",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty,
                            null,
                            webBrowser2,
                            new object[] { _silent });
                    }
                }
            }

            [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            private interface IOleServiceProvider
            {
                [PreserveSig]
                int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
            }

            public void Dispose()
            {
                if (_webBrowser != null)
                {
                    _webBrowser.Loaded -= OnWebBrowserLoaded;
                    _webBrowser.Navigated -= OnWebBrowserNavigated;
                }
            }
        }
    }
}
