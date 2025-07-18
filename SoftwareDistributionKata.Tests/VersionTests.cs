using NUnit.Framework;
using SoftwareDistributionKata.Core;

namespace SoftwareDistributionKata.Tests
{
    [TestFixture]
    public class VersionTests
    {
        private Service service;

        [SetUp]
        public void Setup()
        {
            service = new Service();
        }

        [TestCase("1.0.0", "1.0.0", true)]
        [TestCase("1.0.1", "1.0.0", true)]
        [TestCase("1.1.0", "1.0.9", true)]
        [TestCase("2.0.0", "1.9.9", true)]
        [TestCase("1.0.0", "1.0.1", false)]
        [TestCase("1.0.0", "2.0.0", false)]
        [TestCase("1.0.0", "1.1.0", false)]
        [TestCase("1.13.0", "1.2.0", true)]
        public void IsVersionNewerOrSame_Parametrized(string version1, string version2, bool expected)
        {
            var result = service.IsVersionNewerOrSame(version1, version2);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
