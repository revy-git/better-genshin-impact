﻿using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.AutoGeniusInvokation;
using BetterGenshinImpact.GameTask.Model;
using BetterGenshinImpact.Service.Interface;
using BetterGenshinImpact.View.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BetterGenshinImpact.GameTask.AutoDomain;
using BetterGenshinImpact.GameTask.AutoFight;
using BetterGenshinImpact.GameTask.AutoWood;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace BetterGenshinImpact.ViewModel.Pages;

public partial class TaskSettingsPageViewModel : ObservableObject, INavigationAware
{
    public AllConfig Config { get; set; }

    private readonly INavigationService _navigationService;
    private readonly TaskTriggerDispatcher _taskDispatcher;

    private CancellationTokenSource? _cts;


    [ObservableProperty] private string[] _strategyList;
    [ObservableProperty] private string _switchAutoGeniusInvokationButtonText;

    [ObservableProperty] private int _autoWoodRoundNum;
    [ObservableProperty] private string _switchAutoWoodButtonText;


    [ObservableProperty] private string[] _combatStrategyList;
    [ObservableProperty] private int _autoDomainRoundNum;
    [ObservableProperty] private string _switchAutoDomainButtonText = "Start";
    [ObservableProperty] private string _switchAutoFightButtonText = "Start";


    public TaskSettingsPageViewModel(IConfigService configService, INavigationService navigationService, TaskTriggerDispatcher taskTriggerDispatcher)
    {
        Config = configService.Get();
        _navigationService = navigationService;
        _taskDispatcher = taskTriggerDispatcher;

        _strategyList = LoadCustomScript();
        _combatStrategyList = LoadCustomCombatScript();
        _switchAutoGeniusInvokationButtonText = "Start";

        _switchAutoWoodButtonText = "Start";
    }

