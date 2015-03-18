using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Maps.SpatialToolbox.Bing.Clustering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ClusteringSample
{
    public sealed partial class MainPage : Page
    {
        #region Private Properties

        private ClusteringLayer layer;
        private ItemLocationCollection mockData;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            MyMap.Loaded += (s, e) =>
            {
                layer = new ClusteringLayer(MyMap)
                {
                    ClusterRadius = 35,
                    ClusterType = ClusteringType.Grid
                };

                //Add event handlers to create the pushpins 
                layer.CreateItemPushpin += CreateItemPushpin;
                layer.CreateClusteredItemPushpin += CreateClusteredItemPushpin;

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

                if (string.Compare(item.Content as string, "grid based", StringComparison.OrdinalIgnoreCase) == 0)
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

        #endregion

        #region Pushpin Callback Methods

        private UIElement CreateItemPushpin(object item, ClusteredPoint clusterInfo)
        {
            var point = new Microsoft.Maps.SpatialToolbox.Point(clusterInfo.Location.ToGeometry());

            point.Metadata = new ShapeMetadata();
            point.Metadata.Properties.Add("ClusterInfo", item);

            var pin = StyleTools.GenerateMapShape(point, new ShapeStyle()
            {
                IconColor = new StyleColor() { A = 255, B = 255 },
                StrokeColor = new StyleColor() { A = 255, R = 255, B = 255, G = 255 },
                StrokeThickness = 3
            });

            pin.Tapped += Pushpin_Tapped;

            return pin;
        }

        private UIElement CreateClusteredItemPushpin(ClusteredPoint clusterInfo)
        {
            var point = new Microsoft.Maps.SpatialToolbox.Point(clusterInfo.Location.ToGeometry());

            point.Metadata = new ShapeMetadata();
            point.Metadata.Properties.Add("ClusterInfo", clusterInfo);

            var pin = StyleTools.GenerateMapShape(point, new ShapeStyle()
            {
                IconColor = new StyleColor() { A = 255, R = 255 },
                StrokeColor = new StyleColor() { A = 255, R = 255, B = 255, G = 255 },
                StrokeThickness = 3
            });

            pin.Tapped += Pushpin_Tapped;

            return pin;
        }

        #endregion 

        #region Private Methods

        private async void Pushpin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is UIElement)
            {
                var pushpin = sender as UIElement;
                var tag = pushpin.GetValue(MapElementExt.TagProperty);

                if (tag is ShapeMetadata)
                {
                    var clusterInfo = (tag as ShapeMetadata).Properties["ClusterInfo"];

                    if (clusterInfo != null)
                    {
                        string msg = string.Empty;

                        if (clusterInfo is ItemLocation)
                        {
                            msg = (clusterInfo as ItemLocation).Item as string;
                        }
                        else if (clusterInfo is ClusteredPoint)
                        {
                            msg = (clusterInfo as ClusteredPoint).ItemIndices.Count + " items in cluster.";
                        }

                        if (!string.IsNullOrEmpty(msg))
                        {
                            var dialog = new MessageDialog(msg);
                            await dialog.ShowAsync();
                        }
                    }
                }
            }
            
            e.Handled = true;
        }

        private Task GenerateMockData(int numEntities)
        {
            layer.Items.Clear();

            return Task.Run(async () =>
            {
                mockData = new ItemLocationCollection();

                Random rand = new Random();

                object item;
                Geopoint loc;

                for (int i = 0; i < numEntities; i++)
                {
                    item = "Location number: " + i;

                    loc = new Geopoint(new BasicGeoposition()
                    {
                        Latitude = rand.NextDouble() * 180 - 90,
                        Longitude = rand.NextDouble() * 360 - 180
                    });

                    mockData.Add(item, loc);
                }

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //Add mock data to layer 
                    layer.Items.AddRange(mockData);
                });
            });
        } 

        #endregion
    }
}
