using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;

using ImGuiNET;

using System;

namespace MSQGauge;

public sealed class Gauge : Window
{
	private readonly IClientState _clientState;
	private readonly Func<float> _progressProvider;
	private readonly Func<Expansion> _expansionProvider;
	
	public Gauge(Func<Expansion> expansionProvider, Func<float> progressProvider, IClientState clientState) : base("MSQ", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground)
	{
		IsOpen = true;
		RespectCloseHotkey = false;

		_clientState = clientState;
		_progressProvider = progressProvider;
		_expansionProvider = expansionProvider;
	}

	public sealed override unsafe bool DrawConditions()
	{
		return _clientState.LocalPlayer != null;
	}

	public override void Draw()
	{
		ImGui.Text($"EXP: {_expansionProvider()} | PROGRESS: {_progressProvider()}");
	}
}
