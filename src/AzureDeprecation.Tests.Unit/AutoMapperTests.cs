using System.Collections.Generic;
using AutoMapper;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Notices.Management.Mappings;
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

        private Issue CreateBogusGitHubIssue()
        {
            var labels = new List<Label>();

            for(int i = 0; i < 10; i++)
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
