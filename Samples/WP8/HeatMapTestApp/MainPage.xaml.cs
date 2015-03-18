using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HeatMapTestApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region Private Properties

        private HeatMapLayer layer;
        private GeoCoordinateCollection testData;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += (s, e) =>
            {
                testData = new GeoCoordinateCollection();

                var baseLayer = new MapLayer();
                MyMap.Layers.Add(baseLayer);

                layer = new HeatMapLayer()
                {
                    ParentMap = MyMap,
                    Locations = GetTestData(500),
                    Radius = 250000
                };

                var overlay = new MapOverlay()
                {
                    Content = layer
                };

                baseLayer.Add(overlay);

                layer.Locations = GetTestData(10000);
            };
        }

        #endregion

        #region Event Handlers

        private void TestDataSizeBtn_Clicked(object sender, EventArgs e)
        {
            TestDataPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void RadiusBtn_Clicked(object sender, EventArgs e)
        {
            RadiusPanel.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void IntensityBtn_Clicked(object sender, EventArgs e)
        {
            IntensityPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void HeatGrdBtn_Clicked(object sender, EventArgs e)
        {
            HeatGradientPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void BackgroundBtn_Clicked(object sender, EventArgs e)
        {
            BackgroundPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void OpacityBtn_Clicked(object sender, EventArgs e)
        {
            OpacityPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void TestDataSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;
                layer.Locations = GetTestData(int.Parse(item.Content as string));

                TestDataPanel.Visibility = System.Windows.Visibility.Collapsed;
            }            
        }

        private void Intensity_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;

                layer.Intensity = double.Parse((cbx.SelectedItem as ComboBoxItem).Content as string);

                IntensityPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Radius_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var rKM = int.Parse((cbx.SelectedItem as ComboBoxItem).Content as string);
                layer.Radius = rKM * 1000;

                RadiusPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Opacity_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;

                var opacity = double.Parse(item.Content as string);
                layer.Opacity = opacity;

                OpacityPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Gradient_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;

                var bg = item.Background as LinearGradientBrush;
                layer.HeatGradient = bg.GradientStops;

                HeatGradientPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void Background_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;
                layer.Background = item.Background;

                BackgroundPanel.Visibility = System.Windows.Visibility.Collapsed;
            }            
        }

        #endregion

        #region Private Methods

        private GeoCoordinateCollection GetTestData(int size)
        {
            if (size <= testData.Count)
            {
                var locs = new GeoCoordinateCollection();

                for (int i = 0; i < size; i++)
                {
                    locs.Add(testData[i]);
                }

                return locs;
            }
            else
            {
                var cnt = size - testData.Count;
                var rand = new Random();

                for (int i = 0; i < cnt; i++)
                {
                    testData.Add(new GeoCoordinate()
                    {
                        Latitude = rand.NextDouble() * 170 - 85,
                        Longitude = rand.NextDouble() * 360 - 180
                    });
                }

                return testData;
            }
        }

        #endregion
    }
}