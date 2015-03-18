using Microsoft.Maps.SpatialToolbox.Bing;
using System;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace HeatMapTestApp
{
    public sealed partial class MainPage : Page
    {
        #region Private Properties

        private HeatMapLayer layer;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.Loaded += (s, e) =>
            {
                layer = new HeatMapLayer()
                {
                    ParentMap = MyMap,
                    Locations = GetTestData(500),
                    Radius = 250000
                };

                MyMap.Children.Add(layer);

                layer.Locations = GetTestData(10000);
            };
        }

        #endregion

        #region Event Handlers

        private void TestDataSize_Changed(object sender, SelectionChangedEventArgs e)
        {         
            if (layer != null)
            {
                TestDataSizeFlyout.Hide();

                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;
                layer.Locations = GetTestData(int.Parse(item.Content as string));
            }
        }

        private void Intensity_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                IntensityFlyout.Hide();

                var cbx = sender as ComboBox;

                layer.Intensity = double.Parse((cbx.SelectedItem as ComboBoxItem).Content as string);
            }
        }

        private void Radius_Changed(object sender, SelectionChangedEventArgs e)
        {            
            if (layer != null)
            {
                RadiusFlyout.Hide();

                var cbx = sender as ComboBox;
                var rKM = int.Parse((cbx.SelectedItem as ComboBoxItem).Content as string);
                layer.Radius = rKM * 1000;
            }
        }

        private void Opacity_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                OpacityFlyout.Hide();

                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;

                var opacity = double.Parse(item.Content as string);
                layer.Opacity = opacity;
            }
        }

        private void Gradient_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                HeatGradientFlyout.Hide();

                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;

                var bg = item.Background as LinearGradientBrush;
                layer.HeatGradient = bg.GradientStops;
            }
        }

        private void Background_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                BackgroundColorFlyout.Hide();

                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;
                layer.Background = item.Background;
            }
        }

        #endregion

        #region Private Methods

        private Geopath GetTestData(int size)
        {
            var locs = new List<BasicGeoposition>();
            var rand = new Random();

            for (int i = 0; i < size; i++)
            {
                locs.Add(new BasicGeoposition() {
                    Latitude = rand.NextDouble() * 170 - 85,
                    Longitude = rand.NextDouble() * 360 - 180
                });
            }

            return new Geopath(locs);
        }

        #endregion
    }
}