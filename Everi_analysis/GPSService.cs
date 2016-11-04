using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using System.Timers;

namespace Everi_analysis
{
    [Service]
    public class GPSService : Service, ILocationListener
    {
        private string _location = string.Empty;
        private string _address = string.Empty;
        private string _remarks = string.Empty;
        private Timer _movementTrackingTimer;

        const int TwoMinutes = 1000 * 60 * 2;

        IBinder _binder;

        protected LocationManager _locationManager = (LocationManager)Application.Context.GetSystemService(LocationService);

        protected LocationManager _locationManager2 = (LocationManager)Application.Context.GetSystemService(LocationService);

        private Location _oldLocation;
        private Location _newLocation;

        public override IBinder OnBind(Intent intent)
        {
            _binder = new GPSServiceBinder(this);
            return _binder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
        }

        public void OnLocationChanged(Location location)
        {
            try
            {
                //bool moving = false;
                if (location == null)
                    _location = "Unable to determine your location.";
                else
                {
                    //_location = String.Format("{0},{1}", location.Latitude, location.Longitude);

                    //Geocoder geocoder = new Geocoder(this);

                    ////The Geocoder class retrieves a list of address from Google over the internet  
                    //IList<Address> addressList = geocoder.GetFromLocation(location.Latitude, location.Longitude, 10);

                    //Address addressCurrent = addressList.FirstOrDefault();
                    //if (addressCurrent != null)
                    //{
                    //    StringBuilder deviceAddress = new StringBuilder();

                    //    for (int i = 0; i < addressCurrent.MaxAddressLineIndex; i++)
                    //        deviceAddress.Append(addressCurrent.GetAddressLine(i))
                    //            .AppendLine(",");

                    //    _address = deviceAddress.ToString();

                    //}
                    //else
                    //    _address = "Unable to determine the address.";
                    if (IsBetterLocation(location,_newLocation))
                    {
                        _newLocation = location;
                    }
                   
                    //if (_oldLocation!=null)
                    //{
                    //    var coord1 = new LatLng(_oldLocation.Latitude, _oldLocation.Longitude);
                    //    var coord2 = new LatLng(location.Latitude, location.Longitude);

                    //    var distanceInRadius = Utils.HaversineDistance(coord1, coord2, Utils.DistanceUnit.Kilometers);
                    //    if (distanceInRadius >= 0.0005)
                    //    {
                    //        moving = true;
                    //    }
                    //}




                    ////_remarks = string.Format("Your are {0} miles away from your original location.", distanceInRadius);

                    //if (moving)
                    //{
                    //    _remarks = string.Format("You are moving");
                    //}
                    //else
                    //{
                    //    _remarks = string.Format("You are still");
                    //}

                    //_oldLocation = location;
                    //Intent intent = new Intent(this, typeof(MainActivity.GpsServiceReceiver));
                    //intent.SetAction(MainActivity.GpsServiceReceiver.LOCATION_UPDATED);
                    //intent.AddCategory(Intent.CategoryDefault);
                    //intent.PutExtra("Location", _location);
                    //intent.PutExtra("Address", location.Provider);
                    //intent.PutExtra("Remarks", _remarks);

                    //SendBroadcast(intent);

                }
            }
            catch (Exception)
            {
                _address = "Unable to determine the address.";
            }
        }

        public void StartLocationUpdates()
        {
            Criteria criteriaForGPSService = new Criteria
            {
                //A constant indicating an approximate accuracy  
                Accuracy = Accuracy.Coarse,
                PowerRequirement = Power.Medium
            };

            var locationProvider = _locationManager.GetBestProvider(criteriaForGPSService, true);
            _locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);


            criteriaForGPSService = new Criteria
            {
                //A constant indicating an approximate accuracy  
                Accuracy = Accuracy.Fine,
                PowerRequirement = Power.Medium
            };

            locationProvider = _locationManager.GetBestProvider(criteriaForGPSService, true);
            _locationManager2.RequestLocationUpdates(locationProvider, 0, 0, this);
            _movementTrackingTimer = new Timer(1000);
            _movementTrackingTimer.Elapsed += _movementTrackingTimer_Elapsed;
            _movementTrackingTimer.Start();
        }

