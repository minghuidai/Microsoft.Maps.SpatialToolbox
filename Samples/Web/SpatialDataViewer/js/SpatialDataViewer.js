var map, mapTools, infobox, shapeLayer;
var baseUrl = "/FeedProxyService.svc/GetFeed?feedUrl={feedUrl}&feedType={feedType}";

function GetMap() {
    // Initialize the map
    map = new Microsoft.Maps.Map(document.getElementById("myMap"),
    {
        credentials: "YOUR_BING_MAPS_KEY"
    });

    map.getMode().setOptions({ drawShapesInSingleLayer: true });

    shapeLayer = new Microsoft.Maps.EntityCollection();
    map.entities.push(shapeLayer);

    //Register modules
    Microsoft.Maps.registerModule("WKTModule", "js/WKTModule.js");
    Microsoft.Maps.registerModule("CanvasPushpinModule", "js/CanvasPushpinModule.js");
    Microsoft.Maps.registerModule("SpatialToolboxModule", "js/SpatialToolboxModule.js");
    Microsoft.Maps.loadModule("WKTModule", {
        callback: function () {
            Microsoft.Maps.loadModule("CanvasPushpinModule", {
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
    });
}

function ImportData(feed, feedType) {
    mapTools.ClearLayers();

    var url = baseUrl.replace(/{feedUrl}/gi, escape(feed)).replace(/{feedType}/gi, feedType);

    var xmlHttp;
    if (window.XMLHttpRequest) {
        xmlHttp = new XMLHttpRequest();
    } else if (window.ActiveXObject) {
        try {
            xmlHttp = new ActiveXObject("Msxml2.XMLHTTP");
        } catch (e) {
            try {
                xmlHttp = new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e) {
                throw (e);
            }
        }
    }

    xmlHttp.open("GET", url, false);

    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4) {
            var data = eval('(' + xmlHttp.responseText + ')');
            mapTools.LoadFeedShapes(data, DisplayInfobox);
        }
    }

    xmlHttp.send();
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

    var title = '', desc = '', hasData = false;
    if(e.target.Title && e.target.Title != ''){
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

    if(e.target.Description && e.target.Description != ''){
        desc = e.target.Description;
        hasData = true;
    }

    if (center && hasData) {
        infobox.setLocation(center);
        infobox.setOptions({ visible: true, title: title, description: desc });
    }
}