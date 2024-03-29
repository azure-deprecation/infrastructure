﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuardNet;
using Microsoft.Extensions.Configuration;
using Octokit;

namespace AzureDeprecation.Integrations.GitHub.Repositories
{
    public class GitHubRepository
    {
        private readonly string _repoOwner;
        private readonly string _repoName;
        private readonly GitHubClient _githubClient;

        public GitHubRepository(IConfiguration configuration)
        {
            _repoOwner = configuration["GitHub_Owner"];
            _repoName = configuration["GitHub_RepoName"];

            var tokenAuth = new Credentials(configuration["GitHub_Token"]);
            _githubClient = new GitHubClient(new ProductHeaderValue(name: "arcus-automation-app"))
            {
                Credentials = tokenAuth
            };
        }

        public async Task PostCommentAsync(int issueNumber, string commentText)
        {
            Guard.NotNullOrEmpty(commentText, nameof(commentText));

            await _githubClient.Issue.Comment.Create(_repoOwner, _repoName, issueNumber, commentText);
        }

        public async Task LockIssueAsync(int issueNumber)
        {
            await _githubClient.Issue.LockUnlock.Lock(_repoOwner, _repoName, issueNumber);
        }

        public async Task<Issue> CreateIssueAsync(string title, string content, Milestone milestone, List<string> labels)
        {
            var foundRepository = await GetRepositoryAsync($"{_repoOwner}/{_repoName}");
            
            var noticeIssue = new NewIssue(title)
            {
                Body = content,
                Milestone = milestone.Number
            };

            foreach (var label in labels)
            {
                noticeIssue.Labels.Add(label);
            }
            
            return await _githubClient.Issue.Create(foundRepository.Id, noticeIssue);
        }

        public async Task<Milestone> GetOrCreateMilestoneAsync(string name, string description, DateTimeOffset? dueDate = null)
        {
            var allMilestones = await _githubClient.Issue.Milestone.GetAllForRepository(_repoOwner, _repoName);
            var foundMilestone = allMilestones.FirstOrDefault(milestone => milestone.Title.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (foundMilestone == null)
            {
                var newMilestone = new NewMilestone(name)
                {
                    Description = description,
                    DueOn = dueDate
                };
                foundMilestone = await _githubClient.Issue.Milestone.Create(_repoOwner, _repoName, newMilestone);
            }

            return foundMilestone;
        }

        private async Task<Repository> GetRepositoryAsync(string repositoryName)
        {
            IReadOnlyList<Repository> allRepositories = await _githubClient.Repository.GetAllForCurrent();
            var foundRepository = allRepositories.FirstOrDefault(repo => repo.FullName.Equals(repositoryName, StringComparison.InvariantCultureIgnoreCase));
            if (foundRepository == null)
            {
                throw new Exception($"Repository '{repositoryName}' was not found. Make sure you are a collaborator on the repository and you use the full name. (such as arcus-azure/yourrepo)");
            }

            return foundRepository;
        }

        public async Task<Project> GetOrCreateProjectAsync(string projectName)
        {
            var foundRepository = await GetRepositoryAsync($"{_repoOwner}/{_repoName}");
            var allProjects = await _githubClient.Repository.Project.GetAllForRepository(foundRepository.Id);
            var foundProject = allProjects.FirstOrDefault(project => project.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase));
            if (foundProject == null)
            {
                var newProject = new NewProject(projectName)
                {
                    Body = "Dashboard for all deprecation notices per year"
                };
                foundProject = await _githubClient.Repository.Project.CreateForRepository(foundRepository.Id, newProject);
            }

            return foundProject;
        }

        public async Task AddIssueToProjectAsync(int projectId, string columnName, int issueId)
        {
            var allProjectColumns = await _githubClient.Repository.Project.Column.GetAll(projectId);
            var foundProjectColumn = allProjectColumns.FirstOrDefault(column => column.Name.Equals(columnName, StringComparison.InvariantCultureIgnoreCase));
            if (foundProjectColumn == null)
            {
                var newProjectColumn = new NewProjectColumn(columnName);
                foundProjectColumn = await _githubClient.Repository.Project.Column.Create(projectId, newProjectColumn);
            }

            var projectCard = new NewProjectCard(issueId, ProjectCardContentType.Issue);
            await _githubClient.Repository.Project.Card.Create(foundProjectColumn.Id, projectCard);
        }
    }
}
