﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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

                Feel("HeartBeat", 0);
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

        public void StopAllHapticFeedback()
        {
            StopHeartBeat();
            StopHeartBeatSlow();

            OWO.Stop();
        }
    }
}
