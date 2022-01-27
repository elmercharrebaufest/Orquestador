using System.Web.Mvc;
using Molinos.Orquest.Web.Helpers;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Helpers
{
    [TestFixture]
    public class TestExtensionesHtml
    {
        private HtmlHelper htmlHelp;

        [SetUp]
        public void SetUp()
        {
            htmlHelp = new HtmlHelper(new ViewContext(), new ViewPage());
        }

        [Test]
        public void BotonLink()
        {
            var htmlHelpit = htmlHelp.BotonLink("Boton1", "action1", "controller1", new object());      
            Assert.That(htmlHelpit, Is.Not.Null);
        }

        [Test]
        public void BotonId()
        {
            var htmlHelpit = htmlHelp.BotonId("Boton1", 1);
            Assert.That(htmlHelpit, Is.Not.Null);
        }

        [Test]
        public void RadioBoton()
        {
            var htmlHelpit = htmlHelp.RadioBoton("label","name","value");
            Assert.That(htmlHelpit, Is.Not.Null);
        }

        [Test]
        public void IconoColor()
        {
            var htmlHelpit = htmlHelp.IconoColor("color");
            Assert.That(htmlHelpit, Is.Not.Null);
        }

        [Test]
        public void Breadcrumb()
        {
            var htmlHelpit = htmlHelp.Breadcrumb("label", "name");
            Assert.That(htmlHelpit, Is.Not.Null);
        }
    }
}
