using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.Devices.Geolocation;
using System.Collections.Generic;
using Windows.UI;

#if WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
#elif WINDOWS_APP
using Bing.Maps;
using Microsoft.Maps.SpatialToolbox.Bing;
#endif

namespace DataViewer
{
    public class MapView : Grid, INotifyPropertyChanged
    {
        #region Private Methods

        #if WINDOWS_APP
        private Map _map;
        private MapShapeLayer _shapeLayer;
        private MapLayer _pinLayer;
        #elif WINDOWS_PHONE_APP
        private MapControl _map;
        #endif

        #endregion

        #region Constructor

        public MapView()
        {           
            #if WINDOWS_APP
            _map = new Map();

            _shapeLayer = new MapShapeLayer();
            _map.ShapeLayers.Add(_shapeLayer);

            _pinLayer = new MapLayer();
            _map.Children.Add(_pinLayer);
            _map.PointerPressedOverride += _map_PointerPressedOverride;
            #elif WINDOWS_PHONE_APP
            _map = new MapControl();
            _map.PointerPressed += _map_PointerPressed;
            #endif

            this.Children.Add(_map);

            SetMapBindings();
        }

        #if WINDOWS_APP
        private void _map_PointerPressedOverride(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MapClicked != null)
            {
                Location l;
                _map.TryPixelToLocation(e.GetCurrentPoint(_map).Position, out l);
                Geopoint p = new Geopoint(new BasicGeoposition()
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude
                });
                MapClicked(p);
            }
        }
        #elif WINDOWS_PHONE_APP
        private void _map_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (MapClicked != null)
            {
                Geopoint p;
                _map.GetLocationFromOffset(e.GetCurrentPoint(_map).Position, out p);
                MapClicked(p);
            }
        }
        #endif
        #endregion

        #region Public Properties

        #if WINDOWS_APP
        public Map BaseMap { get { return _map; } }

        public MapShapeLayer ShapeLayer { get { return _shapeLayer; } }

        public MapLayer PinLayer { get { return _pinLayer; } }
        #elif WINDOWS_PHONE_APP
        public MapControl BaseMap { get { return _map; } }

        public IList<MapElement> ShapeLayer { get { return _map.MapElements; } }

        public IList<DependencyObject> PinLayer { get { return _map.Children; } }
        #endif

        public double Zoom
        {
            get
            {
                return _map.ZoomLevel;
            }
            set
            {
                _map.ZoomLevel = value;
                OnPropertyChanged("Zoom");
            }
        }

        public Geopoint Center
        {
            get
            {
                #if WINDOWS_APP
                return _map.Center.ToGeopoint();
                #elif WINDOWS_PHONE_APP
                return _map.Center;
                #endif
            }
            set
            {
                #if WINDOWS_APP
                _map.Center = value.ToLocation();
                #elif WINDOWS_PHONE_APP
                _map.Center = value;
                #endif
                OnPropertyChanged("Center");
            }
        }

        public double Heading
        {
            get
            {
                return _map.Heading;
            }
            set
            {
                _map.Heading = value;
                OnPropertyChanged("Heading");
            }
        }

        public string Credentials
        {
            get
            {
                #if WINDOWS_APP
                return _map.Credentials;
                #elif WINDOWS_PHONE_APP
                return string.Empty;
                #endif
            }
            set
            {
                #if WINDOWS_APP
                if (!string.IsNullOrEmpty(value))
                {
                    _map.Credentials = value;
                }
                #endif

                OnPropertyChanged("Credentials");
            }
        }

        public string MapServiceToken
        {
            get
            {
                #if WINDOWS_APP
                return string.Empty;
                #elif WINDOWS_PHONE_APP
                return _map.MapServiceToken;
                #endif
            }
            set
            {
                #if WINDOWS_PHONE_APP
                if (!string.IsNullOrEmpty(value))
                {
                    _map.MapServiceToken = value;
                }
                #endif

                OnPropertyChanged("MapServiceToken");
            }
        }

        public bool ShowTraffic
        {
            get
            {
                #if WINDOWS_APP
                return _map.ShowTraffic;
                #elif WINDOWS_PHONE_APP
                return _map.TrafficFlowVisible;
                #endif
            }
            set
            {
                #if WINDOWS_APP
                _map.ShowTraffic = value;
                #elif WINDOWS_PHONE_APP
                _map.TrafficFlowVisible = value;
                #endif
                OnPropertyChanged("ShowTraffic");
            }
        }

        public delegate void ViewChangeHandler(Geopoint center, double zoom, double heading);

        /// <summary>
        /// A callback method used to create a single Pushpin icon.
        /// </summary>
        public event ViewChangeHandler ViewChangedEnd;

        public delegate void MapClickHandler(Geopoint center);

        /// <summary>
        /// A callback method used to when the map is Clicked
        /// </summary>
        public event MapClickHandler MapClicked;

        #endregion

        #region Public Methods

        public void ClearMap()
        {
            #if WINDOWS_APP
            _pinLayer.Children.Clear();
            _shapeLayer.Shapes.Clear();
            #elif WINDOWS_PHONE_APP
            _map.MapElements.Clear();
            _map.Children.Clear();
            #endif
        }

        public void AddPushpin(BasicGeoposition location, string text)
        {
            #if WINDOWS_APP
            var pin = new Pushpin()
            {
                Text = text
            };
            MapLayer.SetPosition(pin, location.ToLocation());
            _pinLayer.Children.Add(pin);
            #elif WINDOWS_PHONE_APP
            var pin = new Grid()
            {
                Width = 24,
                Height = 24,
                Margin = new Windows.UI.Xaml.Thickness(-12)
            };

            pin.Children.Add(new Ellipse()
            {
                Fill = new SolidColorBrush(Colors.DodgerBlue),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 3,
                Width = 24,
                Height = 24
            });

            pin.Children.Add(new TextBlock()
            {
                Text = text,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center,
                VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center
            });

            MapControl.SetLocation(pin, new Geopoint(location));
            _map.Children.Add(pin);
            #endif
        }

        public void AddPolyline(List<BasicGeoposition> locations, Color strokeColor, double strokeThickness)
        {
            #if WINDOWS_APP
            var line = new MapPolyline()
            {
                Locations = locations.ToLocationCollection(),
                Color = strokeColor,
                Width = strokeThickness
            };

            _shapeLayer.Shapes.Add(line);
            #elif WINDOWS_PHONE_APP
            var line = new MapPolyline()
            {
                Path = new Geopath(locations),
                StrokeColor = strokeColor,
                StrokeThickness = strokeThickness
            };

            _map.MapElements.Add(line);
            #endif
        }

        public void AddPolygon(List<BasicGeoposition> locations, Color fillColor)
        {
            AddPolygon(locations, fillColor, Windows.UI.Color.FromArgb(0,0,0,0), 1);
        }

        public void AddPolygon(List<BasicGeoposition> locations, Color fillColor, Color strokeColor, double strokeThickness)
        {
            #if WINDOWS_APP
            var line = new MapPolygon()
            {
                Locations = locations.ToLocationCollection(),
                FillColor = fillColor
            };

            _shapeLayer.Shapes.Add(line);
            #elif WINDOWS_PHONE_APP
            var line = new MapPolygon()
            {
                Path = new Geopath(locations),
                FillColor = fillColor,
                StrokeColor = strokeColor,
                StrokeThickness = strokeThickness
            };

            _map.MapElements.Add(line);
            #endif
        }

        public void SetMapType(string type)
        {
            #if WINDOWS_APP
            switch (type)
            {
                case "aerial":
                case "aerialWithRoads":
                    _map.MapType = MapType.Aerial;
                    break;
                case "birdseye":
                    _map.MapType = MapType.Birdseye;
                    break;
                case "road":
                case "terrain":
                default:
                    _map.MapType = MapType.Road;
                    break;
            }
            #elif WINDOWS_PHONE_APP
            switch (type)
            {
                case "aerial":
                    _map.Style = MapStyle.Aerial;
                    break;
                case "aerialWithRoads":
                case "birdseye":
                    _map.Style = MapStyle.AerialWithRoads;
                    break;
                case "terrain":
                    _map.Style = MapStyle.Terrain;
                    break;
                case "road":
                default:
                    _map.Style = MapStyle.Terrain;
                    break;
            }
            #endif
        }

        public void SetView(BasicGeoposition center, double zoom)
        {
            #if WINDOWS_APP
            _map.SetView(center.ToLocation(), zoom);
            #elif WINDOWS_PHONE_APP
            _map.Center = new Geopoint(center);
            _map.ZoomLevel = zoom;
            #endif
        }

        ~MapView()
        {
            #if WINDOWS_APP
            _map.ViewChangeEnded -= _map_ViewChangeEnded;
            #elif WINDOWS_PHONE_APP
            _map.ZoomLevelChanged -= _map_ZoomLevelChanged;
            _map.CenterChanged -= _map_CenterChanged;
            _map.HeadingChanged -= _map_HeadingChanged;
            _map.PitchChanged -= _map_PitchChanged;
            #endif
        }

        #endregion

        #region Private Methods

        private void SetMapBindings()
        {
            #if WINDOWS_APP
            _map.ViewChangeEnded += _map_ViewChangeEnded;
            #elif WINDOWS_PHONE_APP
            _map.ZoomLevelChanged += _map_ZoomLevelChanged;
            _map.CenterChanged += _map_CenterChanged;
            _map.HeadingChanged += _map_HeadingChanged;
            _map.PitchChanged += _map_PitchChanged;
            #endif
        }

        private void _map_PitchChanged(object sender, object args)
        {
            CallViewChangedEndEvent();
        }

        private void _map_HeadingChanged(object sender, object args)
        {
            CallViewChangedEndEvent();
        }

        private void _map_CenterChanged(object sender, object args)
        {
            CallViewChangedEndEvent();
        }

        private void _map_ZoomLevelChanged(object sender, object args)
        {
            CallViewChangedEndEvent();
        }

        private void _map_ViewChangeEnded(object sender, object e)
        {
            CallViewChangedEndEvent();
        }

        private void CallViewChangedEndEvent()
        {
            if (ViewChangedEnd != null)
            {
                #if WINDOWS_APP
                ViewChangedEnd(_map.Center.ToGeopoint(), _map.ZoomLevel, _map.Heading);
                #elif WINDOWS_PHONE_APP
                ViewChangedEnd(_map.Center, _map.ZoomLevel, _map.Heading);
                #endif
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}