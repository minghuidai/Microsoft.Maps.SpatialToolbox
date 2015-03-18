using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Maps.SpatialToolbox.IO;
using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace DataViewer
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

        private SpatialDataSet CurrentDataSet = null;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        #endregion

        #region Button Handlers

        private async void ImportData_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var selectedItem = (sender as Button).Tag.ToString();

                BaseFeed reader = null;
                string[] fileTypes = null;

                //Check to see if the Tag property of the selected item has a string that is a spatial file type.
                if (!string.IsNullOrEmpty(selectedItem))
                {
                    switch (selectedItem)
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
#if WINDOWS_APP
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

                    var file = await openPicker.PickSingleFileAsync();

                    if (file != null)
                    {
                        using (var fileStream = await file.OpenStreamForReadAsync())
                        {
                            //Read the spatial data file
                            CurrentDataSet = await reader.ReadAsync(fileStream);
                        }
                    }
#elif WINDOWS_PHONE_APP
                    if(DataSourceUrls.ContainsKey(selectedItem))
                    {
                        using (var fileStream = await ServiceHelper.GetStreamAsync(new Uri(DataSourceUrls[selectedItem])))
                        {
                            //Read the spatial data file
                            CurrentDataSet = await reader.ReadAsync(fileStream);
                        }                       
                    }
#endif                        

                    if (string.IsNullOrEmpty(CurrentDataSet.Error))
                    {
                        //Load the spatial set data into the map
                        MapTools.LoadGeometries(CurrentDataSet, MyMap.PinLayer, MyMap.ShapeLayer, DefaultStyle, null);

                        //If the data set has a bounding box defined, use it to set the map view.
                        if (CurrentDataSet.BoundingBox != null)
                        {
#if WINDOWS_APP
                            MyMap.SetView(CurrentDataSet.BoundingBox.Center.ToBMGeometry().ToGeopoint().Position, 10);
#elif WINDOWS_PHONE_APP
                            MyMap.SetView(CurrentDataSet.BoundingBox.Center.ToBMGeometry(), 10);
#endif
                        }
                    }
                    else
                    {
                        //If there is an error message, display it to the user.
                        var msg = new Windows.UI.Popups.MessageDialog(CurrentDataSet.Error);
                        await msg.ShowAsync();
                    }                    
                }
            }
            catch (Exception ex)
            {
                var t = "";
            }
        }

        private void ClearMap_Clicked(object sender, RoutedEventArgs e)
        {
            MyMap.ClearMap();
        }

        #endregion  
    }
}
