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

public sealed class GaugeWindow : Window
{
	private readonly IClientState _clientState;
	private readonly IDalamudPluginInterface _pluginInterface;
	private readonly Configuration _configuration;
	private readonly Func<float> _progressProvider;
	private readonly Func<Expansion> _expansionProvider;
	
	public GaugeWindow(Func<Expansion> expansionProvider, Func<float> progressProvider, IClientState clientState, IDalamudPluginInterface pluginInterface, Configuration configuration) : base("MSQ", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground)
	{
		IsOpen = true;
		RespectCloseHotkey = false;

		_clientState = clientState;
		_pluginInterface = pluginInterface;
		_configuration = configuration;
		_progressProvider = progressProvider;
		_expansionProvider = expansionProvider;

		Size = new Vector2(240, 180);
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

		TryDrawBackground(
			_configuration.GaugeBackground switch
			{
				GaugeBackground.Grayscale => "Images/Gauge_Grayscale.png",
				GaugeBackground.ARR => "Images/Gauge_ARR.png",
				_ => null,
			});

		ImGui.Dummy(new Vector2(0, 90));

		DrawExpansionIcons();

		ImGui.Dummy(new Vector2(0, 10));

		DrawProgressBar();
	}

	private void DrawProgressBar()
	{
		var currentProgress = _progressProvider();

		ImGui.SetCursorPosX(15);

		ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGuiEx.Vector4FromRGB(0x111111));

		ImGui.PushStyleColor(ImGuiCol.PlotHistogram, ImGuiEx.Vector4FromRGB(0x111199));

		ImGui.ProgressBar(currentProgress, new Vector2(210, 20));

		ImGui.PopStyleColor();

		ImGui.PopStyleColor();
	}

	private void DrawExpansionIcons()
	{
		var currentExpansion = _expansionProvider();

		for (var i = Expansion.ARR; i <= Expansion.DAWNTRAIL; i++)
		{
			if (i == 0)
			{
				ImGui.Dummy(new Vector2(5, 0));
			}

			ImGui.SameLine();

			TryDrawImage($"Images/{(i <= currentExpansion ? i : "NONE")}.png");
		}
	}

	private void TryDrawBackground(string? img)
	{
		if (img is null)
		{
			return;
		}

		var url = Path.Combine(_pluginInterface.AssemblyLocation.DirectoryName!, img);

		if (ThreadLoadImageHandler.TryGetTextureWrap(url, out var dalamudTextureWrap))
		{
			var windowPos = ImGui.GetWindowPos();

			ImGui
				.GetBackgroundDrawList()
				.AddImage(
					dalamudTextureWrap.ImGuiHandle,
					windowPos,
					windowPos + new Vector2(
						dalamudTextureWrap.Width,
						dalamudTextureWrap.Height));
		}
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
