using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

namespace MSQGauge;

public sealed class SettingsWindow(IDalamudPluginInterface pluginInterface, Configuration configuration) : Window("MSQ Gauge Settings", ImGuiWindowFlags.AlwaysAutoResize)
{
	private readonly IDalamudPluginInterface _pluginInterface = pluginInterface;
	private readonly Configuration _configuration = configuration;
	private bool _isGaugeLocked = configuration.IsGaugeLocked;
	private GaugeBackground _gaugeBackground = configuration.GaugeBackground;

	public override void Draw()
	{
		ImGui.Checkbox("Lock Gauge", ref _isGaugeLocked);

		if (ImGui.RadioButton("Transparent", _gaugeBackground == GaugeBackground.Transparent))
		{
			_gaugeBackground = GaugeBackground.Transparent;
		}

		ImGui.SameLine();

		if (ImGui.RadioButton("Grayscale", _gaugeBackground == GaugeBackground.Grayscale))
		{
			_gaugeBackground = GaugeBackground.Grayscale;
		}

		ImGui.SameLine();

		if (ImGui.RadioButton("ARR", _gaugeBackground == GaugeBackground.ARR))
		{
			_gaugeBackground = GaugeBackground.ARR;
		}

		if (ImGui.Button("Apply Changes"))
		{
			SaveChanges();
		}

		ImGui.SameLine();

		if (ImGui.Button("Close"))
		{
			IsOpen = false;
		}
	}

	private void SaveChanges()
	{
		_configuration.IsGaugeLocked = _isGaugeLocked;
		_configuration.GaugeBackground = _gaugeBackground;

		_pluginInterface.SavePluginConfig(_configuration);
	}
}
