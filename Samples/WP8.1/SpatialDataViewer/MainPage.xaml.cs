using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Maps.SpatialToolbox.IO;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;

namespace SpatialDataViewer
{
    public sealed partial class MainPage : Page
    {
        #region Private Properties

        private ShapeStyle DefaultStyle = new ShapeStyle()
        {
            FillColor = StyleColor.FromArgb(150, 0, 0, 255),
            StrokeColor = StyleColor.FromArgb(150, 0, 0, 0),
            StrokeThickness = 4
        };

        private Dictionary<string, string> DataSourceUrls = new Dictionary<string, string>()
        {
            { "kml", "http://www.bing.com/maps/GeoCommunity.aspx?action=export&format=kml&mkt=en-us&cid=D35222484A76A01!2835" }
        };

        const string STATIC_HTML1 = "<!DOCTYPE html><html><head><style>body,html{{margin:0;padding:0;width:100%;height:100%;}}</style></head><body><div id='WebViewWrapper'>";
        const string STATIC_HTML2 = "</div></body></html>";

        #endregion

        #region Constructor 

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        #endregion

        #region Event Handlers

        private async void ClearMap_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();
        }

        private async void ImportData_Clicked(object sender, RoutedEventArgs e)
        {
            ImportFlyout.Hide();

            try
            {
                ClearMap();

                var selectedItem = sender as Button;
                
                BaseTextFeed reader = null;
                string defaultExt = string.Empty;
                string filters = string.Empty;

                //Check to see if the Tag property of the selected item has a string that is a spatial file type.
                if (selectedItem != null && DataSourceUrls.ContainsKey(selectedItem.Tag.ToString()))
                {
                    var key = selectedItem.Tag.ToString();
                    var url = DataSourceUrls[key];

                    switch (key)
                    {
                        case "gpx":
                            reader = new GpxFeed();
                            break;
                        case "georss":
                            reader = new GeoRssFeed();
                            break;
                        case "kml":
                            reader = new KmlFeed();
                            break;
                        case "geojson":
                            reader = new GeoJsonFeed();
                            break;
                        default:
                            break;
                    }

                    if (reader != null && !string.IsNullOrEmpty(url))
                    {
                        var data = await reader.ReadAsync(new Uri(url));

                        if (string.IsNullOrEmpty(data.Error))
                        {
                            //Load the spatial set data into the map
                            MapTools.LoadGeometries(data, MyMap.Children, MyMap.MapElements, DefaultStyle, DisplayMetadata);
                            
                            //If the data set has a bounding box defined, use it to set the map view.
                            if (data.BoundingBox != null)
                            {
                                var bounds = data.BoundingBox;
                                await MyMap.TrySetViewBoundsAsync(bounds.ToBMGeometry(), new Thickness(30), MapAnimationKind.Default);
                            }
                        }
                        else
                        {
                            //If there is an error message, display it to the user.                            
                            var dialog = new MessageDialog(data.Error);
                            await dialog.ShowAsync();
                        }
                    }
                }
                else
                {
                    var dialog = new MessageDialog("Data Source currently not setup.");
                    await dialog.ShowAsync();
                }
            }
            catch { }
        }

        private void CloseMetadataPanel_Clicked(object sender, RoutedEventArgs e)
        {
            MetadataPanel.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Private Methods

        private void ClearMap()
        {
            MyMap.Children.Clear();
            MyMap.MapElements.Clear();
            MetadataTbx.Text = string.Empty;
            MetadataPanel.DataContext = null;
            MetadataPanel.Visibility = Visibility.Collapsed;
            DescriptionBox.Navigate(new Uri("about:blank"));
        }

        private void DisplayMetadata(object sender, RoutedEventArgs e)
        {
            ShapeMetadata tagMetadata = null;
            StringBuilder sb = new StringBuilder();

            MetadataPanel.DataContext = null;
            DescriptionBox.Navigate(new Uri("about:blank"));

            if (sender is MapElement)
            {
                var shape = sender as MapElement;
                tagMetadata = shape.GetValue(MapElementExt.TagProperty) as ShapeMetadata;
            }
            else if (sender is FrameworkElement)
            {
                var pin = sender as FrameworkElement;
                //Get the stored metadata from the Tag property
                tagMetadata = pin.Tag as ShapeMetadata;
            }

            if (tagMetadata != null && tagMetadata.HasMetadata())
            {
                MetadataPanel.DataContext = tagMetadata;

                if (!string.IsNullOrEmpty(tagMetadata.Description))
                {
                    string webPageData = STATIC_HTML1 + tagMetadata.Description + STATIC_HTML2;
                    DescriptionBox.NavigateToString(webPageData);
                }

                foreach (var val in tagMetadata.Properties)
                {
                    sb.AppendLine(val.Key + ": " + val.Value);
                }
            }

            MetadataTbx.Text = sb.ToString();
            MetadataPanel.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
