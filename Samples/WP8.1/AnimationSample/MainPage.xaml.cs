using Microsoft.Maps.SpatialToolbox.Bing.Animations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace AnimationSample
{
    public sealed partial class MainPage : Page
    {
        #region Private Properties

        private List<BasicGeoposition> path = new List<BasicGeoposition>(){
            new BasicGeoposition(){ Latitude = 42.8, Longitude = 12.49 },   //Italy
            new BasicGeoposition(){ Latitude = 51.5, Longitude = 0},       //London
            new BasicGeoposition(){ Latitude = 40.8, Longitude = -73.8},   //New York
            new BasicGeoposition(){ Latitude = 47.6, Longitude = -122.3}   //Seattle
        };

        private PathAnimation currentAnimation;

        #endregion

        #region Constructor

        public MainPage()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Button Handlers

        private void ClearMapBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();
        }

        private void DropPinBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();

            var pin = CreatePushpin();
            MapControl.SetLocation(pin, MyMap.Center);

            MyMap.Children.Add(pin);

            PushpinAnimations.Drop(pin, null, null);
        }

        private void BouncePinBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();

            var pin = CreatePushpin();
            MapControl.SetLocation(pin, MyMap.Center);

            MyMap.Children.Add(pin);

            PushpinAnimations.Bounce(pin, null, null);
        }

        private async void Bounce4PinsBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();

            for (var i = 0; i < path.Count; i++)
            {
                var pin = CreatePushpin();
                MapControl.SetLocation(pin, new Geopoint(path[i]));

                MyMap.Children.Add(pin);

                PushpinAnimations.Bounce(pin, null, null);

                await Task.Delay(500);
            }
        }

        private void MovePinOnPathBtn_Clicked(object sender, RoutedEventArgs e)
        {
            MovePinOnPath(false);
        }

        private void MovePinOnGeodesicPathBtn_Clicked(object sender, RoutedEventArgs e)
        {
            MovePinOnPath(true);
        }

        private void MoveMapOnPathBtn_Clicked(object sender, RoutedEventArgs e)
        {
            MoveMapOnPath(false);
        }

        private void MoveMapOnGeodesicPathBtn_Clicked(object sender, RoutedEventArgs e)
        {
            MoveMapOnPath(true);
        }

        private void DrawPathBtn_Clicked(object sender, RoutedEventArgs e)
        {
            DrawPath(false);
        }

        private void DrawGeodesicPathBtn_Clicked(object sender, RoutedEventArgs e)
        {
            DrawPath(true);
        }

        #endregion

        #region Private Methods

        private UIElement CreatePushpin()
        {
            return new Ellipse()
            {
                Width = 24,
                Height = 24,
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Colors.Blue),
                Stroke = new SolidColorBrush(Colors.White),
                Margin = new Thickness(-12, -12, 0, 0)
            };
        }
        
        private void ClearMap()
        {
            MyMap.Children.Clear();
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

            var pin = CreatePushpin();
            MapControl.SetLocation(pin, new Geopoint(path[0]));

            MyMap.Children.Add(pin);

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                MapControl.SetLocation(pin, new Geopoint(coord));
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        private async void MoveMapOnPath(bool isGeodesic)
        {
            ClearMap();

            //Change zooms levels as map reaches points along path.
            var zooms = new double[4] { 5, 4, 6, 5 };

            await MyMap.TrySetViewAsync(new Geopoint(path[0]), zooms[0], null, null, MapAnimationKind.None);

            currentAnimation = new PathAnimation(path, async (coord, pathIdx, frameIdx) =>
            {
                await MyMap.TrySetViewAsync(new Geopoint(coord), zooms[pathIdx], null, null, MapAnimationKind.None);
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        private void DrawPath(bool isGeodesic)
        {
            ClearMap();

            MapPolyline line = new MapPolyline()
            {
                StrokeColor = Colors.Red,
                StrokeThickness = 4
            };

            var pathSteps = new List<BasicGeoposition>() { path[0] };

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                pathSteps.Add(coord);
                line.Path = new Geopath(pathSteps);

                if (frameIdx == 1)
                {
                    MyMap.MapElements.Add(line);
                }
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        #endregion
    }
}
