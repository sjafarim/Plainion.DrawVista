using Plainion.DrawVista.IO;

namespace Plainion.DrawVista.Tests
{
    [TestFixture()]
    [TestOf(typeof(DrawIOAppFactory))]
    public class DrawIOAppFactoryTest
    {
        [Test()]
        public void Singelton()
        {
            var factoryFirstInstance = DrawIOAppFactory.Instance;
            var factorySecondInstance = DrawIOAppFactory.Instance;

            Assert.AreEqual(factoryFirstInstance, factorySecondInstance);
        }

        [Test()]
        public void FactoryCanCreateDrawIOAppExtensionInstance()
        {
            var factory = DrawIOAppFactory.Instance;
            var model = new DrawIOModel(DocumentBuilder.Create("Page-1", "UserService", "User", "Database").Content.ToString());

            var drawIOAppExtension = factory.CreateDrawIOAppExtension(model);

            Assert.IsInstanceOf<DrawIOAppExtension>(drawIOAppExtension);
        }

        [Test()]
        public void FactoryCanCreateDrawIOAppBaseInstance()
        {
            var factory = DrawIOAppFactory.Instance;
            var model = new DrawIOModel(DocumentBuilder.Create("Page-1", "UserService", "User", "Database").Content.ToString());

            var drawIOAppExtension = factory.CreateDrawIOApp(model);

            Assert.IsInstanceOf<DrawIOApp>(drawIOAppExtension);
        }

    }
}