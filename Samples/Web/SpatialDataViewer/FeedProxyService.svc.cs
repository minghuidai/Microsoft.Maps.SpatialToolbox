using Microsoft.Maps.SpatialToolbox;
using Microsoft.Maps.SpatialToolbox.Component;
using Microsoft.Maps.SpatialToolbox.IO;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace SpatialDataViewer
{
    [ServiceContract()]
    [ServiceKnownType(typeof(FeedResponse))]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FeedProxyService
    {
        /// <summary>
        /// Downloads a spatial data feed and extracts it as a spatial data set.
        /// </summary>
        /// <param name="feedUrl">An absolute Url to a feed to read.</param>
        /// <param name="feedType">Type of feed to get.</param>
        /// <returns>A spatial data set extracted from a feed.</returns>
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        public async Task<FeedResponse> GetFeed(string feedUrl, string feedType)
        {
            var response = new FeedResponse();

            try
            {
                var feed = GetBaseFeed(feedType);

                if (feed != null)
                {
                    using (var stream = await ServiceHelper.GetStreamAsync(new Uri(feedUrl)))
                    {
                        var data = await feed.ReadAsync(stream);

                        if (data != null && data.Geometries != null && data.Geometries.Count > 0)
                        {
                            response.Shapes = new List<FeedShape>();

                            FeedShapeStyle style;

                            foreach (var g in data.Geometries)
                            {
                                style = null;

                                if (!string.IsNullOrEmpty(g.StyleKey) && data.Styles != null && data.Styles.ContainsKey(g.StyleKey))
                                {
                                    style = ConvertStyle(data.Styles[g.StyleKey]);
                                }

                                var s = new FeedShape(g.ToString())
                                {
                                    Style = style
                                };

                                if (g.Metadata != null)
                                {
                                    s.Title = g.Metadata.Title;
                                    s.Description = g.Metadata.Description;
                                    s.Metadata = g.Metadata.Properties;
                                }

                                response.Shapes.Add(s);
                            }

                            if (data.BoundingBox != null)
                            {
                                response.BoundingBox = string.Format("{0},{1},{2},{3}",
                                    data.BoundingBox.Center.Latitude,
                                    data.BoundingBox.Center.Longitude,
                                    data.BoundingBox.Width,
                                    data.BoundingBox.Height);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return response;
        }

        #region Private Methods

        private static BaseFeed GetBaseFeed(string feedType)
        {
            switch (feedType.ToLowerInvariant())
            {
                case "georss":
                    return new GeoRssFeed();
                case "gpx":
                    return new GpxFeed();
                case "kml":
                    return new KmlFeed();
                case "shp":
                    return new ShapefileReader();
                case "wkt":
                    return new WellKnownText();
                case "wkb":
                    return new WellKnownBinary();
                case "geojson":
                    return new GeoJsonFeed();
                default:
                    break;
            }

            return null;
        }

        private static FeedShapeStyle ConvertStyle(ShapeStyle style)
        {
            if (style == null)
            {
                return null;
            }

            var s = new FeedShapeStyle()
            {
                FillPolygon = style.FillPolygon,
                IconHeading = style.IconHeading,
                IconScale = style.IconScale,
                IconUrl = style.IconUrl,
                OutlinePolygon = style.OutlinePolygon,
                StrokeThickness = style.StrokeThickness
            };

            if (style.FillColor.HasValue)
            {
                s.FillColor = style.FillColor.Value.ToString();
            }

            if (style.IconColor.HasValue)
            {
                s.IconColor = style.IconColor.Value.ToString();
            }

            if (style.StrokeColor.HasValue)
            {
                s.StrokeColor = style.StrokeColor.Value.ToString();
            }

            return s;
        }

        #endregion
    }
}
