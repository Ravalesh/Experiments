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

namespace Everi_analysis
{
    [Service]
    public class GPSService : Service, ILocationListener
    {
        private string _location = string.Empty;
        private string _address = string.Empty;
        private string _remarks = string.Empty;

        IBinder _binder;

        protected LocationManager _locationManager = (LocationManager)Application.Context.GetSystemService(LocationService);

        protected LocationManager _locationManager2 = (LocationManager)Application.Context.GetSystemService(LocationService);

        private Location _oldLocation;

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
                bool moving = false;
                if (location == null)
                    _location = "Unable to determine your location.";
                else
                {
                    _location = String.Format("{0},{1}", location.Latitude, location.Longitude);

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
                    if (_oldLocation!=null)
                    {
                        var coord1 = new LatLng(_oldLocation.Latitude, _oldLocation.Longitude);
                        var coord2 = new LatLng(location.Latitude, location.Longitude);

                        var distanceInRadius = Utils.HaversineDistance(coord1, coord2, Utils.DistanceUnit.Kilometers);
                        if (distanceInRadius >= 0.0005)
                        {
                            moving = true;
                        }
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

                    _oldLocation = location;
                    Intent intent = new Intent(this, typeof(MainActivity.GpsServiceReceiver));
                    intent.SetAction(MainActivity.GpsServiceReceiver.LOCATION_UPDATED);
                    intent.AddCategory(Intent.CategoryDefault);
                    intent.PutExtra("Location", _location);
                    intent.PutExtra("Address", location.Provider);
                    intent.PutExtra("Remarks", _remarks);

                    SendBroadcast(intent);

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
        }

        public void StopLoactionUpdates()
        {
            _locationManager.RemoveUpdates(this);
            _locationManager2.RemoveUpdates(this);
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