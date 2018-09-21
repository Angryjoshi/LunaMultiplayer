﻿using LmpCommon.Enums;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using Server.Settings.Structures;
using Server.System.Scenario;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.System
{
    public class ScenarioSystem
    {
        public const string ScenarioFileFormat = ".txt";
        public static string ScenariosPath = Path.Combine(ServerContext.UniverseDirectory, "Scenarios");

        public static bool GenerateDefaultScenarios()
        {
            var scenarioFilesCreated =
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "CommNetScenario.xml"), Resources.CommNetScenario) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "PartUpgradeManager.xml"), Resources.PartUpgradeManager) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ProgressTracking.xml"), Resources.ProgressTracking) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ResourceScenario.xml"), Resources.ResourceScenario) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioAchievements.xml"), Resources.ScenarioAchievements) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioDestructibles.xml"), Resources.ScenarioDestructibles) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "SentinelScenario.xml"), Resources.SentinelScenario) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "VesselRecovery.xml"), Resources.VesselRecovery) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioNewGameIntro.xml"), Resources.ScenarioNewGameIntro);

            if (GeneralSettings.SettingsStore.GameMode != GameMode.Sandbox)
            {
                scenarioFilesCreated &= FileHandler.CreateFile(Path.Combine(ScenariosPath, "ResearchAndDevelopment.xml"), Resources.ResearchAndDevelopment);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ResearchAndDevelopment.xml"));
            }

            if (GeneralSettings.SettingsStore.GameMode == GameMode.Career)
            {
                scenarioFilesCreated &=
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "ContractSystem.xml"), Resources.ContractSystem) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "Funding.xml"), Resources.Funding) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "Reputation.xml"), Resources.Reputation) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioContractEvents.xml"), Resources.ScenarioContractEvents) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioUpgradeableFacilities.xml"), Resources.ScenarioUpgradeableFacilities) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "StrategySystem.xml"), Resources.StrategySystem);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ContractSystem.xml"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "Funding.xml"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "Reputation.xml"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ScenarioContractEvents.xml"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ScenarioUpgradeableFacilities.xml"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "StrategySystem.xml"));
            }

            return scenarioFilesCreated;
        }

        public static void SendScenarioModules(ClientStructure client)
        {
            var scenarioDataArray = ScenarioStoreSystem.CurrentScenarios.Keys.Select(s =>
            {
                var scenarioConfigNode = ScenarioStoreSystem.GetScenarioInConfigNodeFormat(s);
                var serializedData = Encoding.UTF8.GetBytes(scenarioConfigNode);
                return new ScenarioInfo
                {
                    Data = serializedData,
                    NumBytes = serializedData.Length,
                    Module = Path.GetFileNameWithoutExtension(s)
                };
            }).ToArray();

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScenarioDataMsgData>();
            msgData.ScenariosData = scenarioDataArray;
            msgData.ScenarioCount = scenarioDataArray.Length;

            MessageQueuer.SendToClient<ScenarioSrvMsg>(client, msgData);
        }


        public static void ParseReceivedScenarioData(ClientStructure client, ScenarioBaseMsgData messageData)
        {
            var data = (ScenarioDataMsgData)messageData;
            LunaLog.Debug($"Saving {data.ScenarioCount} scenario modules from {client.PlayerName}");
            for (var i = 0; i < data.ScenarioCount; i++)
            {
                var scenarioAsConfigNode = Encoding.UTF8.GetString(data.ScenariosData[i].Data, 0, data.ScenariosData[i].NumBytes);
                ScenarioDataUpdater.RawConfigNodeInsertOrUpdate(data.ScenariosData[i].Module, scenarioAsConfigNode);
            }
        }
    }
}
