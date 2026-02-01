using Microsoft.VisualStudio.TestTools.UnitTesting;
using CBTW_TEST.Domain.Models.Dto; // Ajusta a tus namespaces

namespace CBTW_TEST.Tests;

[TestClass]
public class LibraryLogicTests
{
    [TestMethod]
    public void Hypothesis_Should_Keep_Data_Correctly()
    {
        // ARRANGE (Preparar)
        var title = "The Hobbit";
        var author = "Tolkien";

        // ACT (Ejecutar)
        var hypothesis = new BookHypothesisDto(title, author, new string[0], "", "Specific");

        // ASSERT (Verificar)
        Assert.AreEqual(title, hypothesis.Title);
        Assert.AreEqual(author, hypothesis.Author);
    }
}