using AzureDeprecation.Contracts.Enum;
using AzureDeprecation.Contracts.v1.Shared;
using AzureDeprecation.Notices.Management;
using AzureDeprecation.Tests.Unit.Generator;
using Xunit;

namespace AzureDeprecation.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class IssueFactoryTests
    {
        [Fact]
        public void GenerateNewDeprecationNotice_ValidMessageWithBasicTimeline_GeneratesExpectedIssue()
        {
            // Arrange
            const string ExpectedIssueContent = @"Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.

**Deadline:** May 24, 2021
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

### Timeline

| Phase | Date | Description |
|:------|------|-------------|
|Deprecation|May 24, 2021|Feature will no longer work|

### Impact

Service will no longer be available.

### Required Action

Here's the official report from Microsoft:

> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.
> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal.

### Contact

Contact the product group through email ([email](mailto:example@example.com)).

### More information

https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3";

            var newAzureDeprecationV1Message = NewAzureDeprecationGenerator.GenerateSample();

            // Act
            var issueContent = IssueFactory.GenerateNewDeprecationNotice(newAzureDeprecationV1Message).ReplaceLineEndings();

            // Assert
            Assert.Equal(ExpectedIssueContent, issueContent);
        }

        [Fact]
        public void GenerateNewDeprecationNotice_ValidMessageWithMultipleSupportOptions_GeneratesExpectedIssue()
        {
            // Arrange
            const string ExpectedIssueContent = @"Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.

**Deadline:** May 24, 2021
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

### Timeline

| Phase | Date | Description |
|:------|------|-------------|
|Deprecation|May 24, 2021|Feature will no longer work|

### Impact

Service will no longer be available.

### Required Action

Here's the official report from Microsoft:

> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.
> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal.

### Contact

You can get in touch through the following options:
- Contact the product group through email ([email](mailto:example@example.com)).
- Get answers from Microsoft Q&A ([link](https://example.com)).

### More information

https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3";

            var newAzureDeprecationV1Message = NewAzureDeprecationGenerator.GenerateSample();
            newAzureDeprecationV1Message.Contact = new List<ContactEntry>
            {
                new ContactEntry{ Type = ContactType.Email, Data = "example@example.com" },
                new ContactEntry{ Type = ContactType.MicrosoftQAndA, Data = "https://example.com" },
            };

            // Act
            var issueContent = IssueFactory.GenerateNewDeprecationNotice(newAzureDeprecationV1Message).ReplaceLineEndings();

            // Assert
            Assert.Equal(ExpectedIssueContent, issueContent);
        }

        [Fact]
        public void GenerateNewDeprecationNotice_ValidMessageWithAdvancedTimeline_GeneratesExpectedIssue()
        {
            // Arrange
            const string ExpectedIssueContent = @"Azure Cognitive Services Translator v2 is deprecated by 24 May 2021.

**Deadline:** May 24, 2021
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

### Timeline

| Phase | Date | Description |
|:------|------|-------------|
|Creation Disabled|Jan 01, 2021|No longer able to create new resources with type V1|
|Deprecation|May 24, 2021|Feature will no longer work|

### Impact

Service will no longer be available.

### Required Action

Here's the official report from Microsoft:

> - If you are using version 2 of Translator, please [migrate your applications to version 3](https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3) before 24 May 2021 to avoid service disruption.
> - Your current Translator subscription and key will work with version 3, there is no need to create a new Translator subscription in the Azure portal.

### Contact

Contact the product group through email ([email](mailto:example@example.com)).

### More information

https://docs.microsoft.com/en-us/azure/cognitive-services/translator/migrate-to-v3";

            var newAzureDeprecationV1Message = NewAzureDeprecationGenerator.GenerateSample(useAdvancedTimeline: true);

            // Act
            var issueContent = IssueFactory.GenerateNewDeprecationNotice(newAzureDeprecationV1Message).ReplaceLineEndings();

            // Assert
            Assert.Equal(ExpectedIssueContent, issueContent);
        }
    }
}