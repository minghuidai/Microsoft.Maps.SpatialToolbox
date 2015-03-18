using Bing.Maps;
using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Maps.SpatialToolbox.IO;
using System;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SpatialDataViewer
{
    public sealed partial class MainPage : Page
    {
        #region Private Properties

        private MapShapeLayer PolyLayer;

        private ShapeStyle DefaultStyle = new ShapeStyle()
        {
            FillColor = StyleColor.FromArgb(150, 0, 0, 255),
            StrokeColor = StyleColor.FromArgb(150, 0, 0, 0),
            StrokeThickness = 4 
        };

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            PolyLayer = new MapShapeLayer();
            MyMap.ShapeLayers.Add(PolyLayer);
        }

        #endregion

        #region Button Handlers

        private async void ImportData_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var selectedItem = sender as Windows.UI.Xaml.Controls.MenuFlyoutItem;

                BaseFeed reader = null;
                string[] fileTypes = null;

                //Check to see if the Tag property of the selected item has a string that is a spatial file type.
                if (selectedItem != null)
                {
                    switch (selectedItem.Tag.ToString())
                    {
                        case "wkt":
                            reader = new WellKnownText();
                            fileTypes = new string[] { ".txt" };
                            break;
                        case "shp":
                            reader = new ShapefileReader();
                            fileTypes = new string[] { ".shp" };
                            break;
                        case "gpx":
                            reader = new GpxFeed();
                            fileTypes = new string[] { ".xml", ".gpx" };
                            break;
                        case "georss":
                            reader = new GeoRssFeed();
                            fileTypes = new string[] { ".xml", ".rss" };
                            break;
                        case "kml":
                            reader = new KmlFeed();
                            fileTypes = new string[] { ".xml", ".kml", ".kmz" };
                            break;
                        case "geojson":
                            reader = new GeoJsonFeed();
                            fileTypes = new string[] { ".js", ".json" };
                            break;
                        default:
                            break;
                    }
                }

                if (reader != null && fileTypes != null)
                {
                    ClearMap();

                    //Create a FileOpenPicker to allow the user to select which file to import
                    var openPicker = new FileOpenPicker()
                    {
                        ViewMode = PickerViewMode.List,
                        SuggestedStartLocation = PickerLocationId.Desktop
                    };

                    //Add the allowed file extensions to the FileTypeFilter
                    foreach (var type in fileTypes)
                    {
                        openPicker.FileTypeFilter.Add(type);
                    }

                    //Get the selected file
                    var file = await openPicker.PickSingleFileAsync();
                    if (file != null)
                    {
                        using (var fileStream = await file.OpenStreamForReadAsync())
                        {
                            //Read the spatial data file
                            var data = await reader.ReadAsync(fileStream);

                            if (string.IsNullOrEmpty(data.Error))
                            {
                                //Load the spatial set data into the map
                                MapTools.LoadGeometries(data, PinLayer, PolyLayer, DefaultStyle, DisplayInfobox);

                                //If the data set has a bounding box defined, use it to set the map view.
                                if (data.BoundingBox != null)
                                {
                                    MyMap.SetView(data.BoundingBox.ToBMGeometry());
                                }
                            }
                            else
                            {
                                //If there is an error message, display it to the user.
                                var msg = new Windows.UI.Popups.MessageDialog(data.Error);
                                await msg.ShowAsync();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void CloseInfobox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #endregion

        #region Private Methods

        private void ClearMap()
        {
            PinLayer.Children.Clear();
            PolyLayer.Shapes.Clear();
            InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void DisplayInfobox(object sender, TappedRoutedEventArgs e)
        {
            ShapeMetadata tagMetadata = null;
            Location anchor = null;

            if (sender is FrameworkElement)
            {
                var pin = sender as Windows.UI.Xaml.FrameworkElement;
                anchor = MapLayer.GetPosition(pin);

                //Get the stored metadata from the Tag property
                tagMetadata = pin.Tag as ShapeMetadata;
            }
            else if (sender is MapShape)
            {
                var shape = sender as MapShape;
                tagMetadata = shape.GetValue(MapShapeExt.TagProperty) as ShapeMetadata;
                anchor = shape.ToGeometry().Envelope().Center.ToBMGeometry();
            }

            if (anchor != null && tagMetadata != null)
            {
                bool hasContent = false;

                if (!string.IsNullOrWhiteSpace(tagMetadata.Title))
                {
                    InfoboxTitle.Text = tagMetadata.Title;
                    hasContent = true;
                }
                else if (!string.IsNullOrWhiteSpace(tagMetadata.ID))
                {
                    //Set the Infobox title as the ID
                    InfoboxTitle.Text = tagMetadata.ID;
                    hasContent = true;
                }

                if (!string.IsNullOrWhiteSpace(tagMetadata.Description))
                {
                    //Since the description value is being passed to a WebView, use the NavigateToString method to render the HTML.
                    InfoboxDescription.NavigateToString(tagMetadata.Description);
                    InfoboxDescription.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    hasContent = true;
                }
                else
                {
                    InfoboxDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }

                if (hasContent)
                {
                    //Display the infobox
                    InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }

                //Set the position of the infobox
                MapLayer.SetPosition(Infobox, anchor);
            }
        }

        #endregion
    }
}
