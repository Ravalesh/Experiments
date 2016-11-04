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
using Android.Hardware;

namespace Everi_analysis
{
    [Service]
    public class SensorService : Service, ISensorEventListener
    {
        bool hasUpdated = false;
        DateTime lastUpdate;
        float last_x = 0.0f;
        float last_y = 0.0f;
        float last_z = 0.0f;

        const int ShakeDetectionTimeLapse = 250;
        const double ShakeThreshold = 800;
        SensorManager _sensorManager;

        public void StartSensorService()
        {
            _sensorManager = GetSystemService(SensorService) as Android.Hardware.SensorManager;
            var sensor = _sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            _sensorManager.RegisterListener(this, sensor, Android.Hardware.SensorDelay.Game);
        }

        public void StopSensorService()
        {
            _sensorManager.UnregisterListener(this);
            _sensorManager.Dispose();
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        public override IBinder OnBind(Intent intent)
        {
            return new SensorBinder(this);
        }

        public void OnSensorChanged(SensorEvent e)
        {
            if (e.Sensor.Type == SensorType.Accelerometer)
            {
                float x = e.Values[0];
                float y = e.Values[1];
                float z = e.Values[2];

                DateTime curTime = System.DateTime.Now;

                if (hasUpdated == false)
                {
                    hasUpdated = true;
                    lastUpdate = curTime;

                    last_x = x;
                    last_y = y;
                    last_z = z;

                }
                else
                {
                    if ((curTime - lastUpdate).TotalMilliseconds > ShakeDetectionTimeLapse)
                    {
                        float diffTime = (float)(curTime - lastUpdate).TotalMilliseconds;
                        lastUpdate = curTime;
                        float total = x + y + z - last_x - last_y - last_z;
                        float speed = Math.Abs(total) / diffTime * 10000;

                        if (speed > ShakeThreshold)
                        {
                            //call Broadcast receiver for shake event
                            Intent intent = new Intent(this, typeof(MainActivity.MyBroadcastReceiver));
                            intent.SetAction(MainActivity.MyBroadcastReceiver.SHAKE_DETECTED);
                            intent.AddCategory(Intent.CategoryDefault);
                            SendBroadcast(intent);
                        }
                        last_x = x;
                        last_y = y;
                        last_z = z;
                    }
                }
            }
        }
    }

    public class SensorBinder : Binder
    {
        public bool isBound = false;
        public SensorService MySensorService
        {
            get
            {
                return _sensorService;
            }
        }

        protected SensorService _sensorService;
        public SensorBinder(SensorService service)
        {
            _sensorService = service;
        }
    }

    public class SensorConnection : Java.Lang.Object, IServiceConnection
    {
        SensorBinder _binder;
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _binder = (SensorBinder)service;

            if (_binder != null)
            {
                _binder.isBound = true;
                _binder.MySensorService.StartSensorService();
            }

        }

        public void StopSensorService()
        {
            if (_binder != null)
            {
                _binder.MySensorService.StopSensorService();
            }
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _binder.isBound = false;
        }
    }
}