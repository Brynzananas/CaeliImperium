using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace CaeliImperium.Components;

public class BooleanConfigComponent : MonoBehaviour
{
    public string configSectonName;
    public string configKeyName;
    public UnityEvent<bool> onBooleanConfig;
    private ConfigEntry<bool> configEntry;
    private bool toRemove;
    public void Awake()
    {
        ConfigDefinition configDefinition = new ConfigDefinition(configSectonName, configKeyName);
        if (!(CaeliImperiumPlugin.instance.Config.TryGetEntry(configDefinition, out configEntry))) return;
        toRemove = true;
        configEntry.SettingChanged += ConfigEntry_SettingChanged;
        onBooleanConfig?.Invoke(configEntry.Value);
    }
    public void OnDestroy()
    {
        if (!toRemove || configEntry == null) return;
        configEntry.SettingChanged -= ConfigEntry_SettingChanged;
        toRemove = false;
    }
    private void ConfigEntry_SettingChanged(object sender, EventArgs e)
    {
        if (configEntry == null) return;
        onBooleanConfig?.Invoke(configEntry.Value);
    }
}
