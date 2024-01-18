using BetterGenshinImpact.Core.Config;
using BetterGenshinImpact.GameTask;
using BetterGenshinImpact.GameTask.Macro;
using BetterGenshinImpact.Service.Interface;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using BetterGenshinImpact.Helpers.Extensions;
using Microsoft.Extensions.Logging;
using HotKeySettingModel = BetterGenshinImpact.Model.HotKeySettingModel;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CommunityToolkit.Mvvm.Messaging;
using BetterGenshinImpact.GameTask.QucikBuy;

namespace BetterGenshinImpact.ViewModel.Pages;

public partial class HotKeyPageViewModel : ObservableObject
{
    private readonly ILogger<HotKeyPageViewModel> _logger;
    private readonly TaskSettingsPageViewModel _taskSettingsPageViewModel;
    public AllConfig Config { get; set; }

    [ObservableProperty] private ObservableCollection<HotKeySettingModel> _hotKeySettingModels = new();

    public HotKeyPageViewModel(IConfigService configService, ILogger<HotKeyPageViewModel> logger, TaskSettingsPageViewModel taskSettingsPageViewModel)
    {
        _logger = logger;
        _taskSettingsPageViewModel = taskSettingsPageViewModel;
        // 获取配置
        Config = configService.Get();

        // 构建快捷键配置列表
        BuildHotKeySettingModelList();

        foreach (var hotKeyConfig in HotKeySettingModels)
        {
            hotKeyConfig.RegisterHotKey();
            hotKeyConfig.PropertyChanged += (sender, e) =>
            {
                if (sender is HotKeySettingModel model)
                {
                    Debug.WriteLine($"{model.FunctionName} 快捷键变更为 {model.HotKey}");

                    // 反射更新配置
                    var pi = Config.HotKeyConfig.GetType().GetProperty(model.ConfigPropertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (null != pi && pi.CanWrite)
                    {
                        var str = model.HotKey.ToString();
                        if (str == "< None >")
                        {
                            str = "";
                        }

                        pi.SetValue(Config.HotKeyConfig, str, null);
                    }

                    model.UnRegisterHotKey();
                    model.RegisterHotKey();
                }
            };
        }
    }

    private void BuildHotKeySettingModelList()
    {

        var bgiEnabledHotKeySettingModel = new HotKeySettingModel(
            "Start/Stop BetterGI",
            nameof(Config.HotKeyConfig.BgiEnabledHotkey),
            Config.HotKeyConfig.BgiEnabledHotkey,
            (_, _) =>
            {
                WeakReferenceMessenger.Default.Send(new PropertyChangedMessage<object>(this, "SwitchTriggerStatus", "", ""));
            }
        );
        HotKeySettingModels.Add(bgiEnabledHotKeySettingModel);

        var autoPickEnabledHotKeySettingModel = new HotKeySettingModel(
            "Auto Pickup",
            nameof(Config.HotKeyConfig.AutoPickEnabledHotkey),
            Config.HotKeyConfig.AutoPickEnabledHotkey,
            (_, _) =>
            {
                TaskContext.Instance().Config.AutoPickConfig.Enabled = !TaskContext.Instance().Config.AutoPickConfig.Enabled;
                _logger.LogInformation("切换{Name}状态为[{Enabled}]", "自动拾取", ToChinese(TaskContext.Instance().Config.AutoPickConfig.Enabled));
            }
        );
        HotKeySettingModels.Add(autoPickEnabledHotKeySettingModel);

        var autoSkipEnabledHotKeySettingModel = new HotKeySettingModel(
            "Auto Dialogue Skip",
            nameof(Config.HotKeyConfig.AutoSkipEnabledHotkey),
            Config.HotKeyConfig.AutoSkipEnabledHotkey,
            (_, _) =>
            {
                TaskContext.Instance().Config.AutoSkipConfig.Enabled = !TaskContext.Instance().Config.AutoSkipConfig.Enabled;
                _logger.LogInformation("切换{Name}状态为[{Enabled}]", "自动剧情", ToChinese(TaskContext.Instance().Config.AutoSkipConfig.Enabled));
            }
        );
        HotKeySettingModels.Add(autoSkipEnabledHotKeySettingModel);

        var autoFishingEnabledHotKeySettingModel = new HotKeySettingModel(
            "Auto Fishing",
            nameof(Config.HotKeyConfig.AutoFishingEnabledHotkey),
            Config.HotKeyConfig.AutoFishingEnabledHotkey,
            (_, _) =>
            {
                TaskContext.Instance().Config.AutoFishingConfig.Enabled = !TaskContext.Instance().Config.AutoFishingConfig.Enabled;
                _logger.LogInformation("切换{Name}状态为[{Enabled}]", "自动钓鱼", ToChinese(TaskContext.Instance().Config.AutoFishingConfig.Enabled));
            }
        );
        HotKeySettingModels.Add(autoFishingEnabledHotKeySettingModel);

        var turnAroundHotKeySettingModel = new HotKeySettingModel(
            "Hold to Spin (Neuvilette)",
            nameof(Config.HotKeyConfig.TurnAroundHotkey),
            Config.HotKeyConfig.TurnAroundHotkey,
            (_, _) => { TurnAroundMacro.Done(); }
        );
        HotKeySettingModels.Add(turnAroundHotKeySettingModel);

        var enhanceArtifactHotKeySettingModel = new HotKeySettingModel(
            "Enhance Artifact",
            nameof(Config.HotKeyConfig.EnhanceArtifactHotkey),
            Config.HotKeyConfig.EnhanceArtifactHotkey,
            (_, _) => { QuickEnhanceArtifactMacro.Done(); }
        );
        HotKeySettingModels.Add(enhanceArtifactHotKeySettingModel);

        HotKeySettingModels.Add(new HotKeySettingModel(
            "Quick Buy",
            nameof(Config.HotKeyConfig.QuickBuyHotkey),
            Config.HotKeyConfig.QuickBuyHotkey,
            (_, _) => { QuickBuyTask.Done(); }
        ));


        HotKeySettingModels.Add(new HotKeySettingModel(
            "Start/Stop Auto TCG",
            nameof(Config.HotKeyConfig.AutoGeniusInvokation),
            Config.HotKeyConfig.AutoGeniusInvokation,
            (_, _) => { _taskSettingsPageViewModel.OnSwitchAutoGeniusInvokation(); }
        ));

        HotKeySettingModels.Add(new HotKeySettingModel(
            "Start/Stop Auto Woodcutting",
            nameof(Config.HotKeyConfig.AutoWoodHotkey),
            Config.HotKeyConfig.AutoWoodHotkey,
            (_, _) => { _taskSettingsPageViewModel.OnSwitchAutoWood(); }
        ));

        HotKeySettingModels.Add(new HotKeySettingModel(
            "Start/Stop Auto Combat",
            nameof(Config.HotKeyConfig.AutoFightHotkey),
            Config.HotKeyConfig.AutoFightHotkey,
            (_, _) => { _taskSettingsPageViewModel.OnSwitchAutoFight(); }
        ));

        HotKeySettingModels.Add(new HotKeySettingModel(
            "Start/Stop Auto Domain",
            nameof(Config.HotKeyConfig.AutoDomainHotkey),
            Config.HotKeyConfig.AutoDomainHotkey,
            (_, _) => { _taskSettingsPageViewModel.OnSwitchAutoDomain(); }
        ));
    }

    private string ToChinese(bool enabled)
    {
        return enabled.ToChinese();
    }
}