using Xunit;

namespace RemoteDev.Core.Tests
{
    public class GitIgnoreParserTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        [InlineData("# Comment")]
        public void GitIgnoreParser_NullLine_Test(string inputLine)
            => Assert.Null(GitIgnoreParser.ParseLine(inputLine));

        [Theory]
        [InlineData("**/*.jpg", "images/pic.jpg", true, true)]
        [InlineData("**/*.jpg", "pic.jpg", true, true)]
        [InlineData("*.jpg", "images/pic.jpg", true, true)]
        [InlineData("*.jpg", "pic.jpg", true, true)]
        [InlineData("[Cc]ase-insensitive", "case-insensitive", true, true)]
        [InlineData("[Cc]ase-insensitive", "Case-insensitive", true, true)]
        [InlineData(".vs/", ".vs/file.txt", true, true)]
        [InlineData(".vs/", ".vs/folder/file.txt", true, true)]
        [InlineData("remote", "remote.txt", true, false)]
        [InlineData("remote", "folder/remote", true, false)]
        public void GitIgnoreParser_Exclusion_Test(string pattern, string relativePath, bool isFile, bool expectMatch)
            => Assert.Equal(expectMatch, GitIgnoreParser.ParseLine(pattern).IsMatch(relativePath, isFile));
    }
}
