﻿using System;
using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.Core.Recognition.OCR;
using BetterGenshinImpact.Helpers;
using BetterGenshinImpact.Model;
using BetterGenshinImpact.Service.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;

namespace BetterGenshinImpact.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly IConfigService _configService;
        public string Title { get; set; } = $"BetterGI · A Better Genshin Impact · {Global.Version}";

        public AllConfig Config { get; set; }

        public MainWindowViewModel(INavigationService navigationService, IConfigService configService)
        {
            _configService = configService;
            Config = configService.Get();
            _logger = App.GetLogger<MainWindowViewModel>();
#if DEBUG
            Title += " · Dev";
#endif
        }


        [RelayCommand]
        private async void OnLoaded()
        {
            _logger.LogInformation("A Better Genshin Impact {Version}", Global.Version);
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var s = OcrFactory.Paddle.Ocr(new Mat(Global.Absolute("Assets\\Model\\PaddleOCR\\test_ocr.png"), ImreadModes.Grayscale));
                        Debug.WriteLine("PaddleOcr预热结果:" + s);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _logger.LogError("PaddleOcr预热异常：" + e.Source + "\r\n--" + Environment.NewLine + e.StackTrace + "\r\n---" + Environment.NewLine + e.Message);
                        var innerException = e.InnerException;
                        if (innerException != null)
                        {
                            _logger.LogError("PaddleOcr预热内部异常：" + innerException.Source + "\r\n--" + Environment.NewLine + innerException.StackTrace + "\r\n---" + Environment.NewLine + innerException.Message);
                            throw innerException;
                        }
                        else
                        {
                            throw;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show("PaddleOcr预热失败：" + e.Source + "\r\n--" + Environment.NewLine + e.StackTrace + "\r\n---" + Environment.NewLine + e.Message);
            }


            try
            {
                await Task.Run(GetNewestInfo);
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取最新版本信息失败：" + e.Source + "\r\n--" + Environment.NewLine + e.StackTrace + "\r\n---" + Environment.NewLine + e.Message);
                _logger.LogWarning("获取 BetterGI 最新版本信息失败");
            }
        }

        private async void GetNewestInfo()
        {
            try
            {
                var httpClient = new HttpClient();
                var notice = await httpClient.GetFromJsonAsync<Notice>(@"https://hui-config.oss-cn-hangzhou.aliyuncs.com/bgi/notice.json");
                if (notice != null && !string.IsNullOrWhiteSpace(notice.Version))
                {
                    if (Global.IsNewVersion(notice.Version))
                    {
                        if (!string.IsNullOrEmpty(Config.NotShowNewVersionNoticeEndVersion)
                            && !Global.IsNewVersion(Config.NotShowNewVersionNoticeEndVersion, notice.Version))
                        {
                            return;
                        }

                        await UIDispatcherHelper.Invoke(async () =>
                        {
                            var uiMessageBox = new Wpf.Ui.Controls.MessageBox
                            {
                                Title = "Update Note",
                                Content = $"Newest version found {notice.Version}，click Sure to go to the download page for the latest version",
                                PrimaryButtonText = "Sure",
                                SecondaryButtonText = "Do not remind again",
                                CloseButtonText = "Cancel",
                            };

                            var result = await uiMessageBox.ShowDialogAsync();
                            if (result == Wpf.Ui.Controls.MessageBoxResult.Primary)
                            {
                                Process.Start(new ProcessStartInfo("https://bgi.huiyadan.com/download.html") { UseShellExecute = true });
                            }
                            else if (result == Wpf.Ui.Controls.MessageBoxResult.Secondary)
                            {
                                Config.NotShowNewVersionNoticeEndVersion = notice.Version;
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("获取最新版本信息失败：" + e.Source + "\r\n--" + Environment.NewLine + e.StackTrace + "\r\n---" + Environment.NewLine + e.Message);
                _logger.LogWarning("获取 BetterGI 最新版本信息失败");
            }
        }

        [RelayCommand]
        private void OnClosed()
        {
            _configService.Save();
            WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<object>(this, "Close", "", ""));
            Debug.WriteLine("MainWindowViewModel Closed");
            Application.Current.Shutdown();
        }
    }
}