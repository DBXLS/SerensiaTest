using System.Collections.Generic;
using NUnit.Framework;
using Serensia.ApiKata;

namespace Serensia.ApiKata.Tests
{
    /// <summary>
    /// Ensemble de tests unitaires pour la classe <see cref="SernesiaSuggestion"/>.
    /// Chaque test vérifie une règle de similarité ou de tri définie dans la logique métier.
    /// </summary>
    [TestFixture]
    public class SuggestionTest
    {
        // Méthode utilitaire : simplifie l’appel à la méthode Suggestion()
        private static IReadOnlyList<string> S(string term, IEnumerable<string> choices, int n)
            => new SernesiaSuggestion().Suggestion(term, choices, n);
        // -------------------- Tests de base --------------------

        /// <summary>
        /// Vérifie que le mot exact est toujours premier,
        /// et qu’un mot très proche (une lettre changée) arrive ensuite.
        /// </summary>
        [Test]
        public void ExactMatch_Basic()
            => CollectionAssert.AreEqual(new[] { "gros", "gras" },
                S("gros", new[] { "gros", "gras", "graisse", "agressif", "go", "ros", "gro" }, 2));
        /// <summary>
        /// Cas où le mot exact apparaît dans une sous-chaîne d’un mot plus long.
        /// </summary>
        [Test]
        public void ExactMatch_Multiple()
            => CollectionAssert.AreEqual(new[] { "abc", "yabcx" },
                S("abc", new[] { "zzz", "abc", "abx", "yabcx" }, 2));
        /// <summary>
        /// Vérifie qu’un seul résultat exact est retourné si N=1.
        /// </summary>
        [Test]
        public void ExactMatch_Single()
            => CollectionAssert.AreEqual(new[] { "000" },
                S("000", new[] { "1000", "000", "010" }, 1));
        // -------------------- Tests sur la comparaison par fenêtre --------------------

        /// <summary>
        /// Teste la recherche du terme dans une sous-chaîne d’un mot plus long.
        /// </summary>
        [Test]
        public void BestWindow_InLongerWord()
            => CollectionAssert.AreEqual(new[] { "xchatx", "chaud" },
                S("chat", new[] { "xchatx", "chaud", "tcha", "hache" }, 2));
        /// <summary>
        /// Vérifie le tri à égalité : par écart de longueur, puis ordre alphabétique.
        /// </summary>
        [Test]
        public void BestWindow_TieBrokenByLength()
            => CollectionAssert.AreEqual(new[] { "bleuet", "tableur" },
                S("bleu", new[] { "tableur", "bleuet", "blxe" }, 2));
        /// <summary>
        /// Cas où certaines entrées sont trop courtes : elles sont ignorées.
        /// Vérifie que la seule fenêtre valide est bien gardée.
        /// </summary>
        [Test]
        public void NoValidWindow_WhenAllTooShort()
        {
            CollectionAssert.AreEqual(new[] { "lonnn" }, S("long", new[] { "lo", "lonnn", "lg" }, 3));
            CollectionAssert.IsEmpty(S("aaaa", new[] { "aa", "aaa", "a" }, 2));
        }
        // -------------------- Tests de tri multi-critères --------------------

        /// <summary>
        /// Vérifie que le tri respecte la priorité :
        /// distance -> écart de longueur -> ordre alphabétique.
        /// </summary>
        [Test]
        public void TiesByScoreThenLengthThenAlpha()
        {
            CollectionAssert.AreEqual(new[] { "pierre", "pierreX", "pierreY" },
                S("pierre", new[] { "pierre", "pierreX", "bierre", "pierreY", "qierre" }, 3));

            CollectionAssert.AreEqual(new[] { "abcd", "abce", "xbcdx", "zabcq" },
                S("abcd", new[] { "xbcdx", "abce", "abcd", "zabcq" }, 4));

            // Sur le mot "code", la méthode doit classer par distance,
            // puis longueur, puis ordre alphabétique.
            CollectionAssert.AreEqual(new[] { "codec", "xcode", "acodex" },
                S("code", new[] { "xcode", "codec", "codx", "acodex" }, 3));
        }
        // -------------------- Cas particuliers --------------------

