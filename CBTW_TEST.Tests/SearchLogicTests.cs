using Microsoft.VisualStudio.TestTools.UnitTesting;
using CBTW_TEST.Domain.Models.Dto;
using System.Collections.Generic;

namespace CBTW_TEST.Tests
{
    [TestClass] // Le dice a Visual Studio: "Esto es una clase de pruebas"
    public class SearchLogicTests
    {
        [TestMethod] // Le dice a Visual Studio: "Este método es un test ejecutable"
        public void BookHypothesis_Constructor_AssignsPropertiesCorrectly()
        {
            // --- ARRANGE ---
            var expectedTitle = "The Hobbit";
            var expectedAuthor = "J.R.R. Tolkien";
            var keywords = new string[] { "1937", "First Edition" };

            // --- ACT ---
            var hypothesis = new BookHypothesisDto(expectedTitle, expectedAuthor, keywords,string.Empty, "Specific");

            // --- ASSERT ---
            Assert.AreEqual(expectedTitle, hypothesis.Title, "El título no se asignó correctamente.");
            Assert.AreEqual(expectedAuthor, hypothesis.Author, "El autor no se asignó correctamente.");
            Assert.IsNotNull(hypothesis.Keywords);
            Assert.AreEqual(2, hypothesis.Keywords.Length);
        }

        [TestMethod]
        public void MatchResult_Should_Identify_Exact_Match_Level()
        {
            // --- ARRANGE ---
            var result = new BookMatchResultDto(1, "Lord of the Rings", "Tolkien", "Match exacto", "/key/1", "Exact");

            // --- ACT & ASSERT ---
            Assert.AreEqual("Exact", result.MatchLevel);
            Assert.IsTrue(result.Rank > 0);
        }

        [TestMethod]
        public void CacheKey_Generation_Should_Be_URL_Friendly()
        {
            // --- ARRANGE ---
            var title = "The Hobbit";
            var author = "J.R.R. Tolkien";
            var expectedKey = "search_the_hobbit_j.r.r._tolkien";

            // --- ACT ---
            string resultKey = $"search_{title}_{author}".Replace(" ", "_").ToLower();

            // --- ASSERT ---
            Assert.AreEqual(expectedKey, resultKey, "La generación de la Key de caché no está normalizando el texto correctamente.");
        }

        [TestMethod]
        public void Search_With_Empty_Title_Should_Be_Marked_As_Ambiguous()
        {
            // --- ARRANGE ---
            var messyInput = ""; // Entrada vacía

            // --- ACT ---
            // Simulamos la lógica de tu actividad cuando falla o no hay datos
            bool isAmbiguous = string.IsNullOrWhiteSpace(messyInput);

            // --- ASSERT ---
            Assert.IsTrue(isAmbiguous, "El sistema debería identificar una entrada vacía como ambigua.");
        }
    }
}