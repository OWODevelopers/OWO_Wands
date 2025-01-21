using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using OWOGame;

namespace MyOWOTactsuit
{
    public class OWOSkin
    {       
        public bool suitDisabled = true;
        public bool systemInitialized = false;        
        private static bool heartBeatIsActive = false;
        private static bool slowHeartBeatIsActive = false;        
        public Dictionary<String, Sensation> FeedbackMap = new Dictionary<String, Sensation>();

        //private static OWOLib.RotationOption defaultRotationOption = new OWOLib.RotationOption(0.0f, 0.0f);

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            InitializeOWO();
        }

        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("42425328");

            OWO.Configure(gameAuth);
            string[] myIPs = getIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == ConnectionState.Connected)
            {
                suitDisabled = false;
                LOG("OWO suit connected.");
            }
            if (suitDisabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in FeedbackMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] getIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + "\\Mods\\" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    IPAddress address;
                    if (IPAddress.TryParse(line, out address)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }


        public async Task HeartBeatFuncAsync()
        {           
                while (heartBeatIsActive)
                {
                    Feel("HeartBeat", 0);
                    await Task.Delay(600);
                }            
        }

        public async Task SlowHeartBeatFuncAsync()
        {
            while (slowHeartBeatIsActive)
            {
                Feel("HeartBeat", 0);
                await Task.Delay(1000);
            }
        }

        public void LOG(string logStr)
        {
            #pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
            #pragma warning restore CS0618
        }

        void RegisterAllSensationsFiles()
        {
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    FeedbackMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.ToString()); }
            }

            systemInitialized = true;
        }

        public void Feel(String key, int Priority, float intensity = 1.0f, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                OWO.Send(FeedbackMap[key].WithPriority(Priority));
                LOG("SENSATION: " + key);
            }
            else LOG("Feedback not registered: " + key);
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            LOG("PlayBackHit");

            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            //OWOLib.ScaleOption scaleOption = new OWOLib.ScaleOption(1f, 1f);
            //OWOLib.RotationOption rotationOption = new OWOLib.RotationOption(xzAngle, yShift);
            //OWOLib.OWOManager.PlayRegistered(key, key, scaleOption, rotationOption);
        }

        public void Recoil(string weaponName, bool isRightHand, float intensity = 1.0f)
        {
            float duration = 1.0f;
            //var scaleOption = new OWOLib.ScaleOption(intensity, duration);
            // the function needs some rotation if you want to give the scale option as well
            // var rotationFront = new OWOLib.RotationOption(0f, 0f);
            // make postfix according to parameter
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            // stitch together pattern names for Arm and Hand recoil
            string keyHands = "RecoilHands" + postfix;
            string keyArm = "Recoil" + postfix;
            // vest pattern name contains the weapon name. This way, you can quickly switch
            // between swords, pistols, shotguns, ... by just changing the shoulder feedback
            // and scaling via the intensity for arms and hands
            string keyVest = "Recoil" + weaponName + "Vest" + postfix;
            //OWOLib.OWOManager.PlayRegistered(keyHands, keyHands, scaleOption, rotationFront);
            //OWOLib.OWOManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            //OWOLib.OWOManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
            LOG("Recoil");

        }

        public void CastSpell(string spellName, bool isRightHand, float intensity = 1.0f)
        {
            float duration = 1.0f;
            // var scaleOption = new OWOLib.ScaleOption(intensity, duration);
            // var rotationFront = new OWOLib.RotationOption(0f, 0f);
            string postfix = "_L";
            if (isRightHand) { postfix = "_R"; }

            string keyHand = "Spell" + spellName + "Hand" + postfix;
            string keyArm = "Spell" + spellName + "Arm" + postfix;
            string keyVest = "Spell" + spellName + "Vest" + postfix;
            //OWOLib.OWOManager.PlayRegistered(keyHand, keyHand, scaleOption, rotationFront);
            //OWOLib.OWOManager.PlayRegistered(keyArm, keyArm, scaleOption, rotationFront);
            //OWOLib.OWOManager.PlayRegistered(keyVest, keyVest, scaleOption, rotationFront);
            LOG("CastSpell");

        }

        public void HeadShot(String key, float hitAngle)
        {
            // I made 4 patterns in the Tactal for fron/back/left/right headshots
            //if (OWOLib.OWOManager.IsDeviceConnected(OWOLib.PositionID.Head))
            //{
            //    if ((hitAngle < 45f) | (hitAngle > 315f)) { PlaybackHaptics("Headshot_F"); }
            //    if ((hitAngle > 45f) && (hitAngle < 135f)) { PlaybackHaptics("Headshot_L"); }
            //    if ((hitAngle > 135f) && (hitAngle < 225f)) { PlaybackHaptics("Headshot_B"); }
            //    if ((hitAngle > 225f) && (hitAngle < 315f)) { PlaybackHaptics("Headshot_R"); }
            //}
            // If there is no Tactal, just forward to the vest  with angle and at the very top (0.5)
            // else { PlayBackHit(key, hitAngle, 0.5f); }
            LOG("HeadShot");

        }

        public void StartHeartBeat()
        {
            if (heartBeatIsActive) return;

            heartBeatIsActive = true;
            _ = HeartBeatFuncAsync();
        }

        public void StopHeartBeat()
        {
            heartBeatIsActive = false;
        }

        public void StartHeartBeatSlow()
        {
            if (slowHeartBeatIsActive) return;

            slowHeartBeatIsActive = true;
            _ = SlowHeartBeatFuncAsync();
        }

        public void StopHeartBeatSlow()
        {
            slowHeartBeatIsActive = false;
        }

        public bool IsPlaying(String effect)
        {
            return false;
        }

        public void StopHapticFeedback(String effect)
        {
            //OWOLib.OWOManager.StopPlaying(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopHeartBeat();
            StopHeartBeatSlow();

            OWO.Stop();
        }
    }
}
