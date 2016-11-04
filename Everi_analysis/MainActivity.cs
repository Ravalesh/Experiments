using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Geolocator;

namespace Everi_analysis
{
    [Activity(Label = "Everi_analysis", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        TextView _status, _latitude, _longitude;

        GPSServiceBinder _binder;
        GPSServiceConnection _gpsServiceConnection;
        Intent _gpsServiceIntent;
        private GpsServiceReceiver _receiver;

        public static MainActivity Instance;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Instance = this;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            
            _status = FindViewById<TextView>(Resource.Id.positionStatus);

            _latitude = FindViewById<TextView>(Resource.Id.positionLatitude);

            _longitude = FindViewById<TextView>(Resource.Id.positionLongitude);

            RegisterService();


            //var locator = CrossGeolocator.Current;

            //locator.DesiredAccuracy = 100;

            //var position = await locator.GetPositionAsync(timeoutMilliseconds: 1000);

            //_status.Text = "Position Status: " + position.Timestamp;
            //_latitude.Text = "Position Latitude: " + position.Latitude;
            //_longitude.Text = "Position Longitude: " + position.Longitude;

        }

        private void RegisterService()
        {
            _gpsServiceConnection = new GPSServiceConnection(_binder);
            _gpsServiceIntent = new Intent(Android.App.Application.Context, typeof(GPSService));
            BindService(_gpsServiceIntent, _gpsServiceConnection, Bind.AutoCreate);
        }

        private void RegisterBroadcastReceiver()
        {
            IntentFilter filter = new IntentFilter(GpsServiceReceiver.LOCATION_UPDATED);
            filter.AddCategory(Intent.CategoryDefault);
            _receiver = new GpsServiceReceiver();
            RegisterReceiver(_receiver, filter);
        }

        private void UnRegisterBroadcastReceiver()
        {
            UnregisterReceiver(_receiver);
        }

        public void UpdateUI(Intent intent)
        {
            _latitude.Text = intent.GetStringExtra("Location");
            _longitude.Text = intent.GetStringExtra("Address");
            _status.Text = intent.GetStringExtra("Remarks");
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterBroadcastReceiver();
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnRegisterBroadcastReceiver();
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            _gpsServiceConnection.StopLocationUpdates();
            UnbindService(_gpsServiceConnection);
            _gpsServiceConnection = null;
            Finish();
        }

        [BroadcastReceiver]
        internal class GpsServiceReceiver : BroadcastReceiver
        {
            public static readonly string LOCATION_UPDATED = "LOCATION_UPDATED";
            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action.Equals(LOCATION_UPDATED))
                {
                    Instance.UpdateUI(intent);
                }
            }
        }
    }
}

