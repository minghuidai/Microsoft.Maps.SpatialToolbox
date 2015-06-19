using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Bing;
using Microsoft.Maps.SpatialToolbox.IO;
using Microsoft.Maps.MapControl.WPF;

namespace ReadKmlTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region  Data


        private MapLayer mPolyLayer;
        private MapLayer mPinLayer;
        private ShapeStyle mDefaultStyle = new ShapeStyle()
        {
            FillColor = new StyleColor() { A = 150, B = 255 },
            StrokeColor = new StyleColor() { A = 150 },
            StrokeThickness = 4
        };

       

        #endregion



        public MainWindow()
        {
            InitializeComponent();

            mPolyLayer = new MapLayer();
            mPinLayer = new MapLayer();
            MyMap.Children.Add(mPolyLayer);
            MyMap.Children.Add(mPinLayer);

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ImportKML();
        }



        private void ClearMap()
        {
            mPinLayer.Children.Clear();
            mPolyLayer.Children.Clear();

        }


        private async void ImportKML()
        {

            ClearMap();


            Microsoft.Maps.SpatialToolbox.IO.KmlFeed reader = new Microsoft.Maps.SpatialToolbox.IO.KmlFeed();


            var kmltext = System.IO.File.ReadAllText(@"G:\Temp\SpPoly_finalmesh_FL.kml");

            // read spatial data string
            var data = await reader.ReadAsync(kmltext);


            if (string.IsNullOrEmpty(data.Error))
            {
                MapTools.LoadGeometries(data, mPinLayer, mPolyLayer, mDefaultStyle, DisplayInfobox);

                MyMap.SetView(data.BoundingBox.ToBMGeometry());

                //mBoundingBox = data.BoundingBox;
                //mRight = mBoundingBox.Center.Latitude;
               // MessageBox.Show("no error");
            }


        }



        private void DisplayInfobox(object sender, RoutedEventArgs e)
        {

        }


    }
}
