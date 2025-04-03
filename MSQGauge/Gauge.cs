using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using ECommons.ImGuiMethods;

using FFXIVClientStructs.FFXIV.Component.GUI;

using ImGuiNET;

using System;
using System.IO;
using System.Numerics;

namespace MSQGauge;

public sealed class Gauge : Window
{
	private readonly IClientState _clientState;
	private readonly IDalamudPluginInterface _pluginInterface;
	private readonly Configuration _configuration;
	private readonly Func<float> _progressProvider;
	private readonly Func<Expansion> _expansionProvider;
	
	public Gauge(Func<Expansion> expansionProvider, Func<float> progressProvider, IClientState clientState, IDalamudPluginInterface pluginInterface, Configuration configuration) : base("MSQ", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground)
	{
		IsOpen = true;
		RespectCloseHotkey = false;

		_clientState = clientState;
		_pluginInterface = pluginInterface;
		_configuration = configuration;
		_progressProvider = progressProvider;
		_expansionProvider = expansionProvider;

		Size = new Vector2(240, 90);
	}

	public sealed override unsafe bool DrawConditions()
	{
		return _clientState.LocalPlayer != null && IsActionBarsVisible();
	}

	private static unsafe bool IsActionBarsVisible()
	{
		var stage = AtkStage.Instance();

		var loadedUnitsList = &stage->RaptureAtkUnitManager->AtkUnitManager.AllLoadedUnitsList;

		for (var i = 0; i <= Math.Min(loadedUnitsList->Entries.Length, loadedUnitsList->Count); i++)
		{
			var addon = loadedUnitsList->Entries[i].Value;

			if (addon !=  null && addon->IsVisible && addon->NameString == "_ActionBar")
			{
				return true;
			}
		}

		return false;
	}

	public override void Draw()
	{
		if (_configuration.IsGaugeLocked)
		{
			Flags |= ImGuiWindowFlags.NoInputs;
		}
		else
		{
			Flags &= ~ImGuiWindowFlags.NoInputs;
		}

		var currentExpansion = _expansionProvider();

		var currentProgress = _progressProvider();

		for (var i = Expansion.ARR; i <= Expansion.DAWNTRAIL; i++)
		{
			ImGui.SameLine();

			TryDrawImage($"Images/{(i <= currentExpansion ? i : "NONE")}.png");
		}

		ImGui.ProgressBar(currentProgress, new Vector2(214, 20));
	}

	private void TryDrawImage(string img)
	{
		var url = Path.Combine(_pluginInterface.AssemblyLocation.DirectoryName!, img);

		if (ThreadLoadImageHandler.TryGetTextureWrap(url, out var dalamudTextureWrap))
		{
			ImGui.Image(
				dalamudTextureWrap.ImGuiHandle,
				new Vector2(
					dalamudTextureWrap.Width,
					dalamudTextureWrap.Height));
		}
	}
}
