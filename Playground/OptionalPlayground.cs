using System.Collections.Generic;
using System.Linq;
using CSharpFunctional.Options;
using FluentAssertions;
using NUnit.Framework;

namespace Playground
{
    /// <summary>
    /// Benefits
    /// * Removes null from the code, so no NullReferenceException.
    /// * Better intent, it's clear the method may or may not yield a value.
    /// * Better chaining, Map/FlatMap will only map if it's got a value, no need for branching.
    /// * Easier to read declarative code.
    /// * Allows optional to be treated as a list with 0 or 1 values.
    ///
    /// Filter:         Takes a T and turns it into an Option<T>.  Filter the predicate passes, then it's a some, else none.
    /// FirstOrNone:    Same as FirstOrDefault, where default is None, not default(T).
    /// 
    /// Map:            Takes a Option<T> performs a mapping if the Option is a Some.  Also maps None<T> to None<TResult>.  Scala Map will create an Option<Option<T>> for option maps, although this function wont.
    /// FlatMap:        Takes a stream of Option<T>s and removes all the Nones.  Since Option<T> is a sequence with count of 0 or 1, FlatMap reduces 0 collections into nothing.
    /// 
    /// GetOrElse:      Takes an Option<T> and returns Some if present, else the specified default (or default(T)).
    /// GetOrThrow:     Takes an Option<T> and returns Some if present, else throws the exception provided.
    /// Contains:       Checks whether an optional contains the value or not (defined by the predicate).
    /// 
    /// ForEach:        Allows side effects on an option (value if some, empty if none).
    /// ToList:         Returns a list of Some values (or empty list if single None).
    /// ToEnumerable:   Returns an enumerable of Some values;
    /// 
    /// Match:          A method that looks like Scala's pattern matching.  Returns Some mutation if Some, else the None mutation.
    /// </summary>
    internal sealed class OptionalPlayground
    {
        [Test]
        [TestCase("NAS", false)]
        [TestCase("Speaker",true)]
        public void FindProductExample(string productToFind, bool isFound)
        {
            GetAllProducts()
                .FirstOrNone(product => string.Equals(product, productToFind))
                .Map(product => "Found!")
                .GetOrElse("Not Found!")
                .Should()
                .Be($"{(isFound ? "" : "Not ")}Found!");
        }
        
        [Test]
        public void MapDoesNotRemoveNone()
        {
            GetAllProducts()
                .Filter<string>(product => string.Equals(product, "Speaker"))
                .Map(product => "Found!")
                .Count()
                .Should()
                .Be(4);
        }

        [Test]
        public void FlatMapRemovesNone()
        {
            GetAllProducts()
                .Filter<string>(product => string.Equals(product, "Speaker"))
                .FlatMap<string, string>(product => "Found!")
                .Count()
                .Should()
                .Be(1);
        }

        [Test]
        public void NoItemsExample()
        {
            Enumerable.Empty<string>()
                .FirstOrNone(product => string.Equals(product, "Camera"))
                .GetOrElse("Not Found!")
                .Should()
                .Be("Not Found!");
        }
        
        [Test]
        public void ChainingExample()
        {
            GetSerialisedNumbers()
                .Select(ParseInteger)
                .FlatMap<int, int>(number => 10 / number)
                .ToEnumerable()
                .Sum()
                .Should()
                .Be(26);
        }

        [Test]
        public void SingleInvalidOptionExample()
        {
            ParseInteger("NotValid")
                .Map(i => i * 2)
                .GetOrElse()
                .Should()
                .Be(0);
        }

        [Test]
        public void SingleValidOptionExample()
        {
            ParseInteger("10")
                .Map(i => i * 2)
                .GetOrElse()
                .Should()
                .Be(20);
        }

        [Test]
        public void FlatMapToFilterNoneExample()
        {
            GetSerialisedNumbers()
                .Select(ParseInteger)
                .FlatMap()
                .Count()
                .Should()
                .Be(90);
        }

        [Test]
        public void EnumerableWhenExample()
        {
            GetSerialisedNumbers()
                .Select(ParseInteger)
                .Filter(number => number < 10)
                .FlatMap()
                .Count()
                .Should()
                .Be(9);
        }

