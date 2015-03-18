using Microsoft.Maps.SpatialToolbox.Bing.Animations;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Controls;
using System;
using System.Device.Location;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnimationSample
{
    public partial class MainPage : PhoneApplicationPage
    {        
        #region Private Properties

        private GeoCoordinateCollection path = new GeoCoordinateCollection(){
            new GeoCoordinate(42.8, 12.49),   //Italy
            new GeoCoordinate(51.5, 0),       //London
            new GeoCoordinate(40.8, -73.8),   //New York
            new GeoCoordinate(47.6, -122.3)   //Seattle
        };

        private PathAnimation currentAnimation;

        private MapLayer dataLayer;

        #endregion

        #region Constructor

        public MainPage()
        {
            InitializeComponent();

            dataLayer = new MapLayer();
            MyMap.Layers.Add(dataLayer);
        }

        #endregion

        #region Button Handlers

        private void ClearMapBtn_Clicked(object sender, EventArgs e)
        {
            ClearMap();
        }

        private void DropPinBtn_Clicked(object sender, EventArgs e)
        {
            ClearMap();

            var pin = CreatePushpin(MyMap.Center);
            dataLayer.Add(pin);

            PushpinAnimations.Drop(pin.Content as UIElement, null, null);
        }

        private void BouncePinBtn_Clicked(object sender, EventArgs e)
        {
            ClearMap();

            var pin = CreatePushpin(MyMap.Center);
            dataLayer.Add(pin);

            PushpinAnimations.Bounce(pin.Content as UIElement, null, null);
        }

        private async void Bounce4PinsBtn_Clicked(object sender, EventArgs e)
        {
            ClearMap();

            for (var i = 0; i < path.Count; i++)
            {
                var pin = CreatePushpin(path[i]);
                dataLayer.Add(pin);

                PushpinAnimations.Bounce(pin.Content as UIElement, null, null);

                await Task.Delay(500);
            }
        }

        private void MovePinOnPathBtn_Clicked(object sender, EventArgs e)
        {
            MovePinOnPath(false);
        }

        private void MovePinOnGeodesicPathBtn_Clicked(object sender, EventArgs e)
        {
            MovePinOnPath(true);
        }

        private void MoveMapOnPathBtn_Clicked(object sender, EventArgs e)
        {
            MoveMapOnPath(false);
        }

        private void MoveMapOnGeodesicPathBtn_Clicked(object sender, EventArgs e)
        {
            MoveMapOnPath(true);
        }

        private void DrawPathBtn_Clicked(object sender, EventArgs e)
        {
            DrawPath(false);
        }

        private void DrawGeodesicPathBtn_Clicked(object sender, EventArgs e)
        {
            DrawPath(true);
        }

        #endregion

        #region Private Methods

        private MapOverlay CreatePushpin(GeoCoordinate loc)
        {
            return new MapOverlay()
            {
                Content = new Ellipse()
                {
                    Width = 24,
                    Height = 24,
                    StrokeThickness = 2,
                    Fill = new SolidColorBrush(Colors.Blue),
                    Stroke = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(-12, -12, 0, 0)
                },
                GeoCoordinate = loc
            };
        }

        private void ClearMap()
        {
            dataLayer.Clear();
            MyMap.MapElements.Clear();

            if (currentAnimation != null)
            {
                currentAnimation.Stop();
                currentAnimation = null;
            }
        }

        private void MovePinOnPath(bool isGeodesic)
        {
            ClearMap();

            var pin = CreatePushpin(path[0]);
            dataLayer.Add(pin);

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                pin.GeoCoordinate = coord;
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        private void MoveMapOnPath(bool isGeodesic)
        {
            ClearMap();

            //Change zooms levels as map reaches points along path.
            int[] zooms = new int[4] { 5, 4, 6, 5 };

            MyMap.SetView(path[0], zooms[0]);

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                MyMap.SetView(coord, zooms[pathIdx], MapAnimationKind.None);
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        private void DrawPath(bool isGeodesic)
        {
            ClearMap();

            var line = new MapPolyline()
            {
                StrokeColor = Colors.Red,
                StrokeThickness = 4
            };

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                if (frameIdx == 1)
                {
                    //Create the line after the first frame so that we have two points to work with.	                
                    line.Path = new GeoCoordinateCollection() { path[0], coord };
                    MyMap.MapElements.Add(line);
                }
                else if (frameIdx > 1)
                {
                    line.Path.Add(coord);
                }
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        #endregion
    }
}