using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NetRadio
{
    class BlinkButtonBehavior
    {
        static readonly DispatcherTimer timer = new DispatcherTimer();
        static bool isDimmed;

        public static Button BlinkButton { get; set; }

        static BlinkButtonBehavior()
        {
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += delegate (object sender, EventArgs args)
            {
               if(BlinkButton != null)
                {
                    if (isDimmed)
                        BlinkButton.Opacity = 1.0;
                    else
                        BlinkButton.Opacity = 0.2;
                    isDimmed = !isDimmed;
                }
            };
            timer.Start();
        }
        
        public static readonly DependencyProperty BlinkProperty =
            DependencyProperty.RegisterAttached("Blink", typeof(bool), typeof(BlinkButtonBehavior), new UIPropertyMetadata(OnBlinkChanged));

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static bool GetBlink(DependencyObject o)
        {
            return (bool)o.GetValue(BlinkProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Button))]
        public static void SetBlink(DependencyObject o, bool value)
        {
            o.SetValue(BlinkProperty, value);
        }

        public static void OnBlinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var btn = d as Button;
            if(btn != null)
            {
                if ((bool)e.NewValue)
                {
                    timer.Stop();
                    btn.Opacity = 1.0;
                }
                else
                    timer.Start();
            }
        }
    }
}
