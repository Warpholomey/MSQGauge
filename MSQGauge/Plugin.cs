using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using ECommons;
using ECommons.DalamudServices;

using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

using Lumina.Excel.Sheets;

using System.Collections.Generic;

namespace MSQGauge;

public sealed class Plugin : IDalamudPlugin
{
	private readonly Dictionary<Expansion, ushort> _expansionBegins;
	private readonly Dictionary<Expansion, ushort> _expansionEnds;
	private readonly Gauge _gauge;

	[PluginService]
	public static IDataManager DataManager { get; private set; } = null!;

	[PluginService]
	public static IChatGui ChatGui { get; private set; } = null!;

	[PluginService]
	public static IClientState ClientState { get; private set; } = null!;

	public WindowSystem WindowSystem { get; } = new();

	public unsafe Plugin(IDalamudPluginInterface dalamudPluginInterface)
	{
		(_expansionBegins, _expansionEnds) = CalculateExpansions();

		_gauge = new(GetCurrentExpansion, GetCurrentExpansionProgress, ClientState);

		WindowSystem.AddWindow(_gauge);

		ECommonsMain.Init(dalamudPluginInterface, this);

		Svc.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
	}

	public void Dispose()
	{
		Svc.PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;

		ECommonsMain.Dispose();
	}

	private unsafe ScenarioTree? GetCurrentScenarioTreeEntry()
	{
		var agentScenarioTree = AgentScenarioTree.Instance();

		if (agentScenarioTree is null || agentScenarioTree->Data is null)
		{
			return null;
		}

		var index = agentScenarioTree->Data->CompleteScenarioQuest;

		if (index == 0)
		{
			index = agentScenarioTree->Data->CurrentScenarioQuest;
		}

		return index == 0 ? null : DataManager.GetExcelSheet<ScenarioTree>().GetRow(index | 0x10000U);
	}

	private unsafe Expansion GetCurrentExpansion()
	{
		var current = GetCurrentScenarioTreeEntry();

		if (current is null || !DataManager.GetExcelSheet<Quest>().TryGetRow(current.Value.RowId, out var q))
		{
			return Expansion.ARR;
		}

		return (Expansion) q.Expansion.Value.RowId;
	}

	private unsafe float GetCurrentExpansionProgress()
	{
		var expansion = GetCurrentExpansion();

		var current = GetCurrentScenarioTreeEntry();

		if (current == null)
		{
			return 0;
		}

		var begin = _expansionBegins[expansion];

		var end = _expansionEnds[expansion];

		var progress = current.Value.Unknown2 - (float) begin;

		var all = 1 + (end - (float) begin);

		if (QuestManager.IsQuestComplete(current.Value.RowId))
		{
			progress++;
		}

		return progress / all;
	}

	private static (Dictionary<Expansion, ushort> Begins, Dictionary<Expansion, ushort> Ends) CalculateExpansions()
	{
		Dictionary<Expansion, ushort> expansionBegins = [], expansionEnds = [];

		foreach (var scenarioTree in DataManager.GetExcelSheet<ScenarioTree>())
		{
			if (!DataManager.GetExcelSheet<Quest>().TryGetRow(scenarioTree.RowId, out var q) || !q.Expansion.IsValid)
			{
				continue;
			}

			var expansion = (Expansion) q.Expansion.Value.RowId;

			expansionEnds.TryAdd(expansion, scenarioTree.Unknown2);

			if (scenarioTree.Unknown2 > expansionEnds[expansion])
			{
				expansionEnds[expansion] = scenarioTree.Unknown2;
			}

			expansionBegins.TryAdd(expansion, scenarioTree.Unknown2);

			if (scenarioTree.Unknown2 < expansionBegins[expansion])
			{
				expansionBegins[expansion] = scenarioTree.Unknown2;
			}
		}

		return (expansionBegins, expansionEnds);
	}
}
