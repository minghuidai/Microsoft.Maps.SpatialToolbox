Imports Bing.Maps
Imports Microsoft.Maps.SpatialToolbox
Imports Microsoft.Maps.SpatialToolbox.Bing
Imports Microsoft.Maps.SpatialToolbox.IO
Imports Windows.Storage.Pickers

Public NotInheritable Class MainPage
    Inherits Page

#Region "Private Properties"

    Private PolyLayer As MapShapeLayer
    Private DefaultStyle As ShapeStyle

#End Region

#Region "Consturctor"

    Public Sub New()
        InitializeComponent()

        'Add a MapShapeLayer to map for polylines and polygons.
        PolyLayer = New MapShapeLayer()
        MyMap.ShapeLayers.Add(PolyLayer)

        'Define Default style
        DefaultStyle = New ShapeStyle()
        DefaultStyle.FillColor = StyleColor.FromArgb(150, 0, 0, 255)
        DefaultStyle.StrokeColor = StyleColor.FromArgb(150, 0, 0, 0)
        DefaultStyle.StrokeThickness = 4
    End Sub

#End Region

#Region "Button Handlers"

    Private Async Sub ImportData_Tapped(sender As Object, e As TappedRoutedEventArgs)
        Try
            Dim selectedItem = TryCast(sender, Windows.UI.Xaml.Controls.MenuFlyoutItem)

            Dim reader As BaseFeed
            reader = Nothing

            Dim fileTypes As String()
            fileTypes = Nothing

            'Check to see if the Tag property of the selected item has a string that is a spatial file type.
            If (selectedItem IsNot Nothing) Then
                Select Case selectedItem.Tag.ToString()
                    Case "wkt"
                        reader = New WellKnownText()
                        fileTypes = New String() {".txt"}
                        Exit Select
                    Case "shp"
                        reader = New ShapefileReader()
                        fileTypes = New String() {".shp"}
                        Exit Select
                    Case "gpx"
                        reader = New GpxFeed()
                        fileTypes = New String() {".xml", ".gpx"}
                        Exit Select
                    Case "georss"
                        reader = New GeoRssFeed()
                        fileTypes = New String() {".xml", ".rss"}
                        Exit Select
                    Case "kml"
                        reader = New KmlFeed()
                        fileTypes = New String() {".xml", ".kml", ".kmz"}
                        Exit Select
                    Case "geojson"
                        reader = New GeoJsonFeed()
                        fileTypes = New String() {".js", ".json"}
                        Exit Select
                End Select
            End If

            If (reader IsNot Nothing AndAlso fileTypes IsNot Nothing) Then
                ClearMap()

                'Create a FileOpenPicker to allow the user to select which file to import
                Dim openPicker = New FileOpenPicker()
                openPicker.ViewMode = PickerViewMode.List
                openPicker.SuggestedStartLocation = PickerLocationId.Desktop

                'Add the allowed file extensions to the FileTypeFilter
                For Each type As String In fileTypes
                    openPicker.FileTypeFilter.Add(type)
                Next

                'Get the selected file
                Dim file = Await openPicker.PickSingleFileAsync()
                If (file IsNot Nothing) Then
                    Using fileStream As Stream = Await file.OpenStreamForReadAsync()
                        'Read the spatial data file
                        Dim data = Await reader.ReadAsync(fileStream)

                        If (String.IsNullOrEmpty(data.Error)) Then
                            'Load the spatial data set into the map
                            MapTools.LoadGeometries(data, PinLayer, PolyLayer, DefaultStyle, AddressOf DisplayInfobox)

                            'If the data set has a bounding box defined, use it to set the map view.
                            If (data.BoundingBox IsNot Nothing) Then
                                MyMap.SetView(data.BoundingBox.ToBMGeometry())
                            End If
                        Else
                            'If there is an error message, display it to the user.
                            Dim msg = New Windows.UI.Popups.MessageDialog(data.Error)
                            Await msg.ShowAsync()
                        End If
                    End Using
                End If
            End If
        Catch
        End Try
    End Sub

    Private Sub CloseInfobox_Tapped(sender As Object, e As TappedRoutedEventArgs)
        InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed
    End Sub

#End Region

#Region "Private Methods"

    Private Sub ClearMap()
        PinLayer.Children.Clear()
        PolyLayer.Shapes.Clear()
    End Sub

    Private Sub DisplayInfobox(sender As Object, e As TappedRoutedEventArgs)
        Dim tagMetadata As ShapeMetadata = Nothing
        Dim anchor As Location = Nothing

        If TypeOf sender Is FrameworkElement Then
            Dim pin = TryCast(sender, Windows.UI.Xaml.FrameworkElement)
            anchor = MapLayer.GetPosition(pin)

            'Get the stored metadata from the Tag property
            tagMetadata = TryCast(pin.Tag, ShapeMetadata)
        ElseIf TypeOf sender Is MapShape Then
            Dim shape = TryCast(sender, MapShape)

            tagMetadata = TryCast(shape.GetValue(MapShapeExt.TagProperty), ShapeMetadata)

            anchor = shape.ToGeometry().Envelope().Center.ToBMGeometry()
        End If

        If anchor IsNot Nothing AndAlso tagMetadata IsNot Nothing Then
            Dim hasContent As Boolean = False

            If Not String.IsNullOrWhiteSpace(tagMetadata.Title) Then
                'Set the Infobox title and description using the Metadata information
                InfoboxTitle.Text = tagMetadata.Title
                hasContent = True
            ElseIf Not String.IsNullOrWhiteSpace(tagMetadata.ID) Then
                'Set the Infobox title as the ID
                InfoboxTitle.Text = tagMetadata.ID
                hasContent = True
            End If


            If Not String.IsNullOrWhiteSpace(tagMetadata.Description) Then
                'Since the description value is being passed to a WebView, use the NavigateToString method to render the HTML.
                InfoboxDescription.NavigateToString(tagMetadata.Description)
                InfoboxDescription.Visibility = Windows.UI.Xaml.Visibility.Visible
                hasContent = True
            Else
                InfoboxDescription.Visibility = Windows.UI.Xaml.Visibility.Collapsed
            End If

            If hasContent Then
                'Display the infobox
                InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Visible
            Else
                InfoboxLayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed
            End If

            'Set the position of the infobox
            MapLayer.SetPosition(Infobox, anchor)
        End If
    End Sub
#End Region

End Class
