using System;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using System.Net;
using GoogleMapsApi.Entities.Elevation.Request;
using GoogleMapsApi.Entities.Elevation.Response;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET;
using System.Threading.Tasks;
using System.Device.Location;
using System.Globalization;
using System.Collections.Generic;

namespace PokemonGo.RocketAPI.Console
{
    public partial class LocationSelect : Form
    {
        private GMarkerGoogle _botMarker = new GMarkerGoogle(new PointLatLng(), GMarkerGoogleType.red_small);
        private GMapRoute _botRoute = new GMapRoute("BotRoute");
        private GMapOverlay _pokeStopsOverlay = new GMapOverlay("PokeStops");
        public double alt;
        public bool close = true;

        public LocationSelect(bool asViewOnly)
        {
            InitializeComponent();
            map.Manager.Mode = AccessMode.ServerOnly;

            if (asViewOnly)
                initViewOnly();
        }

        private void initViewOnly()
        {
            //first hide all controls
            foreach (Control c in Controls)
                c.Visible = false;
            //show map
            map.Visible = true;
            map.Dock = DockStyle.Fill;
            map.ShowCenter = false;
            GMapOverlay routeOverlay = new GMapOverlay();
            routeOverlay.Routes.Add(_botRoute);
            routeOverlay.Markers.Add(_botMarker);
            GMarkerGoogle _botStartMarker = new GMarkerGoogle(new PointLatLng(), GMarkerGoogleType.blue_dot);
            _botStartMarker.Position = new PointLatLng(Globals.latitute, Globals.longitude);
            routeOverlay.Markers.Add(_botStartMarker);
            GMapPolygon circle = CreateCircle(new PointLatLng(Globals.latitute, Globals.longitude), Globals.radius, 100);
            routeOverlay.Polygons.Add(circle);
            map.Overlays.Add(routeOverlay);
            map.Overlays.Add(_pokeStopsOverlay);
            //show geodata controls
            label1.Visible = true;
            label2.Visible = true;
            textBox1.Visible = true;
            textBox2.Visible = true;
            cbShowPokeStops.Visible = true;
            //don't ask at closing
            close = false;
            //add & remove live data handler after form loaded
            Globals.infoObservable.HandleNewGeoLocations += handleLiveGeoLocations;
            Globals.infoObservable.HandlePokeStop += InfoObservable_HandlePokeStop; ;

            this.FormClosing += (object s, FormClosingEventArgs e) =>
            {                
                Globals.infoObservable.HandleNewGeoLocations -= handleLiveGeoLocations;
            };
        }

        private void InfoObservable_HandlePokeStop(POGOProtos.Map.Fort.FortData[] pokeStops)
        {
            _pokeStopsOverlay.Markers.Clear();
            foreach (var pokeStop in pokeStops) {
                GMarkerGoogle pokeStopMaker = new GMarkerGoogle(new PointLatLng(pokeStop.Latitude, pokeStop.Longitude), GMarkerGoogleType.purple_small);
                _pokeStopsOverlay.Markers.Add(pokeStopMaker);
            }
        }

        private GMapPolygon CreateCircle(PointLatLng point, double radius, int segments)
        {
            radius /= 100000;
            List<PointLatLng> gpollist = new List<PointLatLng>();
            double seg = Math.PI * 2 / segments;
            for (int i = 0; i < segments; i++)
            {
                double theta = seg * i;
                double a = point.Lat + Math.Cos(theta) * radius * 0.75;
                double b = point.Lng + Math.Sin(theta) * radius;
                gpollist.Add(new PointLatLng(a, b));
            }
            GMapPolygon circle = new GMapPolygon(gpollist, "BotZone");
            circle.Stroke = System.Drawing.Pens.Black;
            circle.Fill = System.Drawing.Brushes.Transparent;
            return circle;
        }

        private void handleLiveGeoLocations(GeoCoordinate coords)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                textBox1.Text = coords.Latitude.ToString();
                textBox2.Text = coords.Longitude.ToString();
                PointLatLng newPosition = new PointLatLng(coords.Latitude, coords.Longitude);
                _botMarker.Position = newPosition;
                _botRoute.Points.Add(newPosition);
                map.Position = newPosition;
            }));
        }

        private void map_Load(object sender, EventArgs e)
        {
            showMap();
        }

        private void showMap()
        {
            try
            {
                map.DragButton = MouseButtons.Left;
                map.MapProvider = GMapProviders.BingMap;
                map.Position = new GMap.NET.PointLatLng(Globals.latitute, Globals.longitude);
                map.MinZoom = 0;
                map.MaxZoom = 20;
                map.Zoom = 16;

                textBox1.Text = Globals.latitute.ToString();
                textBox2.Text = Globals.longitude.ToString();
                textBox3.Text = Globals.altitude.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void map_OnMapDrag()
        {
            Task.Run(() =>
            {
                var elevationRequest = new ElevationRequest()
                {
                    Locations = new[] { new Location(map.Position.Lat, map.Position.Lng) },
                };
                try
                {
                    ElevationResponse elevation = GoogleMaps.Elevation.Query(elevationRequest);
                    if (elevation.Status == Status.OK)
                    {
                        foreach (Result result in elevation.Results)
                        {
                            SetText(result.Elevation);
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });

            textBox1.Text = map.Position.Lat.ToString(CultureInfo.InvariantCulture);
            textBox2.Text = map.Position.Lng.ToString(CultureInfo.InvariantCulture);
        }

        delegate void SetTextCallback(double cord);

        private void SetText(double cord)
        {
            if (this.textBox3.InvokeRequired)
            {
                SetTextCallback d = SetText;
                this.Invoke(d, cord);
            }
            else
            {
                this.textBox3.Text = cord.ToString(CultureInfo.InvariantCulture);
                this.alt = cord;
            }
        }

        private void LocationSelect_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (close)
            {
                var result = MessageBox.Show("You didn't set start location! Are you sure you want to exit this window?", "Location selector", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No || result == DialogResult.Abort)
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Globals.latitute = map.Position.Lat;
            Globals.longitude = map.Position.Lng;
            Globals.altitude = alt;
            close = false;
            ActiveForm.Dispose();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0 && textBox1.Text != "-")
            {
                try
                {
                    double lat = double.Parse(textBox1.Text.Replace(',', '.'), GUI.cords, System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (lat > 90.0 || lat < -90.0)
                        throw new System.ArgumentException("Value has to be between 180 and -180!");
                    map.Position = new GMap.NET.PointLatLng(lat, map.Position.Lng);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    textBox1.Text = "";
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0 && textBox2.Text != "-")
            {
                try
                {
                    double lng = double.Parse(textBox2.Text.Replace(',', '.'), GUI.cords, System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (lng > 180.0 || lng < -180.0)
                        throw new System.ArgumentException("Value has to be between 90 and -90!");
                    map.Position = new GMap.NET.PointLatLng(map.Position.Lat, lng);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    textBox2.Text = "";
                }
            }
        }

        private void LocationSelect_Load(object sender, EventArgs e)
        {

        }

        private void cbShowPokeStops_CheckedChanged(object sender, EventArgs e)
        {
            _pokeStopsOverlay.IsVisibile = cbShowPokeStops.Checked;
            map.Update();
        }
    }
}
