using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace bCoreDriver.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    class BcoreSettings : BindableBase
    {
        #region field

        private string _bcoreName;
        private string _displayName;

        #endregion

        #region property

        public string BcoreName
        {
            get { return _bcoreName; }
            set { SetProperty(ref _bcoreName, value); }
        }

        [JsonProperty]
        public string DisplayName
        {
            get { return _displayName; }
            set { SetProperty(ref _displayName, value); }
        }

        [JsonProperty]
        public List<MotorSetting> MotorSettings { get; }

        [JsonProperty]
        public List<ServoSetting> ServoSettings { get; }

        [JsonProperty]
        public List<PortOutSetting> PortOutSettings { get; }

        private static SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1,1);

        #endregion

        public static async Task<BcoreSettings> Load(string name)
        {
            await Semaphore.WaitAsync();
            BcoreSettings settings = null;
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

                var file = await localFolder.GetFileAsync(name + ".json");
                var json = await FileIO.ReadTextAsync(file);
                Debug.WriteLine("LOAD:" + json);
                settings = JsonConvert.DeserializeObject<BcoreSettings>(json);
                settings.BcoreName = name;
            }
            catch (Exception)
            {
                return new BcoreSettings(name);
            }
            finally
            {
                Semaphore.Release();
            }

            return settings;
        }

        public static async Task<bool> Save(BcoreSettings settings)
        {
            await Semaphore.WaitAsync();
            var result = true;
            try
            {
                var localFolder = ApplicationData.Current.LocalFolder;

                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                var file = await localFolder.CreateFileAsync(settings.BcoreName + ".json", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception)
            {
                result = false;
            }
            finally
            {
                Semaphore.Release();
            }

            return result;
        }

        [JsonConstructor]
        public BcoreSettings()
        {
            MotorSettings = new List<MotorSetting>();
            ServoSettings = new List<ServoSetting>();
            PortOutSettings = new List<PortOutSetting>();
        }

        public BcoreSettings(string name) : this()
        {
            BcoreName = name;
            DisplayName = name;

            for (var i = 0; i < 4; i++)
            {
                MotorSettings.Add(new MotorSetting());
                ServoSettings.Add(new ServoSetting());
                PortOutSettings.Add(new PortOutSetting());
            }
        }

        public bool IsShowMotor(int idx)
        {
            if (idx < 0 || MotorSettings.Count <= idx) return false;
            return MotorSettings[idx]?.IsShow ?? false;
        }

        public bool IsShowServo(int idx)
        {
            if (idx < 0 || ServoSettings.Count <= idx) return false;
            return ServoSettings[idx]?.IsShow ?? false;
        }

        public bool IsShowPortOut(int idx)
        {
            if (idx < 0 || PortOutSettings.Count <= idx) return false;
            return PortOutSettings[idx]?.IsShow ?? false;
        }

        public async void Init()
        {
            var dispacther = CoreApplication.MainView.Dispatcher;

            await dispacther.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(BcoreName));

                for (var i = 0; i < 4; i++)
                {
                    MotorSettings[i].Init();
                    ServoSettings[i].Init();
                }
            });
        }
    }
}