        private void _movementTrackingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool moving = false;
            string provider = "";
            try
            {
                if (_newLocation != null)
                {
                    LatLng coord1;

                    if (_oldLocation != null)
                    {
                        coord1 = new LatLng(_oldLocation.Latitude, _oldLocation.Longitude);
                    }
                    else
                    {
                        coord1 = new LatLng(0, 0);
                    }

                    var coord2 = new LatLng(_newLocation.Latitude, _newLocation.Longitude);

                    var distanceInRadius = Utils.HaversineDistance(coord1, coord2, Utils.DistanceUnit.Kilometers);
                    if (distanceInRadius >= 0.0005)
                    {
                        moving = true;
                    }
                    _location = String.Format("{0},{1}", _newLocation.Latitude, _newLocation.Longitude);
                    provider = _newLocation.Provider;
                }




                //_remarks = string.Format("Your are {0} miles away from your original location.", distanceInRadius);

                if (moving)
                {
                    _remarks = string.Format("You are moving");
                }
                else
                {
                    _remarks = string.Format("You are still");
                }

                _oldLocation = _newLocation;


                Intent intent = new Intent(this, typeof(MainActivity.MyBroadcastReceiver));
                intent.SetAction(MainActivity.MyBroadcastReceiver.LOCATION_UPDATED);
                intent.AddCategory(Intent.CategoryDefault);
                intent.PutExtra("Location", _location);
                intent.PutExtra("Address", provider);
                intent.PutExtra("Remarks", _remarks);

                SendBroadcast(intent);
            }
            catch (Exception)
            {
                _address = "Unable to determine the address.";
            }


        }

        public void StopLoactionUpdates()
        {
            _locationManager.RemoveUpdates(this);
            _locationManager2.RemoveUpdates(this);
            _movementTrackingTimer.Stop();
            _movementTrackingTimer.Dispose();
        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }

        #region Get the most accurate result

        private bool IsBetterLocation(Location location, Location currentBestLocation)
        {
            if (currentBestLocation == null)
            {
                // A new location is always better than no location
                return true;
            }

            long timeDelta = location.Time - currentBestLocation.Time;

            bool isSignificantlyNewer = timeDelta > TwoMinutes;

            bool isSignificantlyOlder = timeDelta < -TwoMinutes;

            bool isNewer = timeDelta > 0;

            // If it's been more than two minutes since the current location, use the new location
            // because the user has likely moved
            if (isSignificantlyNewer)
            {
                return true;
            }
            else if (isSignificantlyOlder)
            {
                return false;
            }

            // Check whether the new location fix is more or less accurate
            int accuracyDelta = (int)(location.Accuracy - currentBestLocation.Accuracy);
            bool isLessAccurate = accuracyDelta > 0;
            bool isMoreAccurate = accuracyDelta < 0;
            bool isSignificantlyLessAccurate = accuracyDelta > 200;

            // Check if the old and new location are from the same provider
            bool isFromSameProvider = IsSameProvider(location.Provider, currentBestLocation.Provider);

            // Determine location quality using a combination of timeliness and accuracy
            if (isMoreAccurate)
            {
                return true;
            }
            else if (isNewer && !isLessAccurate)
            {
                return true;
            }
            else if (isNewer && !isSignificantlyLessAccurate && isFromSameProvider)
            {
                return true;
            }

            return false;
        }

        private bool IsSameProvider(string provider1, string provider2)
        {
            if (provider1 == null)
            {
                return (provider2 == null);
            }

            return (provider1 == provider2);
        }




        #endregion
    }

    public class GPSServiceBinder : Binder
    {
        public GPSService Service
        {
            get
            {
                return LocService;
            }
        }

        protected GPSService LocService;
        public bool IsBound { get; set; }
        public GPSServiceBinder(GPSService service)
        {
            LocService = service;
        }
    }

    public class GPSServiceConnection : Java.Lang.Object, IServiceConnection
    {
        GPSServiceBinder _binder;

        public GPSServiceConnection(GPSServiceBinder binder)
        {
            if (binder != null)
                this._binder = binder;
        }

        public void StopLocationUpdates()
        {
            _binder.Service.StopLoactionUpdates();
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            GPSServiceBinder serviceBinder = (GPSServiceBinder)service;

            if (serviceBinder != null)
            {
                _binder = serviceBinder;
                _binder.IsBound = true;
                serviceBinder.Service.StartLocationUpdates();
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder.IsBound = false;
        }
    }
}