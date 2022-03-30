using System;
using AzureDeprecation.Contracts.Enum;

namespace AzureDeprecation.Notices.Management
{
    internal class LabelFactory
    {
        public static string GetForService(AzureService service)
        {
            switch (service)
            {
                case AzureService.ActiveDirectory:
                    return "services:active-directory";
                case AzureService.Advisor:
                    return "services:advisor";
                case AzureService.AnalysisService:
                    return "services:analysis-service";
                case AzureService.AnomalyDetector:
                    return "services:anomaly-detector";
                case AzureService.ApiApps:
                    return "services:api-apps";
                case AzureService.ApiManagement:
                    return "services:api-management";
                case AzureService.AppConfiguration:
                    return "services:app-config";
                case AzureService.ApplicationGateway:
                    return "services:app-gateway";
                case AzureService.ApplicationInsights:
                    return "services:app-insights";
                case AzureService.AppService:
                    return "services:app-services";
                case AzureService.Arc:
                    return "services:arc";
                case AzureService.Automation:
                    return "services:automation";
                case AzureService.Backup:
                    return "services:backup";
                case AzureService.Bastion:
                    return "services:bastion";
                case AzureService.Batch:
                    return "services:batch";
                case AzureService.BlockchainService:
                    return "services:blockchain-service";
                case AzureService.BlockchainTokens:
                    return "services:blockchain-tokens";
                case AzureService.BlockchainWorkbench:
                    return "services:blockchain-workbench";
                case AzureService.Blueprints:
                    return "services:blueprints";
                case AzureService.BotService:
                    return "services:bot-ervice";
                case AzureService.CacheForRedis:
                    return "services:cache-redis";
                case AzureService.Cdn:
                    return "services:cdn";
                case AzureService.CloudServices:
                    return "services:cloud-services";
                case AzureService.CognitiveSearch:
                    return "services:cognitive-search";
                case AzureService.CognitiveServices:
                    return "services:cognitive-services";
                case AzureService.ContainerApps:
                    return "services:container-apps";
                case AzureService.ContainerInstances:
                    return "services:container-instances";
                case AzureService.ContainerRegistry:
                    return "services:container-registry";
                case AzureService.CosmosDb:
                    return "services:cosmos-db";
                case AzureService.CostManagement:
                    return "services:cost-management";
                case AzureService.CycleCloud:
                    return "services:cycle-cloud";
                case AzureService.DatabaseForMariaDB:
                    return "services:db-for-mariadb";
                case AzureService.DatabaseForMySQL:
                    return "services:db-for-mysql";
                case AzureService.DatabaseForPostgreSQL:
                    return "services:db-for-postgresql";
                case AzureService.DatabaseMigrationService:
                    return "services:db-migration-service";
                case AzureService.Databricks:
                    return "services:databricks";
                case AzureService.DataFactory:
                    return "services:data-factory";
                case AzureService.DataLakeAnalytics:
                    return "services:data-lake-analytics";
                case AzureService.DataLakeStorage:
                    return "services:data-lake-storage";
                case AzureService.DataShare:
                    return "services:data-share";
                case AzureService.DataScience:
                    return "services:data-sience";
                case AzureService.DdosProtection:
                    return "services:ddos-protection";
                case AzureService.DedicatedHost:
                    return "services:dedicated-host";
                case AzureService.DevOps:
                    return "services:devops";
                case AzureService.DevSpaces:
                    return "services:dev-spaces";
                case AzureService.DevTestLabs:
                    return "services:devtest-labs";
                case AzureService.DigitalTwins:
                    return "services:digital-twins";
                case AzureService.Dns:
                    return "services:dns";
                case AzureService.EventGrid:
                    return "services:event-grid";
                case AzureService.ExpressRoute:
                    return "services:express-route";
                case AzureService.Firewall:
                    return "services:firewall";
                case AzureService.FirewallManager:
                    return "services:firewall-manager";
                case AzureService.FormRecognizer:
                    return "services:form-recognizer";
                case AzureService.FrontDoor:
                    return "services:front-door";
                case AzureService.Functions:
                    return "services:functions";
                case AzureService.Genomics:
                    return "services:genomics";
                case AzureService.InformationProtection:
                    return "services:info-protection";
                case AzureService.InternetAnalyzer:
                    return "services:internet-analyzer";
                case AzureService.IotCentral:
                    return "services:iot-central";
                case AzureService.IoTEdge:
                    return "services:iot-edge";
                case AzureService.IotHub:
                    return "services:iot-hub";
                case AzureService.IotHubDps:
                    return "services:iot-dps";
                case AzureService.KubernetesService:
                    return "services:kubernetes-service";
                case AzureService.LabServices:
                    return "services:lab-services";
                case AzureService.Lighthouse:
                    return "services:lightouse";
                case AzureService.LoadBalancer:
                    return "services:loadbalancer";
                case AzureService.LogAnalytics:
                    return "services:log-analytcs";
                case AzureService.LogicApps:
                    return "services:logic-apps";
                case AzureService.MachineLearning:
                    return "services:ml";
                case AzureService.ManagedApplications:
                    return "services:managed-app";
                case AzureService.Maps:
                    return "services:maps";
                case AzureService.MediaServices:
                    return "services:media-services";
                case AzureService.Migrate:
                    return "services:migrate";
                case AzureService.MixedReality:
                    return "services:mixed-reality";
                case AzureService.MobileApps:
                    return "services:mobile-apps";
                case AzureService.Monitor:
                    return "services:monitor";
                case AzureService.NetworkWatcher:
                    return "services:network-watcher";
                case AzureService.NotificationHubs:
                    return "services:notification-hubs";
                case AzureService.Other:
                    return "services:other";
                case AzureService.Policy:
                    return "services:policy";
                case AzureService.PrivateLink:
                    return "services:private-link";
                case AzureService.RedHatOpenshift:
                    return "services:red-hat-openshift";
                case AzureService.RemoteRendering:
                    return "services:remote-rendering";
                case AzureService.ResourceManager:
                    return "services:arm";
                case AzureService.SecurityCenter:
                    return "services:security-center";
                case AzureService.Sentinel:
                    return "services:sentinel";
                case AzureService.ServiceBus:
                    return "services:service-bus";
                case AzureService.ServiceFabric:
                    return "services:service-fabric";
                case AzureService.ServiceFabricMesh:
                    return "services:service-fabric-mesh";
                case AzureService.ServiceHealth:
                    return "services:service-health";
                case AzureService.SignalR:
                    return "services:signal-r";
                case AzureService.SiteRecovery:
                    return "services:site-recovery";
                case AzureService.SpatialAnchors:
                    return "services:spatial-anchors";
                case AzureService.Sphere:
                    return "services:sphere";
                case AzureService.SpringCloud:
                    return "services:spring-cloud";
                case AzureService.SqlDatabase:
                    return "services:sql-db";
                case AzureService.SqlEdge:
                    return "services:sql-edge";
                case AzureService.SqlManagedInstance:
                    return "services:sql-mi";
                case AzureService.Stack:
                    return "services:stack";
                case AzureService.Storage:
                    return "services:storage";
                case AzureService.StreamAnalytics:
                    return "services:stream-analytics";
                case AzureService.SynapseAnalytics:
                    return "services:synapse-analytics";
                case AzureService.TimeSeriesInsights:
                    return "services:time-series-insights";
                case AzureService.TrafficManager:
                    return "services:traffic-manager";
                case AzureService.VideoIndexer:
                    return "services:video-indexer";
                case AzureService.VirtualMachine:
                    return "services:compute-vm";
                case AzureService.VirtualMachineScaleSets:
                    return "services:compute-vmss";
                case AzureService.VirtualNetwork:
                    return "services:virtual-network";
                case AzureService.VisualStudioCodespaces:
                    return "services:codespaces";
                case AzureService.VMwareSolutions:
                    return "services:vmware";
                case AzureService.VpnGateway:
                    return "services:vpn-gateway";
                case AzureService.WebApp:
                    return "services:web-app";
                case AzureService.WebApplicationGateway:
                    return "services:waf";
                case AzureService.WindowsVirtualDesktop:
                    return "services:windows-virtual-desktop";
                case AzureService.PowerBI:
                    return "services:power-bi";
                case AzureService.StoreSimple:
                    return "services:store-simple";
                case AzureService.HDInsight:
                    return "services:hdinsight";
                case AzureService.EventHubs:
                    return "services:event-hubs";
                case AzureService.ResourceGraph:
                    return "services:resource-graph";
                case AzureService.All:
                    return "services:all";
                case AzureService.Relay:
                    return "services:relay";
                default:
                    throw new ArgumentOutOfRangeException(nameof(service), service, null);
            }
        }

