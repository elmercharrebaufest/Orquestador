using System.Collections.Generic;
using System.Web.Mvc;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [TestFixture]
    public class MenuControllerTest
    {
        private MenuController target;
        [SetUp]
        public void SetUp()
        {
            target = new MenuController(new NullLogger());
        }

        [Test]
        public void MenuTest()
        {
            var result = target.Menu() as PartialViewResult;

            var idiomas = (List<SelectListItem>) target.ViewBag.Idiomas;
            var idioma = (string) target.ViewBag.Idioma;

            Assert.That(idiomas[0].Text, Is.EqualTo("English ").Or.EqualTo("Español ").Or.EqualTo("English").Or.EqualTo("español "));
            Assert.That(idiomas[1].Text, Is.EqualTo("English ").Or.EqualTo("Español ").Or.EqualTo("English").Or.EqualTo("español "));

            Assert.That(idioma, Is.EqualTo("English ").Or.EqualTo("Español ").Or.EqualTo("english ").Or.EqualTo("español "));
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.Null);
            Assert.That(result.ViewName, Is.EqualTo("_Menu"));
        }
    }
}
