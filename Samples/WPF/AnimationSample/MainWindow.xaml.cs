using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.SpatialToolbox.Bing.Animations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace AnimationSample
{
    public partial class MainWindow : Window
    {
        #region Private Properties

        private LocationCollection path = new LocationCollection(){
            new Location(42.8, 12.49),   //Italy
            new Location(51.5, 0),       //London
            new Location(40.8, -73.8),   //New York
            new Location(47.6, -122.3)   //Seattle
        };

        private PathAnimation currentAnimation;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
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

            var pin = new Pushpin();
            MapLayer.SetPosition(pin, MyMap.Center);

            MyMap.Children.Add(pin);

            PushpinAnimations.Drop(pin, null, null);
        }

        private void BouncePinBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();

            var pin = new Pushpin();
            MapLayer.SetPosition(pin, MyMap.Center);

            MyMap.Children.Add(pin);

            PushpinAnimations.Bounce(pin, null, null);
        }

        private async void Bounce4PinsBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ClearMap();

            for (var i = 0; i < path.Count; i++)
            {
                var pin = new Pushpin();
                MapLayer.SetPosition(pin, path[i]);

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

        private void ClearMap()
        {
            MyMap.Children.Clear();

            if (currentAnimation != null)
            {
                currentAnimation.Stop();
                currentAnimation = null;
            }
        }

        private void MovePinOnPath(bool isGeodesic)
        {
            ClearMap();

            var pin = new Pushpin();
            MapLayer.SetPosition(pin, path[0]);

            MyMap.Children.Add(pin);

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                MapLayer.SetPosition(pin, coord);
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
                MyMap.SetView(coord, zooms[pathIdx]);
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        private void DrawPath(bool isGeodesic)
        {
            ClearMap();

            MapPolyline line = new MapPolyline()
            {
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 4
            };

            currentAnimation = new PathAnimation(path, (coord, pathIdx, frameIdx) =>
            {
                if (frameIdx == 1)
                {
                    //Create the line after the first frame so that we have two points to work with.	                
                    line.Locations = new LocationCollection() { path[0], coord };
                    MyMap.Children.Add(line);
                }
                else if (frameIdx > 1)
                {
                    line.Locations.Add(coord);
                }
            }, isGeodesic, 10000);

            currentAnimation.Play();
        }

        #endregion
    }
}
