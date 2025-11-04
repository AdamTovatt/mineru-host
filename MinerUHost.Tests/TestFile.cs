using EasyReasy;

namespace MinerUHost.Tests
{
    [ResourceCollection(typeof(EmbeddedResourceProvider))]
    internal static class TestFile
    {
        public static readonly Resource Image01 = new Resource("TestFiles/Image01.png");
        public static readonly Resource Text01 = new Resource("TestFiles/Text01.txt");
    }
}

