using System.Windows;
using System.Windows.Controls;

namespace SliderControl
{
    public class ModernSlider : Slider
    {
        static ModernSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernSlider), new FrameworkPropertyMetadata(typeof(ModernSlider)));
        }
    }
}
