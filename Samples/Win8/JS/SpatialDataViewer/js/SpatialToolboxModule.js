var MapTools = function (map, pinLayer, shapeLayer) {
    var defaultIconStyle = {
        iconColor: "rgba(0,200,255,1)",
        iconHeading: 0,
        iconScale: 1
    };

    /*****************
    * Public Methods
    ******************/

    this.ClearLayers = function () {
        pinLayer.clear();
        shapeLayer.clear();
    };

    this.LoadFeedShapes = function (data, tapEvent) {
        if (data.error) {
            if (typeof Windows != "undefined" &&
                Windows.UI != null &&
                Windows.UI.Popups != null) {
                var popup = Windows.UI.Popups.MessageDialog(data.error);
                popup.showAsync();
            } else {
                alert(data.error);
            }
        } else if (data.shapes && data.shapes.length > 0) {
            for (var i = 0; i < data.shapes.length; i++) {
                AddShapeToLayer(data.shapes[i], tapEvent);
            }

            if (data.boundingBox) {
                var b = data.boundingBox.split(',');
                if (b.length >= 4) {
                    var center = new Microsoft.Maps.Location(parseFloat(b[0]), parseFloat(b[1]));
                    var bounds = new Microsoft.Maps.LocationRect(center, parseFloat(b[2]), parseFloat(b[3]));
                    map.setView({ bounds: bounds });
                }
            }
        }
    };

    /*****************
    * Private Methods
    ******************/

    function AddShapeToLayer(shape, tapEvent) {
        var s = WKTModule.Read(shape.wkt);

        s.Title = shape.title;
        s.Description = shape.description;
        s.Metadata = shape.metadata;

        //Check to see if shape is a pushpin
        if (s.getIcon) {
            //Use canvas pushpins to add support for additional styles: iconColor, iconHeading, iconScale
            pinLayer.push(CreateCanvasIconPushin(s, shape.style, tapEvent));
            return;
        }

        var style = {};

        if (shape.style) {
            if (shape.style.outlinePolygon) {
                style.strokeThickness = shape.style.strokeThickness;

                if (shape.style.strokeColor) {
                    style.strokeColor = ConvertRgbaToMsftColor(shape.style.strokeColor);
                }
            } else {
                style.strokeThickness = 0;
            }

            if (shape.style.fillPolygon) {
                if (shape.style.fillColor) {
                    style.fillColor = ConvertRgbaToMsftColor(shape.style.fillColor);
                }
            } else {
                style.fillColor = new Microsoft.Maps.Color(0, 0, 0, 0);
            }
        }

        s.setOptions(style);

        //Check to see if shape is a collection of shapes.
        if (s.constructor == Microsoft.Maps.EntityCollection && s.getLength) {
            for (var i = 0; i < s.getLength() ; i++) {
                var t = s.get(i);

                if (t.getIcon) { //Only pushpins have this property.
                    pinLayer(CreateCanvasIconPushin(t, style, tapEvent));
                } else {
                    t.setOptions(style);

                    if (tapEvent) {
                        Microsoft.Maps.Events.addHandler(t, 'click', tapEvent);
                    }

                    shapeLayer.push(t);
                }
            }
        } else {
            if (tapEvent) {
                Microsoft.Maps.Events.addHandler(s, 'click', tapEvent);
            }

            if (s.getIcon) { //Only pushpins have this property.else{
                pinLayer.push(s);
            } else {
                shapeLayer.push(s);
            }            
        }
    }

    function CreateCanvasIconPushin(shape, style, tapEvent) {
        var pin = new CanvasPushpin(shape.getLocation(), function (pin, context, setSize) {
            if (pin.Style && pin.Style.iconUrl) {
                var img = new Image();
                img.onload = function () {
                    var width = img.width;
                    var height = img.height;

                    if (context) {
                        //Set the dimensions of the canvas
                        context.canvas.width = width;
                        context.canvas.height = height;
                        context.width = img.width;
                        context.height = img.height;

                        var heading = pin.Style.iconHeading;
                        var scale = pin.Style.iconScale;

                        if (heading != 0 || scale != 1) {
                            //Calculate the new dimensions of the the canvas after the image is rotated
                            var dx = Math.abs(Math.cos(heading * Math.PI / 180));
                            var dy = Math.abs(Math.sin(heading * Math.PI / 180));
                            width = Math.round((img.width * dx + img.height * dy) * scale);
                            height = Math.round((img.width * dy + img.height * dx) * scale);

                            //Set the dimensions of the canvas
                            context.canvas.width = width;
                            context.canvas.height = height;
                            context.width = img.width;
                            context.height = img.height;

                            //Offset the canvas such that we will rotate around the center of our arrow
                            context.translate(width * 0.5, height * 0.5);

                            //Rotate the canvas by the desired heading
                            context.rotate(heading * Math.PI / 180);

                            //Return the canvas offset back to it's original position
                            context.translate(-img.width * 0.5 * scale, -img.height * 0.5 * scale);
                        }

                        if (setSize) {
                            pin._setSize(width, height);
                            return;
                        }

                        //Draw the pushpin icon
                        context.drawImage(img, 0, 0, width, height);
                    }
                };

                img.onerror = function (e) {
                    var scale = defaultIconStyle.iconScale;

                    if (setSize) {
                        pin._setSize(34 * scale, 34 * scale);
                        return;
                    }

                    //Set the dimensions of the canvas
                    context.canvas.width = 34 * scale;
                    context.canvas.height = 34 * scale;
                    context.width = 34 * scale;
                    context.height = 34 * scale;

                    //Draw a colored circle behind the pin
                    context.beginPath();
                    context.arc(17 * scale, 17 * scale, 14 * scale, 0, 2 * Math.PI, false);
                    context.fillStyle = defaultIconStyle.iconColor;
                    context.fill();
                    context.lineWidth = 3 * scale;
                    context.strokeStyle = "white";
                    context.stroke();
                };

                if (pin.Style.iconUrl.absoluteUri) {
                    img.src = pin.Style.iconUrl.absoluteUri;
                } else {
                    img.src = pin.Style.iconUrl;
                }

            } else if (context) {
                var scale = (pin.Style) ? pin.Style.iconScale : 1;

                if (setSize) {
                    pin._setSize(34 * scale, 34 * scale);
                    return;
                }

                //Set the dimensions of the canvas
                context.canvas.width = 34 * scale;
                context.canvas.height = 34 * scale;
                context.width = 34 * scale;
                context.height = 34 * scale;

                //Draw a colored circle behind the pin
                context.beginPath();
                context.arc(17 * scale, 17 * scale, 14 * scale, 0, 2 * Math.PI, false);
                context.fillStyle = (pin.Style && pin.Style.iconColor) ? pin.Style.iconColor : defaultIconStyle.iconColor;
                context.fill();
                context.lineWidth = 3 * scale;
                context.strokeStyle = "white";
                context.stroke();
            }
        });

        pin.Style = style;
        pin.Title = shape.Title;
        pin.Description = shape.Description;
        pin.Metadata = shape.Metadata;

        if (tapEvent) {
            Microsoft.Maps.Events.addHandler(pin, 'click', tapEvent);
        }

        return pin;
    }

    function ConvertRgbaToMsftColor(rgba) {
        var v = rgba.replace('rgba(', '').replace(')', '').split(',');
        if (v.length >= 4) {
            return new Microsoft.Maps.Color(parseFloat(v[3]) * 255, parseFloat(v[0]), parseFloat(v[1]), parseFloat(v[2]))
        }

        return null;
    }
};

Microsoft.Maps.moduleLoaded('SpatialToolboxModule');