    private string[] LoadCustomScript()
    {
        var files = Directory.GetFiles(Global.Absolute(@"User\AutoGeniusInvokation"), "*.*",
            SearchOption.AllDirectories);

        var strategyList = new string[files.Length];
        for (var i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".txt"))
            {
                var fileName = Path.GetFileNameWithoutExtension(files[i]);
                strategyList[i] = fileName;
            }
        }

        return strategyList;
    }

    private string[] LoadCustomCombatScript()
    {
        var files = Directory.GetFiles(Global.Absolute(@"User\AutoFight"), "*.*",
            SearchOption.AllDirectories);

        var strategyList = new string[files.Length];
        for (var i = 0; i < files.Length; i++)
        {
            if (files[i].EndsWith(".txt"))
            {
                var fileName = Path.GetFileNameWithoutExtension(files[i]);
                strategyList[i] = fileName;
            }
        }

        return strategyList;
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }

    [RelayCommand]
    public void OnGoToHotKeyPage()
    {
        _navigationService.Navigate(typeof(HotKeyPage));
    }

    [RelayCommand]
    public void OnSwitchAutoGeniusInvokation()
    {
        try
        {
            if (SwitchAutoGeniusInvokationButtonText == "Start")
            {
                if (string.IsNullOrEmpty(Config.AutoGeniusInvokationConfig.StrategyName))
                {
                    MessageBox.Show("Please select a TCG strategy first");
                    return;
                }

                var path = Global.Absolute(@"User\AutoGeniusInvokation\" + Config.AutoGeniusInvokationConfig.StrategyName + ".txt");

                if (!File.Exists(path))
                {
                    MessageBox.Show("TCG Config file does not exit");
                    return;
                }

                var content = File.ReadAllText(path);
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var param = new GeniusInvokationTaskParam(_cts, _taskDispatcher, content);
                _taskDispatcher.StartIndependentTask(IndependentTaskEnum.AutoGeniusInvokation, param);
                SwitchAutoGeniusInvokationButtonText = "Stop";
            }
            else
            {
                _cts?.Cancel();
                SwitchAutoGeniusInvokationButtonText = "Start";
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    [RelayCommand]
    public void OnGoToAutoGeniusInvokationUrl()
    {
        Process.Start(new ProcessStartInfo("https://bgi.huiyadan.com/doc.html#%E8%87%AA%E5%8A%A8%E4%B8%83%E5%9C%A3%E5%8F%AC%E5%94%A4") { UseShellExecute = true });
    }

    [RelayCommand]
    public void OnSwitchAutoWood()
    {
        try
        {
            if (SwitchAutoWoodButtonText == "Start")
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var param = new WoodTaskParam(_cts, _taskDispatcher, AutoWoodRoundNum);
                _taskDispatcher.StartIndependentTask(IndependentTaskEnum.AutoWood, param);
                SwitchAutoWoodButtonText = "Stop";
            }
            else
            {
                _cts?.Cancel();
                SwitchAutoWoodButtonText = "Start";
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    [RelayCommand]
    public void OnGoToAutoWoodUrl()
    {
        Process.Start(new ProcessStartInfo("https://bgi.huiyadan.com/doc.html#%E8%87%AA%E5%8A%A8%E4%BC%90%E6%9C%A8") { UseShellExecute = true });
    }


    [RelayCommand]
    public void OnSwitchAutoFight()
    {
        try
        {
            if (SwitchAutoFightButtonText == "Start")
            {
                var content = ReadFightStrategy(Config.AutoFightConfig.StrategyName);
                if (string.IsNullOrEmpty(content))
                {
                    return;
                }

                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var param = new AutoFightParam(_cts, content);
                _taskDispatcher.StartIndependentTask(IndependentTaskEnum.AutoFight, param);
                SwitchAutoFightButtonText = "Stop";
            }
            else
            {
                _cts?.Cancel();
                SwitchAutoFightButtonText = "Start";
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private string? ReadFightStrategy(string strategyName)
    {
        if (string.IsNullOrEmpty(strategyName))
        {
            MessageBox.Show("Please select a Combat strategy first");
            return null;
        }

        var path = Global.Absolute(@"User\AutoFight\" + strategyName + ".txt");

        if (!File.Exists(path))
        {
            MessageBox.Show("Combat Config file does not exit");
            return null;
        }

        var content = File.ReadAllText(path);
        return content;
    }

    [RelayCommand]
    public void OnGoToAutoFightUrl()
    {
        Process.Start(new ProcessStartInfo("https://bgi.huiyadan.com/feats/domain.html") { UseShellExecute = true });
    }

    [RelayCommand]
    public void OnSwitchAutoDomain()
    {
        try
        {
            if (SwitchAutoDomainButtonText == "Start")
            {
                var content = ReadFightStrategy(Config.AutoFightConfig.StrategyName);
                if (string.IsNullOrEmpty(content))
                {
                    return;
                }

                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var param = new AutoDomainParam(_cts, AutoDomainRoundNum, content);
                _taskDispatcher.StartIndependentTask(IndependentTaskEnum.AutoDomain, param);
                SwitchAutoDomainButtonText = "Stop";
            }
            else
            {
                _cts?.Cancel();
                SwitchAutoDomainButtonText = "Start";
            }
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    [RelayCommand]
    public void OnGoToAutoDomainUrl()
    {
        Process.Start(new ProcessStartInfo("https://bgi.huiyadan.com/feats/domain.html") { UseShellExecute = true });
    }


    public static void SetSwitchAutoGeniusInvokationButtonText(bool running)
    {
        var instance = App.GetService<TaskSettingsPageViewModel>();
        if (instance == null)
        {
            return;
        }

        instance.SwitchAutoGeniusInvokationButtonText = running ? "Stop" : "Start";
    }

    public static void SetSwitchAutoWoodButtonText(bool running)
    {
        var instance = App.GetService<TaskSettingsPageViewModel>();
        if (instance == null)
        {
            return;
        }

        instance.SwitchAutoWoodButtonText = running ? "Stop" : "Start";
    }

    public static void SetSwitchAutoDomainButtonText(bool running)
    {
        var instance = App.GetService<TaskSettingsPageViewModel>();
        if (instance == null)
        {
            return;
        }

        instance.SwitchAutoDomainButtonText = running ? "Stop" : "Start";
    }

    public static void SetSwitchAutoFightButtonText(bool running)
    {
        var instance = App.GetService<TaskSettingsPageViewModel>();
        if (instance == null)
        {
            return;
        }

        instance.SwitchAutoFightButtonText = running ? "Stop" : "Start";
    }
}