        public static string GetForImpactArea(ImpactArea impactArea)
        {
            switch (impactArea)
            {
                case ImpactArea.Unknown:
                    return "area:unknown";
                case ImpactArea.Sdk:
                    return "area:sdk";
                case ImpactArea.Certification:
                    return "area:certification";
                case ImpactArea.Tooling:
                    return "area:tooling";
                case ImpactArea.Security:
                    return "area:security";
                case ImpactArea.ApiEndpoint:
                    return "area:api-endpoint";
                case ImpactArea.Feature:
                    return "area:feature";
                case ImpactArea.ServiceRuntime:
                    return "area:service-runtime";
                case ImpactArea.Region:
                    return "area:region";
                case ImpactArea.Sku:
                    return "area:sku";
                default:
                    throw new ArgumentOutOfRangeException(nameof(impactArea), impactArea, null);
            }
        }

        public static string GetForImpactType(ImpactType impactType)
        {
            switch (impactType)
            {
                case ImpactType.Unknown:
                    return "impact:unknown";
                case ImpactType.None:
                    return "impact:none";
                case ImpactType.Limited:
                    return "impact:limited";
                case ImpactType.UpgradeRequired:
                    return "impact:upgrade-required";
                case ImpactType.MigrationRequired:
                    return "impact:migration-required";
                case ImpactType.ShutdownWithoutAlternative:
                    return "impact:no-alternative";
                default:
                    throw new ArgumentOutOfRangeException(nameof(impactType), impactType, null);
            }
        }

        public static string GetForCloud(AzureCloud cloud)
        {
            switch (cloud)
            {
                case AzureCloud.Unknown:
                    return "cloud:unknown";
                case AzureCloud.Public:
                    return "cloud:public";
                case AzureCloud.Sovereign:
                    return "cloud:sovereign";
                case AzureCloud.China:
                    return "cloud:china";
                case AzureCloud.Government:
                    return "cloud:government";
                case AzureCloud.German:
                    return "cloud:german";
                default:
                    throw new ArgumentOutOfRangeException(nameof(cloud), cloud, null);
            }
        }
    }
}
