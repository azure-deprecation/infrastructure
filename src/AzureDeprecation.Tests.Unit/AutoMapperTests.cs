using AutoFixture;
using AutoMapper;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.Mappings;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Notices.Management.Mappings;
using AzureDeprecation.Tests.Unit.Generator;
using Bogus;
using DeepEqual.Syntax;
using FluentAssertions;
using Octokit;
using Xunit;
using DeprecationInfo = AzureDeprecation.Contracts.Messages.v1.DeprecationInfo;
using PublishedNotice = AzureDeprecation.Contracts.Messages.v1.PublishedNotice;

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
        public void AutoMapper_MapNoticeEntityToDeprecationInfoApiContract_AllPropertiesMapped()
        {
            var fixture = new Fixture();
            var dbEntity = fixture.Create<NoticeEntity>();

            var resultModel = _mapper.Map<AzureDeprecation.APIs.REST.Contracts.DeprecationInfo>(dbEntity);
            
            Assert.Equal(dbEntity.Id, resultModel.Id);
            Assert.Contains(ExternalLinkType.GitHubNoticeUrl, (IDictionary<ExternalLinkType, string>)resultModel.Links);
            Assert.Equal(dbEntity.PublishedNotice?.DashboardInfo?.Url, resultModel.Links[ExternalLinkType.GitHubNoticeUrl]);
            
            resultModel
                .WithDeepEqual(dbEntity.DeprecationInfo)
                .IgnoreDestinationProperty(x => x.AdditionalInformation)
                .IgnoreSourceProperty(x => x.Id)
                .IgnoreSourceProperty(x => x.Links)
                .Assert();
        }

        static void HasSameNotice(DeprecationInfo publishedNotice, NewAzureDeprecationV1Message deprecationInfo)
        {
            Assert.NotNull(publishedNotice.Notice);
            Assert.Equal(deprecationInfo.Notice?.Description, publishedNotice.Notice?.Description);
            Assert.Equal(deprecationInfo.Notice?.Links, publishedNotice.Notice?.Links);
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
                .RuleFor(s => s.Labels, f => labels)
                .Generate();

            return queryResponseDevice;
        }
    }
}