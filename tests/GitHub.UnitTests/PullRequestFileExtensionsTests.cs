using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Xunit;

namespace GitHub.UnitTests
{
    public class PullRequestFileExtensionsTests
    {
        [Fact]
        public async Task TestExtensions()
        {
            var client = new GitHubClient(new ProductHeaderValue("my-cool-app"));
            List<PullRequestFile> files = (await client.PullRequest.Files("dotnet", "docs", 23395).ConfigureAwait(false)).ToList();
            PullRequestFile addedFile = files.Single(f => f.FileName == ".github/workflows/markdown-links-verifier.yml");
            Assert.True(addedFile.IsAdded());
            Assert.False(addedFile.IsRenamed());
            Assert.Null(addedFile.PreviousFileName);

            PullRequestFile modifiedFile = files.Single(f => f.FileName.EndsWith("containerized-lifecycle/Microsoft-platform-tools-containerized-apps/index.md", StringComparison.Ordinal));
            Assert.False(modifiedFile.IsAdded());
            Assert.False(modifiedFile.IsRenamed());
            Assert.Null(modifiedFile.PreviousFileName);

            PullRequestFile renamedFile = files.Single(f => f.FileName.EndsWith("ic-end-to-enddpcker-app-life-cycle.png", StringComparison.Ordinal));
            Assert.False(renamedFile.IsAdded());
            Assert.True(renamedFile.IsRenamed());
            Assert.NotNull(renamedFile.PreviousFileName);
        }
    }
}
