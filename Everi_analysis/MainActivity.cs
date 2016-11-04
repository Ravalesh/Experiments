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

        GPSServiceBinder _gpsBinder;
        GPSServiceConnection _gpsServiceConnection;
        Intent _gpsServiceIntent;
        private MyBroadcastReceiver _receiver;

        SensorConnection _sensorConnection;
        Intent _sensorIntent;

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
            _gpsServiceConnection = new GPSServiceConnection(_gpsBinder);
            _gpsServiceIntent = new Intent(Android.App.Application.Context, typeof(GPSService));
            BindService(_gpsServiceIntent, _gpsServiceConnection, Bind.AutoCreate);

            _sensorConnection = new SensorConnection();
            _sensorIntent = new Intent(Application.Context, typeof(SensorService));
            BindService(_sensorIntent, _sensorConnection, Bind.AutoCreate);
        }

        private void RegisterBroadcastReceiver()
        {
            IntentFilter filter = new IntentFilter(MyBroadcastReceiver.LOCATION_UPDATED);
            filter.AddCategory(Intent.CategoryDefault);
            _receiver = new MyBroadcastReceiver();
            RegisterReceiver(_receiver, filter);
        }

        private void UnRegisterBroadcastReceiver()
        {
            UnregisterReceiver(_receiver);
        }

        public void UpdateUI(Intent intent, ReceiverType receiver)
        {
            if (receiver == ReceiverType.Location)
            {
                _latitude.Text = intent.GetStringExtra("Location");
                _longitude.Text = intent.GetStringExtra("Address");
                _status.Text = intent.GetStringExtra("Remarks");
            }
            else if (receiver == ReceiverType.Shake)
            {
                Toast.MakeText(this, "Device Shaken", ToastLength.Long).Show();

            }
            
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
            _gpsServiceIntent.Dispose();

            _sensorConnection.StopSensorService();
            UnbindService(_sensorConnection);
            _sensorConnection = null;
            _sensorIntent.Dispose();

            Finish();
        }

        

        [BroadcastReceiver]
        internal class MyBroadcastReceiver : BroadcastReceiver
        {
            public static readonly string LOCATION_UPDATED = "LOCATION_UPDATED";
            public static readonly string SHAKE_DETECTED = "SHAKE_DETECTED";
            public override void OnReceive(Context context, Intent intent)
            {
                if (intent.Action.Equals(LOCATION_UPDATED))
                {
                    Instance.UpdateUI(intent, ReceiverType.Location);
                }
                else if (intent.Action.Equals(SHAKE_DETECTED))
                {
                    Instance.UpdateUI(intent, ReceiverType.Shake);
                }
            }
        }
    }

    public enum ReceiverType
    {
        Location = 1,
        Shake = 2
    }
}

