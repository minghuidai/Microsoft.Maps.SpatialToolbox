(function () {
    var map, mapTools, infobox, shapeLayer;

    function initialize() {
        Microsoft.Maps.loadModule('Microsoft.Maps.Map', { callback: GetMap });

        // Initialize WinJS controls.
        WinJS.UI.processAll();

        document.getElementById("WKTBtn").addEventListener("click", function (e) {
            ImportData_Tapped("wkt", [".txt"]);
        }, true);

        document.getElementById("ShapefileBtn").addEventListener("click", function (e) {
            ImportData_Tapped("shp", [".shp"]);
        }, true);

        document.getElementById("GeoRSSBtn").addEventListener("click", function (e) {
            ImportData_Tapped("georss", [".xml", ".rss"]);
        }, true);

        document.getElementById("GpxBtn").addEventListener("click", function (e) {
            ImportData_Tapped("gpx", [".xml", ".gpx"]);
        }, true);

        document.getElementById("KmlBtn").addEventListener("click", function (e) {
            ImportData_Tapped("kml", [".xml", ".kml", ".kmz"]);
        }, true);

        document.getElementById("GeoJSONBtn").addEventListener("click", function (e) {
            ImportData_Tapped("geojson", [".js", ".json"]);
        }, true);
    }

    function GetMap() {
        map = new Microsoft.Maps.Map(document.getElementById("MyMap"), {
            credentials: "YOUR_BING_MAPS_KEY"
        });

        map.getMode().setOptions({ drawShapesInSingleLayer: true });

        shapeLayer = new Microsoft.Maps.EntityCollection();
        map.entities.push(shapeLayer);

        Microsoft.Maps.loadModule('WKTModule');
        Microsoft.Maps.loadModule('CanvasPushpinModule', {
            callback: function () {
                //Create Canvas Entity Collection
                var pinLayer = new CanvasLayer(map);
                map.entities.push(pinLayer);

                var infoboxLayer = new Microsoft.Maps.EntityCollection();
                map.entities.push(infoboxLayer);

                infobox = new Microsoft.Maps.Infobox(new Microsoft.Maps.Location(0, 0), { visible: false, offset: new Microsoft.Maps.Point(0, 0) });
                infoboxLayer.push(infobox);
                
                Microsoft.Maps.loadModule('SpatialToolboxModule', {
                    callback: function () {
                        mapTools = new MapTools(map, pinLayer, shapeLayer)
                    }
                });
            }
        });        
    }

    function ImportData_Tapped(feedType, fileTypes) {
        mapTools.ClearLayers();

        var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
        openPicker.viewMode = Windows.Storage.Pickers.PickerViewMode.list;
        openPicker.suggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.desktop;
        
        var list = new WinJS.Binding.List(fileTypes);
        list.forEach(function(value, index, array){
            openPicker.fileTypeFilter.push(value);
        });

        openPicker.pickSingleFileAsync().done(
            function (file) {
                if (file != null) {
                    file.openReadAsync().then(function (stream) {
                        Microsoft.Maps.SpatialToolbox.Component.FeedService.readFeedStreamAsync(stream, feedType).then(function (data) {
                            mapTools.LoadFeedShapes(data, DisplayInfobox);
                        });
                    });
                }
            });
    }


    function DisplayInfobox(e) {
        var center = null;

        if (e.targetType == 'pushpin') {
            center = e.target.getLocation();
        } else if (e.target.getLocations) {
            var locs = e.target.getLocations();
            var avgLat = 0, avgLon = 0;

            for (var i = 0; i < locs.length; i++) {
                avgLat += locs[i].latitude;
                avgLon += locs[i].longitude;
            }

            avgLat /= locs.length;
            avgLon /= locs.length;

            center = new Microsoft.Maps.Location(avgLat, avgLon);
        }

        //Get title and description values
        var title = '', desc = '', hasData = false;
        if (e.target.Title && e.target.Title != '') {
            title = e.target.Title;
            hasData = true;
        } else if (e.target.Metadata && e.target.Metadata.length > 0) {
            for (var i = 0; i < e.target.Metadata.length; i++) {
                if (e.target.Metadata[i].Key.toLowerCase() == 'id' || e.target.Metadata[i].Key.toLowerCase() == 'name') {
                    title = e.target.Metadata[i].Value;
                    break;
                }
            }
            hasData = true;
        }

        if (e.target.Description && e.target.Description != '') {
            desc = e.target.Description;
            hasData = true;
        }

        if (center && hasData) {
            infobox.setLocation(center);
            infobox.setOptions({ visible: true, title: title, description: desc });
        }
    }

    document.addEventListener("DOMContentLoaded", initialize, false);
})();