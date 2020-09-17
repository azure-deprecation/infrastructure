using System;
using System.Collections.Generic;
using AzureDeprecation.Contracts.Enum;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Notices.Management;
using Newtonsoft.Json;
using Xunit;

namespace AzureDeprecation.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class IssueFactoryTests
    {
        [Fact]
        public void GenerateNewDeprecationNotice_ValidMessage_GeneratesExpectedIssue()
        {
            // Arrange
            const string ExpectedIssueContent = @"Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.

**Deadline:** Monday, May 24, 2021
**Impacted Services:**
- Azure Cognitive Services

**More information:**
- https://azure.microsoft.com/en-gb/updates/version-2-of-translator-is-retiring-on-24-may-2021/
- https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3

### Notice

Here's the official report from Microsoft:

> In May 2018, we announced the general availability of version 3 of Translator and will retire version 2 of Translator on 24 May 2021.
> 
> Key benefits of version 3 of Translator include: 
> - More functionalities including bilingual dictionary, transliterate and translate to multiple target languages in a single request.
> - Provides a  [layered security model](https://docs.microsoft.com/en-us/azure/cognitive-services/Welcome#securing-resources) as part of Azure Cognitive Services.
> - Customized translations through [Custom Translator](https://portal.customtranslator.azure.ai/).

### Impact

Service will no longer be available.

### Required Action

Here's the official report from Microsoft:

> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.
> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal.

### Contact

None.

### More information

https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3";
            
            var newAzureDeprecationV1Message = CreateNewAzureDeprecationV1Message();

            // Act
            var issueContent = IssueFactory.GenerateNewDeprecationNotice(newAzureDeprecationV1Message);

            // Assert
            Assert.Equal(ExpectedIssueContent, issueContent);
        }

        private NewAzureDeprecationV1Message CreateNewAzureDeprecationV1Message()
        {
            var newAzureDeprecationV1Message = new NewAzureDeprecationV1Message
            {
                Title = "Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.",
                Notice = new Notice
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
                Contact = new Contact
                {
                    Type = ContactType.NotAvailable
                },
                DueOn = new DateTimeOffset(2021,5,24,0,0,0,TimeSpan.Zero),
                AdditionalInformation = "https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3"
            };

            return newAzureDeprecationV1Message;
        }
    }
}
