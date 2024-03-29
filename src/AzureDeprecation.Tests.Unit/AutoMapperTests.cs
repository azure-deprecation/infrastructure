﻿using AutoFixture;
using AutoMapper;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.Mappings;
using AzureDeprecation.Contracts.v1.Documents;
using AzureDeprecation.Notices.Management.Mappings;
using AzureDeprecation.Tests.Unit.Generator;
using Bogus;
using DeepEqual.Syntax;
using FluentAssertions;
using Octokit;
using Xunit;
using DeprecationInfo = AzureDeprecation.Contracts.v1.Shared.DeprecationInfo;
using PublishedNotice = AzureDeprecation.Contracts.v1.Shared.PublishedNotice;

namespace AzureDeprecation.Tests.Unit
{
    [Trait("Category", "Unit")]
    public class AutoMapperTests
    {
        readonly IMapper _mapper;

        public AutoMapperTests()
        {
            var mappingProfile = new MappingProfile();
            var deprecationNoticeProfile = new DeprecationNoticeProfile();
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(deprecationNoticeProfile);
                cfg.AddProfile(mappingProfile);
            });
            
            _mapper = mapperConfiguration.CreateMapper();
        }

        [Fact]
        public void AutoMapper_EnsureValidMappingConfiguration_Succeeds()
        {
            // Assert
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }

        [Fact]
        public void AutoMapper_MapRegisterDeviceRequestToLocation_IsValid()
        {
            // Arrange
            var githubIssue = CreateBogusGitHubIssue();

            // Act
            var publishedNotice = _mapper.Map<Issue, PublishedNotice>(githubIssue);

            // Assert
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
            // Arrange
            var deprecationInfo = NewAzureDeprecationGenerator.GenerateSample(useAdvancedTimeline: true);

            // Act
            var publishedNotice = _mapper.Map<NewAzureDeprecationV1Message, DeprecationInfo>(deprecationInfo);

            // Assert
            Assert.NotNull(publishedNotice);
            Assert.NotNull(publishedNotice.Contact);
            Assert.NotNull(publishedNotice.Timeline);
            Assert.Equal(deprecationInfo.Title, publishedNotice.Title);
            Assert.Equal(deprecationInfo.RequiredAction?.Description, publishedNotice.RequiredAction);
            Assert.Equal(deprecationInfo.AdditionalInformation, publishedNotice.AdditionalInformation);
            Assert.Equal(deprecationInfo.Contact.Count, publishedNotice.Contact.Count);
            foreach (var contactEntry in deprecationInfo.Contact)
            {
                Assert.Contains(publishedNotice.Contact, entry => entry.Type == contactEntry.Type && string.Equals(entry.Data, contactEntry.Data, StringComparison.InvariantCultureIgnoreCase));
            }
            HasSameNotice(publishedNotice, deprecationInfo);
            HasSameImpact(deprecationInfo, publishedNotice);
            HasSameTimeline(deprecationInfo, publishedNotice);
        }

        [Fact]
        public void AutoMapper_MapNewDeprecationNoticePublishedV1MessageToDeprecationNoticeDocument_IsValid()
        {
            // Arrange
            var deprecationInfo = NewAzureDeprecationGenerator.GenerateNewDeprecationNoticePublishedV1Message(useAdvancedTimeline: true);

            // Act
            var deprecationNoticeDocument = _mapper.Map<NewDeprecationNoticePublishedV1Message, DeprecationNoticeDocument>(deprecationInfo);

            // Assert
            Assert.NotNull(deprecationNoticeDocument);
            Assert.Null(deprecationNoticeDocument.CreatedAt); // This is not mapped, only specifically assigned
            Assert.Null(deprecationNoticeDocument.LastUpdatedAt); // This is not mapped, only specifically assigned
            Assert.False(string.IsNullOrWhiteSpace(deprecationNoticeDocument.Id));
            Assert.Equal("v1", deprecationNoticeDocument.SchemaVersion);
            Assert.NotNull(deprecationNoticeDocument.DeprecationInfo);
            Assert.NotNull(deprecationNoticeDocument.DeprecationInfo.Notice);
            Assert.Equal(deprecationInfo!.DeprecationInfo!.Title, deprecationNoticeDocument.DeprecationInfo.Title);
            Assert.Equal(deprecationInfo.DeprecationInfo.RequiredAction, deprecationNoticeDocument.DeprecationInfo.RequiredAction);
            Assert.Equal(deprecationInfo.DeprecationInfo.AdditionalInformation, deprecationNoticeDocument.DeprecationInfo.AdditionalInformation);
            Assert.Equal(deprecationInfo.DeprecationInfo.Contact.Count, deprecationNoticeDocument.DeprecationInfo.Contact.Count);
            foreach (var contactEntry in deprecationNoticeDocument.DeprecationInfo.Contact)
            {
                Assert.Contains(deprecationInfo.DeprecationInfo.Contact, entry => entry.Type == contactEntry.Type && string.Equals(entry.Data, contactEntry.Data, StringComparison.InvariantCultureIgnoreCase));
            }
            Assert.Equal(deprecationInfo.DeprecationInfo!.Notice!.Description, deprecationNoticeDocument.DeprecationInfo.Notice.Description);
            Assert.Equal(deprecationInfo.DeprecationInfo.Notice.Links, deprecationNoticeDocument.DeprecationInfo.Notice.Links);
            Assert.NotNull(deprecationNoticeDocument.DeprecationInfo.Impact);
            Assert.NotNull(deprecationNoticeDocument.DeprecationInfo.Impact.Services);
            Assert.Equal(deprecationInfo.DeprecationInfo.Impact?.Area, deprecationNoticeDocument.DeprecationInfo.Impact.Area);
            Assert.Equal(deprecationInfo.DeprecationInfo.Impact?.Cloud, deprecationNoticeDocument.DeprecationInfo.Impact.Cloud);
            Assert.Equal(deprecationInfo.DeprecationInfo.Impact?.Description, deprecationNoticeDocument.DeprecationInfo.Impact.Description);
            Assert.Equal(deprecationInfo.DeprecationInfo.Impact?.Type, deprecationNoticeDocument.DeprecationInfo.Impact.Type);
            deprecationNoticeDocument.DeprecationInfo.Impact.Services.Should().BeEquivalentTo(deprecationInfo.DeprecationInfo.Impact?.Services, options => options.ComparingRecordsByValue());
            Assert.NotNull(deprecationNoticeDocument.DeprecationInfo.Timeline);
            Assert.Equal(deprecationNoticeDocument.DeprecationInfo.Timeline.Count, deprecationNoticeDocument.DeprecationInfo.Timeline.Count);
            deprecationNoticeDocument.DeprecationInfo.Timeline.Should().BeEquivalentTo(deprecationNoticeDocument.DeprecationInfo.Timeline, options => options.ComparingRecordsByValue());
            Assert.NotNull(deprecationNoticeDocument.PublishedNotice);
            Assert.Equal(deprecationInfo.PublishedNotice!.Title, deprecationNoticeDocument.PublishedNotice.Title);
            Assert.Equal(deprecationInfo.PublishedNotice.CreatedAt, deprecationNoticeDocument.PublishedNotice.CreatedAt);
            Assert.Equal(deprecationInfo.PublishedNotice.UpdatedAt, deprecationNoticeDocument.PublishedNotice.UpdatedAt);
            Assert.Equal(deprecationInfo.PublishedNotice.ClosedAt, deprecationNoticeDocument.PublishedNotice.ClosedAt);
            Assert.NotNull(deprecationNoticeDocument.PublishedNotice.DashboardInfo);
            Assert.Equal(deprecationInfo!.PublishedNotice!.DashboardInfo!.Id, deprecationNoticeDocument.PublishedNotice.DashboardInfo.Id);
            Assert.Equal(deprecationInfo.PublishedNotice.DashboardInfo.Url, deprecationNoticeDocument.PublishedNotice.DashboardInfo.Url);
            Assert.NotNull(deprecationNoticeDocument.PublishedNotice.ApiInfo);
            Assert.Equal(deprecationInfo.PublishedNotice.ApiInfo!.Id, deprecationNoticeDocument.PublishedNotice.ApiInfo.Id);
            Assert.Equal(deprecationInfo.PublishedNotice.ApiInfo.Url, deprecationNoticeDocument.PublishedNotice.ApiInfo.Url);
            Assert.NotNull(deprecationNoticeDocument.PublishedNotice.Labels);
        }

        [Fact]
        public void AutoMapper_MapNoticeEntityToDeprecationInfoApiContract_AllPropertiesMapped()
        {
            var fixture = new Fixture();
            var dbEntity = fixture.Create<DeprecationNoticeDocument>();

            var resultModel = _mapper.Map<APIs.REST.Contracts.DeprecationInfo>(dbEntity);
            
            Assert.Equal(dbEntity.Id, resultModel.Id);
            Assert.Contains(ExternalLinkType.GitHubNoticeUrl, (IDictionary<ExternalLinkType, string>)resultModel.Links);
            Assert.Equal(dbEntity.PublishedNotice?.DashboardInfo?.Url, resultModel.Links[ExternalLinkType.GitHubNoticeUrl]);
            
            resultModel
                .WithDeepEqual(dbEntity.DeprecationInfo)
                .IgnoreDestinationProperty(x => x!.AdditionalInformation)
                .IgnoreSourceProperty(x => x.Id)
                .IgnoreDestinationProperty(x => x!.Timeline)
                .IgnoreSourceProperty(x => x.Links)
                .Assert();
        }

        static void HasSameNotice(DeprecationInfo publishedNotice, NewAzureDeprecationV1Message deprecationInfo)
        {
            Assert.NotNull(publishedNotice.Notice);
            Assert.NotNull(deprecationInfo.Notice);
            Assert.Equal(deprecationInfo.Notice.Description, publishedNotice.Notice.Description);
            Assert.Equal(deprecationInfo.Notice.Links, publishedNotice.Notice.Links);
        }

        static void HasSameImpact(NewAzureDeprecationV1Message deprecationInfo, DeprecationInfo publishedNotice)
        {
            Assert.NotNull(publishedNotice.Impact);
            Assert.NotNull(publishedNotice.Impact.Services);
            Assert.Equal(deprecationInfo.Impact?.Area, publishedNotice.Impact.Area);
            Assert.Equal(deprecationInfo.Impact?.Cloud, publishedNotice.Impact.Cloud);
            Assert.Equal(deprecationInfo.Impact?.Description, publishedNotice.Impact.Description);
            Assert.Equal(deprecationInfo.Impact?.Type, publishedNotice.Impact.Type);
            publishedNotice.Impact.Services.Should().BeEquivalentTo(
                deprecationInfo.Impact?.Services,
                options => options.ComparingRecordsByValue());
        }

        static void HasSameTimeline(NewAzureDeprecationV1Message deprecationInfo, DeprecationInfo publishedNotice)
        {
            Assert.NotNull(publishedNotice.Timeline);
            Assert.Equal(deprecationInfo.Timeline.Count, publishedNotice.Timeline.Count);
            deprecationInfo.Timeline.Should().BeEquivalentTo(
                publishedNotice.Timeline,
                options => options.ComparingRecordsByValue());
        }

        Issue CreateBogusGitHubIssue()
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
                .RuleFor(s => s.Labels, _ => labels)
                .Generate();

            return queryResponseDevice;
        }
    }
}