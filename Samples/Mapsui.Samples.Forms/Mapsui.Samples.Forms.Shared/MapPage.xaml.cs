using System;
using System.IO;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Mapsui.UI.Forms;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Mapsui.UI;
using System.Linq;
using Mapsui.Layers;
using BruTile.MbTiles;
using SQLite;

namespace Mapsui.Samples.Forms
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MapPage : ContentPage
	{
        int markerNum = 1;
        Random rnd = new Random();
        Func<MapView, MapClickedEventArgs, bool> clicker;

        public MapPage ()
		{
            InitializeComponent();
        }

        public MapPage(Action<IMapControl> setup, Func<MapView, MapClickedEventArgs, bool> c = null)
        {
            InitializeComponent();

            Button1.Clicked += ButtonOnClick;

            mapView.RotationLock = false;
            mapView.UnSnapRotationDegrees = 30;
            mapView.ReSnapRotationDegrees = 5;

            mapView.PinClicked += OnPinClicked;
            mapView.MapClicked += OnMapClicked;

            mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position());

            StartGPS();

            setup(mapView);

            clicker = c;
        }

        private void OnMapClicked(object sender, MapClickedEventArgs e)
        {
            e.Handled = clicker != null ? (bool)clicker?.Invoke(sender as MapView, e) : false;
            //Samples.SetPins(mapView, e);
            //Samples.DrawPolylines(mapView, e);
        }

        private void OnPinClicked(object sender, PinClickedEventArgs e)
        {
            if (e.Pin != null)
            {
                if (e.NumOfTaps == 2)
                {
                    // Hide Pin when double click
                    //DisplayAlert($"Pin {e.Pin.Label}", $"Is at position {e.Pin.Position}", "Ok");
                    e.Pin.IsVisible = false;
                }
                if (e.NumOfTaps == 1)
                    e.Pin.IsCalloutVisible = !e.Pin.IsCalloutVisible;
            }

            e.Handled = true;
        }

        public async void StartGPS()
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            // Start GPS
            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1),
                    1,
                    true,
                    new ListenerSettings
                    {
                        ActivityType = ActivityType.Fitness,
                        AllowBackgroundUpdates = false,
                        DeferLocationUpdates = true,
                        DeferralDistanceMeters = 1,
                        DeferralTime = TimeSpan.FromSeconds(0.2),
                        ListenForSignificantChanges = false,
                        PauseLocationUpdatesAutomatically = true
                    });

            CrossGeolocator.Current.PositionChanged += MyLocationPositionChanged;
            CrossGeolocator.Current.PositionError += MyLocationPositionError;
        }

        public async void StopGPS()
        {
            // Stop GPS
            if (CrossGeolocator.Current.IsListening)
            {
                await CrossGeolocator.Current.StopListeningAsync();
            }
        }

        /// <summary>
        /// If there was an error while getting GPS coordinates
        /// </summary>
        /// <param name="sender">Geolocator</param>
        /// <param name="e">Event arguments for position error</param>
        private void MyLocationPositionError(object sender, PositionErrorEventArgs e)
        {
        }

        /// <summary>
        /// New informations from Geolocator arrived
        /// </summary>
        /// <param name="sender">Geolocator</param>
        /// <param name="e">Event arguments for new position</param>
        private void MyLocationPositionChanged(object sender, PositionEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var coords = new UI.Forms.Position(e.Position.Latitude, e.Position.Longitude);
                info.Text = $"{coords.ToString()} - D:{(int)e.Position.Heading} S:{Math.Round(e.Position.Speed, 2)}";
                
                mapView.MyLocationLayer.UpdateMyLocation(new UI.Forms.Position(e.Position.Latitude, e.Position.Longitude));
                mapView.MyLocationLayer.UpdateMyDirection(e.Position.Heading, mapView.Viewport.Rotation);
                mapView.MyLocationLayer.UpdateMySpeed(e.Position.Speed);
            });
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {

            //mapView.Map.Layers.Add(Utilities.OpenStreetMap.CreateTileLayer());
            //_mapControl.Map.Home = null;
            mapView.Map.Layers.Add(CreateMbTilesLayer(Path.Combine(MbTilesSample.MbTilesLocation, "world.mbtiles")));

            var mapResolutions = mapView.Map.Resolutions.ToList();
            //does not work, except I wait before this call a couple of seconds
            var resolution = (double)mapResolutions[(int)(mapResolutions.Count / 2)];
            mapView.Navigator.NavigateTo(new Geometries.Point(0, 0), resolution);

            //// Get the lon lat coordinates from somewhere (Mapsui can not help you there)
            //var centerOfLondonOntario = new Point(-81.2497, 42.9837);
            //// OSM uses spherical mercator coordinates. So transform the lon lat coordinates to spherical mercator
            //var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfLondonOntario.X, centerOfLondonOntario.Y);
            //// Set the center of the viewport to the coordinate. The UI will refresh automatically
            //// Additionally you might want to set the resolution, this could depend on your specific purpose
            //MapControl.Navigator.NavigateTo(sphericalMercatorCoordinate, 23);
        }

        public static TileLayer CreateMbTilesLayer(string path)
        {
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var mbTilesLayer = new TileLayer(mbTilesTileSource);
            return mbTilesLayer;
        }
    }
}