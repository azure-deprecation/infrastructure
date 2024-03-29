﻿using AzureDeprecation.Contracts.Enum;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.Contracts.v1.Shared;

namespace AzureDeprecation.Tests.Unit.Generator
{
    public static class NewAzureDeprecationGenerator
    {
        public static NewAzureDeprecationV1Message GenerateSample(bool useAdvancedTimeline = false)
        {
            var newAzureDeprecationV1Message = new NewAzureDeprecationV1Message
            {
                Title = "Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.",
                Notice = new DraftNotice
                {
                    OfficialReport = @"> In May 2018, we announced the general availability of version 3 of Translator and will retire version 2 of Translator on 24 May 2021.
> 
> Key benefits of version 3 of Translator include: 
> - More functionalities including bilingual dictionary, transliterate and translate to multiple target languages in a single request.
> - Provides a  [layered security model](https://docs.microsoft.com/en-us/azure/cognitive-services/Welcome#securing-resources) as part of Azure Cognitive Services.
> - Customized translations through [Custom Translator](https://portal.customtranslator.azure.ai/).",
                    Links =
                    {
                        "https://azure.microsoft.com/en-gb/updates/version-2-of-translator-is-retiring-on-24-may-2021/",
                        "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3"
                    }
                },
                Impact = new Impact
                {
                    Description = "Service will no longer be available.",
                    Area = ImpactArea.ServiceRuntime,
                    Cloud = AzureCloud.Public,
                    Type = ImpactType.MigrationRequired,
                    Services = new List<AzureService>
                    {
                        AzureService.CognitiveServices
                    }
                },
                RequiredAction = new RequiredAction
                {
                    OfficialReport = @"> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.
> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal."
                },
                Contact = new List<ContactEntry>
                {
                    new ContactEntry
                    {
                        Type = ContactType.Email,
                        Data = "example@example.com"
                    }
                },
                Timeline = new List<TimeLineEntry>(),
                AdditionalInformation = "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3"
            };

            if (useAdvancedTimeline)
            {
                newAzureDeprecationV1Message.Timeline.Add(new TimeLineEntry
                {
                    Date = new DateTimeOffset(2021, 5, 24, 0, 0, 0, TimeSpan.Zero),
                    Description = "Feature will no longer work",
                    IsDueDate = true
                });
                newAzureDeprecationV1Message.Timeline.Add(new TimeLineEntry
                {
                    Date = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Description = "No longer able to create new resources with type V1",
                    Phase = "Creation Disabled"
                });
            }
            else
            {
                newAzureDeprecationV1Message.Timeline.Add(new TimeLineEntry
                {
                    Date = new DateTimeOffset(2021, 5, 24, 0, 0, 0, TimeSpan.Zero),
                    Description = "Feature will no longer work"
                });
            }

            return newAzureDeprecationV1Message;
        }
        public static NewDeprecationNoticePublishedV1Message GenerateNewDeprecationNoticePublishedV1Message(bool useAdvancedTimeline = false)
        {
            var deprecationInfo = new DeprecationInfo
            {
                Title = "Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.",
                Notice = new Notice
                {
                    Description = @"> In May 2018, we announced the general availability of version 3 of Translator and will retire version 2 of Translator on 24 May 2021.
> 
> Key benefits of version 3 of Translator include: 
> - More functionalities including bilingual dictionary, transliterate and translate to multiple target languages in a single request.
> - Provides a  [layered security model](https://docs.microsoft.com/en-us/azure/cognitive-services/Welcome#securing-resources) as part of Azure Cognitive Services.
> - Customized translations through [Custom Translator](https://portal.customtranslator.azure.ai/).",
                    Links =
                    {
                        "https://azure.microsoft.com/en-gb/updates/version-2-of-translator-is-retiring-on-24-may-2021/",
                        "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3"
                    }
                },
                Impact = new Impact
                {
                    Description = "Service will no longer be available.",
                    Area = ImpactArea.ServiceRuntime,
                    Cloud = AzureCloud.Public,
                    Type = ImpactType.MigrationRequired,
                    Services = new List<AzureService>
                    {
                        AzureService.CognitiveServices
                    }
                },
                RequiredAction = @"> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.
> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal.",
                Contact = new List<ContactEntry>
                {
                    new ContactEntry
                    {
                        Type = ContactType.Email,
                        Data = "example@example.com"
                    }
                },
                Timeline = new List<TimeLineEntry>(),
                AdditionalInformation = "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3"
            };

            if (useAdvancedTimeline)
            {
                deprecationInfo.Timeline.Add(new TimeLineEntry
                {
                    Date = new DateTimeOffset(2021, 5, 24, 0, 0, 0, TimeSpan.Zero),
                    Description = "Feature will no longer work",
                    IsDueDate = true
                });
                deprecationInfo.Timeline.Add(new TimeLineEntry
                {
                    Date = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    Description = "No longer able to create new resources with type V1",
                    Phase = "Creation Disabled"
                });
            }
            else
            {
                deprecationInfo.Timeline.Add(new TimeLineEntry
                {
                    Date = new DateTimeOffset(2021, 5, 24, 0, 0, 0, TimeSpan.Zero),
                    Description = "Feature will no longer work"
                });
            }

            var publishedNotice = new PublishedNotice
            {
                Title = "Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.",
                ApiInfo = new ApiInfo
                {
                    Id= 1337,
                    Url = "https://api.github.com/azure-deprecation/dashboard/issues/244"
                },
                DashboardInfo = new DashboardInfo
                {
                    Id = 1337,
                    Url = "https://github.com/azure-deprecation/dashboard/issues/244"
                },
                Labels = new List<string>{"foo", "bar"},
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-3),
                UpdatedAt = DateTimeOffset.UtcNow.AddHours(-1),
                ClosedAt = DateTimeOffset.UtcNow
            };

            var outcome = new NewDeprecationNoticePublishedV1Message
            {
                Id = Guid.NewGuid().ToString(),
                DeprecationInfo = deprecationInfo,
                PublishedNotice = publishedNotice
            };
            return outcome;
        }
    }
}