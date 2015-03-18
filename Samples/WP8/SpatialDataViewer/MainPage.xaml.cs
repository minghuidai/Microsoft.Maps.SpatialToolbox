using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SpatialDataViewer.Resources;
using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.IO;
using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Phone.Maps.Controls;
using System.Text;

namespace SpatialDataViewer
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region Private Properties

        private MapLayer PinLayer;

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
            InitializeComponent();
            MyMap.Loaded += (s, e) =>
            {
                PinLayer = new MapLayer();
                MyMap.Layers.Add(PinLayer);
            };
        }

        #endregion

        #region Event Handlers

        private void ClearMap_Clicked(object sender, EventArgs e)
        {
            ClearMap();
        }

        private void ImportBtn_Clicked(object sender, EventArgs e)
        {
            ImportPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private async void ImportData_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMap();
                ImportPanel.Visibility = System.Windows.Visibility.Collapsed;

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
                            MapTools.LoadGeometries(data, PinLayer, MyMap.MapElements, DefaultStyle, DisplayMetadata);

                            //If the data set has a bounding box defined, use it to set the map view.
                            if (data.BoundingBox != null)
                            {
                                var bounds = data.BoundingBox;
                                MyMap.SetView(bounds.Center.ToBMGeometry(), 10);
                            }
                        }
                        else
                        {
                            //If there is an error message, display it to the user.
                            MessageBox.Show(data.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Data Source currently not setup.");
                }
            }
            catch { }
        }

        private void CloseMetadataPanel_Clicked(object sender, RoutedEventArgs e)
        {
            MetadataPanel.Visibility = System.Windows.Visibility.Collapsed;
        }

        #endregion

        #region Private Methods

        private void ClearMap()
        {
            PinLayer.Clear();
            MyMap.MapElements.Clear();
            MetadataTbx.Text = string.Empty;
            MetadataPanel.DataContext = null;
            MetadataPanel.Visibility = System.Windows.Visibility.Collapsed;
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
            MetadataPanel.Visibility = System.Windows.Visibility.Visible;
        }

        #endregion
    }
}