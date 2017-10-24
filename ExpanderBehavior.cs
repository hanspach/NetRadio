using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace NetRadio
{
    class ExpanderBehavior : Behavior<Expander>
    {
        private Grid grid;
        private double oldWidth;

        protected override void OnAttached()
        {
            base.OnAttached();
            Expander exp = AssociatedObject as Expander;
            grid = exp.Parent as Grid;
            exp.Collapsed += Exp_Collapsed;
            exp.Expanded += Exp_Expanded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Collapsed -= Exp_Collapsed;
            AssociatedObject.Expanded -= Exp_Expanded;
        }
    
        private void Exp_Expanded(object sender, RoutedEventArgs e)
        {
            grid.ColumnDefinitions[0].Width = new GridLength(oldWidth, GridUnitType.Star);
        }

        private void Exp_Collapsed(object sender, RoutedEventArgs e)
        {
            oldWidth = grid.ColumnDefinitions[0].Width.Value; 
            grid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
        }
    }
}
