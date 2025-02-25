﻿
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Model;
using Newtonsoft.Json;
using PU_Test.Common;
using PU_Test.Common.Game;
using PU_Test.Common.Patch;
using PU_Test.Common.Proxy;
using PU_Test.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PU_Test.ViewModel
{
    partial class MainWindow : ObservableObject
    {
        DispatcherTimer dispatcherTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 10) };

        public ProxyHelper.ProxyController proxyController;

        public MainWindow()
        {
            try
            {
                launcherConfig = JsonConvert.DeserializeObject<LauncherConfig>(File.ReadAllText("config.json"));

                //Task.Run(async () =>
                //{
                //    ServerInfo = await ServerInfoGetter.GetAsync(launcherConfig.ProxyConfig.ProxyServer);
                //    AnnounceMents = await ServerInfoGetter.GetAnnounceAsync(launcherConfig.ProxyConfig.ProxyServer);

                UpdateSI();

                //});
            }
            catch (Exception ex)
            {
                launcherConfig = new LauncherConfig();
                launcherConfig.ProxyConfig = new ProxyConfig(true, "gm.elysia.li");
                launcherConfig.ProxyConfig.ProxyPort = "25565";

                launcherConfig.GameInfo = new GameInfo(GameHelper.GameRegReader.GetGameExePath());
                launcherConfig.ProxyOnly = false;

                SaveConfig();
            }
            try
            {


            }
            catch (Exception ex)
            {

            }

            ShowPatchStatue();

            dispatcherTimer.Tick += (a, b) =>
            {
                UpdateSI();
            };

            dispatcherTimer.Start();


        }

        [ObservableProperty]
        private List<AnnounceMentItem> announceMents;

        public void UpdateSI()
        {
            Task.Run(async () =>
            {
                ServerInfoGetter.scheme = launcherConfig.ProxyConfig.UseHttp ? "http" : "https";
                ServerInfo = await ServerInfoGetter.GetAsync(launcherConfig.ProxyConfig.ProxyServer);
                AnnounceMents = await ServerInfoGetter.GetAnnounceAsync(launcherConfig.ProxyConfig.ProxyServer);

            });
        }

        public void ShowPatchStatue()
        {
            switch (new PatchHelper(launcherConfig.GameInfo).GetPatchStatue())
            {
                case PatchHelper.PatchType.None: PatchStatueStr = "官方"; break;
                case PatchHelper.PatchType.All: PatchStatueStr = "已打补丁-ALL"; break;
                case PatchHelper.PatchType.MetaData: PatchStatueStr = "已打补丁-Meta"; break;
                case PatchHelper.PatchType.UserAssemby: PatchStatueStr = "已打补丁-UA"; break;
                case PatchHelper.PatchType.Unknown: PatchStatueStr = "未知"; break;
            }
        }
        public void SaveConfig()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(launcherConfig));

        }

        [ObservableProperty]
        private LauncherConfig launcherConfig;

        [ObservableProperty]
        private string startGameBtnText = "开始游戏";

        [ObservableProperty]
        private string patchStatueStr = "未知";

        [ObservableProperty]
        private ServerInfo serverInfo = new ServerInfo();

        private bool IsGameRunning = false;

        [RelayCommand]
        private void StartGame()
        {


            if (launcherConfig.ProxyOnly == true)
            {
                if (proxyController == null)
                {
                    proxyController = new ProxyHelper.ProxyController(host: launcherConfig.ProxyConfig.ProxyServer, port: launcherConfig.ProxyConfig.ProxyPort,usehttp:launcherConfig.ProxyConfig.UseHttp);
                    proxyController.Start();
                    StartGameBtnText = "关闭代理";
                    return;

                }
                if (proxyController._IsRun == true)
                {
                    proxyController.Stop();
                    proxyController = null;
                    StartGameBtnText = "开始游戏";

                }
                else
                {
                    proxyController.Start();
                    StartGameBtnText = "关闭代理";

                }
                return;
            }



            if (new PatchHelper(launcherConfig.GameInfo).GetPatchStatue() == PatchHelper.PatchType.None)
            {
                GameHelper.StartGame(launcherConfig.GameInfo.GameExePath);
                return;
            }
            if (!IsGameRunning)
            {

                if (!CheckGameCfg())
                {
                    MessageBox.Show("配置项不正确！");
                    return;
                }
                IsGameRunning = true;

                proxyController = new ProxyHelper.ProxyController(
                    host: launcherConfig.ProxyConfig.ProxyServer, 
                    port: launcherConfig.ProxyConfig.ProxyPort,
                    usehttp : launcherConfig.ProxyConfig.UseHttp
                    );
                proxyController.Start();
                StartGameBtnText = "关闭代理";

                GameHelper.StartGame(launcherConfig.GameInfo.GameExePath);


            }
            else
            {
                if (proxyController != null)
                {

                    proxyController.Stop();
                }
                proxyController = null;
                StartGameBtnText = "开始游戏";
                IsGameRunning = false;
            }

        }
        private bool CheckGameCfg()
        {
            if (launcherConfig.GameInfo != null)
            {
                return true;
            }
            MessageBox.Show("请设定游戏路径！");
            return false;
        }









        public void Official_Set()
        {
            new PatchHelper(launcherConfig.GameInfo).UnPatchUserAssembly();
            //MessageBox.Show("暂不支持！");
            ShowPatchStatue();
        }

        public void Private_Set()
        {
            new PatchHelper(launcherConfig.GameInfo).PatchUserAssembly();
            //MessageBox.Show("暂不支持！");
            ShowPatchStatue();
        }


        //public void ShowPatchStatue()
        //{
        //    string PatchStatueStr = "";
        //    switch (new PatchHelper(launcherConfig.GameInfo).GetPatchStatue())
        //    {
        //        case PatchHelper.PatchType.None: PatchStatueStr = "官方"; break;
        //        case PatchHelper.PatchType.All: PatchStatueStr = "已打补丁-ALL"; break;
        //        case PatchHelper.PatchType.MetaData: PatchStatueStr = "已打补丁-Meta"; break;
        //        case PatchHelper.PatchType.UserAssemby: PatchStatueStr = "已打补丁-UA"; break;
        //    }
        //    GlobalValues.mainWindow.vm.PatchStatueStr = PatchStatueStr;

        //}
    }
}
