using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.SpatialToolbox.Bing.Clustering;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClusteringSample
{
    public partial class MainWindow : Window
    {
        #region Private Properties 

        private ClusteringLayer layer;
        private MapLayer pinLayer;
        private ItemLocationCollection mockData;
 
        #endregion 
 
        #region Constructor 
 
        public MainWindow() 
        { 
            this.InitializeComponent();

            MyMap.Loaded += (s, e) =>
            {
                layer = new ClusteringLayer()
                {
                    ClusterRadius = 35,
                    ClusterType = ClusteringType.Grid
                };

                //Add event handlers to create the pushpins 
                layer.CreateItemPushpin += CreateItemPushpin;
                layer.CreateClusteredItemPushpin += CreateClusteredItemPushpin; 

                MyMap.Children.Add(layer);

                //Create a layer for showing all pins
                pinLayer = new MapLayer();
                MyMap.Children.Add(pinLayer);

                GenerateMockData(500);
            };
        }
 
        #endregion 
 
        #region Button Handlers 
 
        private async void TestDataSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;
                await GenerateMockData(int.Parse(item.Content as string));
            }
        }

        private void ClusterType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;

                ViewAllCbx.IsChecked = false;

                if (string.Compare(item.Content as string, "grid based", true) == 0)
                {                   
                    layer.ClusterType = ClusteringType.Grid;
                }
                else
                {
                    layer.ClusterType = ClusteringType.Point;
                }
            }
        }

        private void ClusterRadius_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (layer != null)
            {
                var cbx = sender as ComboBox;
                var item = cbx.SelectedItem as ComboBoxItem;

                layer.ClusterRadius = int.Parse(item.Content as string);
            }
        }

        private void ViewAllData_Checked(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox; 
            
            pinLayer.Children.Clear();

            if (cb.IsChecked.HasValue && cb.IsChecked.Value)
            {
                layer.Visibility = System.Windows.Visibility.Collapsed;

                for (int i = 0; i < mockData.Count; i++)
                {
                    var pin = new Pushpin();
                    pin.Tag = mockData[i].Item;
                    MapLayer.SetPosition(pin, mockData[i].Location);
                    pinLayer.Children.Add(pin);
                }
            }
            else
            {
                layer.Visibility = System.Windows.Visibility.Visible;
            }
        } 
 
        #endregion 
 
        #region Pushpin Callback Methods 
 
        private UIElement CreateItemPushpin(object item, ClusteredPoint clusterInfo) 
        { 
            var pin = new Pushpin() 
            {
                Tag = item 
            }; 
 
            pin.MouseDown += Pushpin_Tapped;

            MapLayer.SetPosition(pin, clusterInfo.Location);

            return pin; 
        } 
 
        private UIElement CreateClusteredItemPushpin(ClusteredPoint clusterInfo) 
        { 
            var pin = new Pushpin() 
            { 
                Background = new SolidColorBrush(Colors.Red),
                Content = "+", 
                Tag = clusterInfo                 
            }; 
 
            pin.MouseDown += Pushpin_Tapped;
            
            MapLayer.SetPosition(pin, clusterInfo.Location);

            return pin; 
        } 
 
        #endregion 
 
        #region Private Helper Methods 
 
        private void Pushpin_Tapped(object sender, MouseButtonEventArgs e) 
        { 
            string msg = string.Empty; 
 
            if (sender is FrameworkElement) 
            { 
                var tag = (sender as FrameworkElement).Tag; 
 
                if (tag is ItemLocation) 
                { 
                    var item = (tag as ItemLocation).Item; 
 
                    if (item is string) 
                    { 
                        msg = item as string; 
                    } 
                } 
                else if (tag is ClusteredPoint) 
                { 
                    msg = (tag as ClusteredPoint).ItemIndices.Count + " items in cluster."; 
                } 
            } 
 
            if (!string.IsNullOrEmpty(msg)) 
            {
                MessageBox.Show(msg); 
            }

            e.Handled = true;
        } 
 
        private Task GenerateMockData(int numEntities) 
        {
            layer.Items.Clear();

            return Task.Run(() =>
            {
                mockData = new ItemLocationCollection();

                Random rand = new Random();

                object item;
                Location loc;

                for (int i = 0; i < numEntities; i++)
                {
                    item = "Location number: " + i;

                    loc = new Location()
                    {
                        Latitude = rand.NextDouble() * 180 - 90,
                        Longitude = rand.NextDouble() * 360 - 180
                    };

                    mockData.Add(item, loc);
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    //Add mock data to layer 
                    layer.Items.AddRange(mockData);
                }), null);
            });
        } 
 
        #endregion 
    } 
}