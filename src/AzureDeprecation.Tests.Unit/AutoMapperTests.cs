using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Notices.Management.Mappings;
using AzureDeprecation.Tests.Unit.Generator;
using Bogus;
using Octokit;
using Xunit;

namespace AzureDeprecation.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class AutoMapperTests
    {
        private readonly IMapper _mapper;

        public AutoMapperTests()
        {
            var mappingProfile = new MappingProfile();
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfile(mappingProfile));
            _mapper = mapperConfiguration.CreateMapper();
        }

        [Fact]
        public void AutoMapper_EnsureValidMappingConfiguration_Succeeds()
        {
            //Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void AutoMapper_MapRegisterDeviceRequestToLocation_IsValid()
        {
            //Arrange
            var githubIssue = CreateBogusGitHubIssue();

            //Act
            var publishedNotice = _mapper.Map<Issue, PublishedNotice>(githubIssue);

            //Assert
            Assert.NotNull(publishedNotice);
            Assert.NotNull(publishedNotice.ApiInfo);
            Assert.NotNull(publishedNotice.DashboardInfo);
            Assert.NotNull(publishedNotice.Labels);
            Assert.Equal(githubIssue.Title, publishedNotice.Title);
            Assert.Equal(githubIssue.CreatedAt, publishedNotice.CreatedAt);
            Assert.Equal(githubIssue.UpdatedAt, publishedNotice.UpdatedAt);
            Assert.Equal(githubIssue.ClosedAt, publishedNotice.ClosedAt);
            Assert.Equal(githubIssue.Id, publishedNotice.ApiInfo.Id);
            Assert.Equal(githubIssue.Url, publishedNotice.ApiInfo.Url);
            Assert.Equal(githubIssue.Number, publishedNotice.DashboardInfo.Id);
            Assert.Equal(githubIssue.HtmlUrl, publishedNotice.DashboardInfo.Url);
            Assert.Equal(githubIssue.Labels.Count, publishedNotice.Labels.Count);
            foreach (var gitHubLabel in githubIssue.Labels)
            {
                Assert.True(publishedNotice.Labels.Contains(gitHubLabel.Name), $"Notice does not contain label '{gitHubLabel.Name}'");
            }
        }

        [Fact]
        public void AutoMapper_MapNewAzureDeprecationV1MessageToDeprecationInfo_IsValid()
        {
            //Arrange
            var deprecationInfo = NewAzureDeprecationGenerator.GenerateSample(useAdvancedTimeline: true);

            //Act
            var publishedNotice = _mapper.Map<NewAzureDeprecationV1Message, DeprecationInfo>(deprecationInfo);

            //Assert
            Assert.NotNull(publishedNotice);
            Assert.NotNull(publishedNotice.Contact);
            Assert.NotNull(publishedNotice.Timeline);
            Assert.Equal(deprecationInfo.Title, publishedNotice.Title);
            Assert.Equal(deprecationInfo.RequiredAction.Description, publishedNotice.RequiredAction);
            Assert.Equal(deprecationInfo.Contact.Type, publishedNotice.Contact.Type);
            Assert.Equal(deprecationInfo.AdditionalInformation, publishedNotice.AdditionalInformation);
            HasSameNotice(publishedNotice, deprecationInfo);
            HasSameImpact(deprecationInfo, publishedNotice);
        }

        private static void HasSameNotice(DeprecationInfo publishedNotice, NewAzureDeprecationV1Message deprecationInfo)
        {
            Assert.NotNull(publishedNotice.Notice);
            Assert.Equal(deprecationInfo.Notice.Description, publishedNotice.Notice.Description);
            Assert.Equal(deprecationInfo.Notice.Links, publishedNotice.Notice.Links);
        }

        private static void HasSameImpact(NewAzureDeprecationV1Message deprecationInfo, DeprecationInfo publishedNotice)
        {
            Assert.NotNull(publishedNotice.Impact);
            Assert.NotNull(publishedNotice.Impact.Services);
            Assert.Equal(deprecationInfo.Impact.Area, publishedNotice.Impact.Area);
            Assert.Equal(deprecationInfo.Impact.Cloud, publishedNotice.Impact.Cloud);
            Assert.Equal(deprecationInfo.Impact.Description, publishedNotice.Impact.Description);
            Assert.Equal(deprecationInfo.Impact.Type, publishedNotice.Impact.Type);
            Assert.Equal(deprecationInfo.Impact.Services.Count, publishedNotice.Impact.Services.Count);
            foreach (var impactedService in publishedNotice.Impact.Services)
            {
                Assert.Contains(impactedService, deprecationInfo.Impact.Services);
            }
        }

        private static void HasSameTimeline(NewAzureDeprecationV1Message deprecationInfo, DeprecationInfo publishedNotice)
        {
            Assert.NotNull(publishedNotice.Timeline);
            Assert.Equal(deprecationInfo.Timeline.Count, publishedNotice.Timeline.Count);
            foreach (var timelineEntry in publishedNotice.Timeline)
            {
                var matchCount = deprecationInfo.Timeline.Count(entry =>
                    entry.Date == timelineEntry.Date &&
                    entry.Description.Equals(timelineEntry.Description, StringComparison.InvariantCultureIgnoreCase) &&
                    entry.Phase.Equals(timelineEntry.Phase, StringComparison.InvariantCultureIgnoreCase));
                Assert.Equal(1, matchCount);
            }
        }

        private Issue CreateBogusGitHubIssue()
        {
            var labels = new List<Label>();

            for (int i = 0; i < 10; i++)
            {
                var label = new Faker<Label>()
                    .RuleFor(s => s.Name, f => f.Name.FirstName())
                    .RuleFor(s => s.Url, f => f.Internet.Url());

                labels.Add(label);
            }

            var queryResponseDevice = new Faker<Issue>()
                .RuleFor(s => s.Title, f => f.Lorem.Sentence())
                .RuleFor(s => s.Id, f => f.Random.Int())
                .RuleFor(s => s.Number, f => f.Random.Int())
                .RuleFor(s => s.CreatedAt, f => f.Date.Past())
                .RuleFor(s => s.ClosedAt, f => f.Date.Future())
                .RuleFor(s => s.UpdatedAt, f => f.Date.Past())
                .RuleFor(s => s.Url, f => f.Internet.Url())
                .RuleFor(s => s.HtmlUrl, f => f.Internet.Url())
                .RuleFor(s => s.Labels, f => labels)
                .Generate();

            return queryResponseDevice;
        }
    }
}
