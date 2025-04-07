using Dalamud.Configuration;

namespace MSQGauge;

public sealed class Configuration : IPluginConfiguration
{
	public int Version { get; set; } = 1;
	public bool IsGaugeLocked { get; set; } = false;
	public GaugeBackground GaugeBackground { get; set; } = GaugeBackground.ARR;
}