        /// <summary>
        /// Vérifie la prise en compte de plusieurs occurrences internes du mot.
        /// Exemple : "test" se trouve dans "contest" et "attested".
        /// </summary>
        [Test]
        public void MultipleInternalOccurrences_TakeBest()
        {
            CollectionAssert.AreEqual(new[] { "tEst", "contest" },
                S("test", new[] { "tEst", "attested", "contest", "zzz" }, 2));


        }
        /// <summary>
        /// Vérifie que la distance fonctionne même sur des lettres très différentes.
        /// </summary>
        [Test]
        public void VeryDifferentLetters()
        {
            CollectionAssert.AreEqual(new[] { "pqxyzr", "xya" },
                S("xyz", new[] { "abc", "xya", "pqxyzr" }, 2));

            CollectionAssert.AreEqual(new[] { "ameme", "mumu" },
                S("meme", new[] { "dodo", "mumu", "ameme" }, 2));
        }

        /// <summary>
        /// Vérifie que le nombre de suggestions n’excède jamais N,
        /// même si beaucoup de résultats sont valides.
        /// </summary>
        [Test]
        public void NGreaterThanAvailable()
            => CollectionAssert.AreEqual(new[] { "lapine", "slapin", "capin" },
                S("lapin", new[] { "lap", "slapin", "lapine", "capin" }, 10));
        /// <summary>
        /// Cas trivial : N=0 → aucun résultat.
        /// </summary>
        [Test]
        public void NEqualsZero()
            => CollectionAssert.IsEmpty(S("abc", new[] { "abc", "abd" }, 0));
        /// <summary>
        /// Vérifie que les doublons dans la liste d’entrée sont conservés.
        /// </summary>
        [Test]
        public void Duplicates_KeptAsInInput()
            => CollectionAssert.AreEqual(new[] { "soleil", "soleil", "soleilx" },
                S("soleil", new[] { "soleil", "soleil", "soleilx", "sopeil" }, 3));

        [Test]
        // -------------------- Tests d’égalité et tri alphabétique --------------------

        /// <summary>
        /// Cas où toutes les distances sont égales → tri purement alphabétique.
        /// </summary>
        public void PureAlphabeticalTieBreak()
        {
            CollectionAssert.AreEqual(new[] { "baaa", "caaa", "daaa" },
                S("aaaa", new[] { "baaa", "caaa", "daaa" }, 3));

            CollectionAssert.AreEqual(new[] { "nota", "noto" },
                S("note", new[] { "noto", "nota", "notz" }, 2));
        }
        // -------------------- Tests divers --------------------

        /// <summary>
        /// Vérifie que la correspondance peut être trouvée en début ou fin de mot.
        /// </summary>
        [Test]
        public void Borders_BeginEnd()
            => CollectionAssert.AreEqual(new[] { "rebord", "bordure", "deborde" },
                S("bord", new[] { "bordure", "rebord", "deborde" }, 3));
        /// <summary>
        /// Test sur des chaînes numériques.
        /// </summary>
        [Test]
        public void NumericStrings()
            => CollectionAssert.AreEqual(new[] { "x1234x", "0234", "1299" },
                S("1234", new[] { "x1234x", "0234", "1299", "5678" }, 3));
        /// <summary>
        /// Cas où le terme est plus long que tous les candidats → aucun résultat possible.
        /// </summary>
        [Test]
        public void TermLongerThanAllChoices()
            => CollectionAssert.IsEmpty(S("longueur", new[] { "lon", "longue", "longu" }, 3));
        /// <summary>
        /// Entrées invalides (non alphanumériques) → ignorées.
        /// </summary>
        [Test]
        public void SameLengthButNoAlnumWindow()
            => CollectionAssert.IsEmpty(S("abcd", new[] { "a_c_d", "a-b-d" }, 2));

        /// <summary>
        /// Cas de gros tie : plusieurs candidats avec score 0 → tri alphabétique à la fin.
        /// </summary>
        [Test]
        public void LargeTie_MixedLengthAndAlpha()
            => CollectionAssert.AreEqual(new[] { "brock", "crock", "frock", "prock", "rocket" },
                S("rock", new[] { "brock", "frock", "rocket", "crock", "roxx", "prock", "xrockx" }, 5));

        /// <summary>
        /// Cas d’égalité totale, tri purement alphabétique.
        /// </summary>
        [Test]
        public void AllEqual_SortedAlphabeticallyAfterZeroes()
            => CollectionAssert.AreEqual(new[] { "zzzabczzz", "abx", "axc", "xbc" },
                S("abc", new[] { "xbc", "axc", "abx", "zzzabczzz" }, 5));
    }
}