        [Test]
        public void ScalarWhenExample()
        {
            ParseInteger("10")
                .Filter(number => number > 10)
                .GetOrElse(0)
                .Should()
                .Be(0);
        }

        [Test]
        public void CreateOptionsUsingWhenExample()
        {
            Enumerable.Range(0, 10)
                      .Concat(Enumerable.Range(0, 10))
                      .Filter(number => number < 5)
                      .FlatMap()
                      .Count()
                      .Should()
                      .Be(10);

        }

        [Test]
        public void ForEachOnSingleOptionalExample()
        {
            var number = 0;
            ParseInteger("10").ForEach(num => number = num);
            number.Should().Be(10);
        }


        [Test]
        public void ForEachOnEnumerableOptionalExample()
        {
            var numbers = new List<int>();
            Enumerable.Range(0, 10)
                      .Filter(number => number > 5)
                      .ForEach(numbers.Add);
           
            numbers.Count.Should().Be(4);
        }

        [Test]
        public void NullIsNone()
        {
            Option<string> optionalString = (string)null;

            optionalString.Should().BeSameAs(None<string>.Value);
        }

        [Test]
        public void PresentValueIsSome()
        {
            Option<string> optionalString = "Hello";

            ((Some<string>)optionalString).Content.Should().BeSameAs("Hello");
        }

        [Test]
        public void NoneIsGenericNone()
        {
            Option<string> optionalString = None.Value;

            ((None<string>) optionalString).Should().NotBeNull();
        }

        [Test]
        public void ToListConvertsSomeIntoListOfOne()
        {
            Option<string> foundProduct = "Camera";
            foundProduct.ToList()[0].Should().BeSameAs("Camera");
        }

        [Test]
        public void ToListConvertsNoneIntoEmptyList()
        {
            Option<string> foundProduct = None.Value;

            foundProduct.ToList().Should().BeEmpty();
        }

        [Test]
        public void ToEnumerableConvertsSomeIntoEnumerableOfOne()
        {
            Option<string> foundProduct = "Camera";
            foundProduct.ToEnumerable().Single().Should().BeSameAs("Camera");
        }

        [Test]
        public void ToEnumerableConvertsNoneIntoEmptyEnumerable()
        {
            Option<string> foundProduct = None.Value;

            foundProduct.ToEnumerable().Should().BeEmpty();
        }

        [Test]
        public void WebApiExample()
        {
            // Using Optionals, declaritive, easy to follow, single statement.
            var controllerReturnValue1 = GetAllProducts()
                                                .FirstOrNone(product => string.Equals(product, "Speaker"))
                                                .Map(product => new { name = product })
                                                .Map(Ok)
                                                .GetOrElse(NotFound);

            // Using traditional code, implicit contract with nulls, imperitive, multiple code paths, declared variables to manage etc.
            var potentiallyFoundProduct = GetAllProducts().FirstOrDefault(product => string.Equals(product, "Speaker"));
            IActionResult controllerReturnValue2;
            if (potentiallyFoundProduct == null)
            {
                controllerReturnValue2 = NotFound();
            }
            else
            {
                controllerReturnValue2 = Ok(new { name = potentiallyFoundProduct });
            }

        }

        private static IActionResult Ok<T>(T content)
        {
            return null; // Pretend this is an Ok action result!
        }

        private static IActionResult NotFound()
        {
            return null; // Pretend this is a NotFound action result!
        }

        private static IEnumerable<string> GetAllProducts()
        {
            yield return "Computer";
            yield return "Speaker";
            yield return "Laptop";
            yield return "Headphones";
        }

        private static Option<int> ParseInteger(string item)
        {
            return int.TryParse(item, out var i) ? (Option<int>)new Some<int>(i) : None<int>.Value;
        }

        private static IEnumerable<string> GetSerialisedNumbers()
        {
            return Enumerable.Range(1, 100).Select(i => i % 10 == 0 ? "NotNumeric" + i : i.ToString());
        }

        private interface IActionResult { }
    }
}
