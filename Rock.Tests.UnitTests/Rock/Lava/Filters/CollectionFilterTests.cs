// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Dynamic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Lava
{


    [TestClass]
    public class CollectionFilterTests : LavaUnitTestBase
    {
        List<string> _TestNameList = new List<string>() { "Ted", "Alisha", "Cynthia", "Brian" };
        List<string> _TestOrderedList = new List<string>() { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };

        /// <summary>
        /// Searching for strings in a collection returns correct match indicators.
        /// </summary>
        [DataTestMethod]
        [DataRow( "Ted", true )]
        [DataRow( "Brian", true )]
        [DataRow( "Cynthia", true )]
        [DataRow( "Zak", false )]
        public void Contains_SearchStringValuesReturnCorrectMatchIndicators( string searchValue, bool isFound )
        {
            var mergeValues = new LavaDictionary { { "TestList", _TestNameList } };

            var lavaTemplate = "{{ TestList | Contains:'<searchValue>' }}";
            lavaTemplate = lavaTemplate.Replace( "<searchValue>", searchValue );

            TestHelper.AssertTemplateOutput( isFound ? "true" : "false", lavaTemplate, mergeValues );
        }

        #region Filter Tests: Index

        /// <summary>
        /// Specifying an index of 0 returns the first item in the collection.
        /// </summary>
        [DataTestMethod]
        [DataRow( 0, "Item 1" )]
        [DataRow( 1, "Item 2" )]
        [DataRow( 2, "Item 3" )]
        public void Index_IndexReferencesReturnExpectedItems( int index, string expectedValue )
        {
            var mergeValues = new LavaDictionary { { "TestList", _TestOrderedList } };

            var lavaTemplate = "{{ TestList | Index:<index> }}";
            lavaTemplate = lavaTemplate.Replace( "<index>", index.ToString() );

            TestHelper.AssertTemplateOutput( expectedValue, lavaTemplate, mergeValues );

        }

        /// <summary>
        /// Specifying an index greater than the number of list items returns an empty string.
        /// </summary>
        [TestMethod]
        public void Index_IndexExceedsItemCount_ProducesEmptyString()
        {
            var mergeValues = new LavaDictionary { { "TestList", _TestNameList } };

            TestHelper.AssertTemplateOutput( "", "{{ TestList | Index:999 }}", mergeValues );
        }

        #endregion

        #region Filter Tests: OrderBy

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByMultipleTextProperties()
        {
            var members = new List<object>();

            members.Add( new
            {
                GroupRole = new { Name = "Member", IsLeader = false },
                Person = new { FirstName = "Alex" }
            } );
            members.Add( new
            {
                GroupRole = new { Name = "Leader", IsLeader = true },
                Person = new { FirstName = "Ted" }
            } );
            members.Add( new
            {
                GroupRole = new { Name = "Member", IsLeader = false },
                Person = new { FirstName = "Cindy" }
            } );

            var mergeValues = new LavaDictionary { { "Members", members } };

            TestHelper.AssertTemplateOutput( "Ted;Alex;Cindy;",
                "{% assign items = Members | OrderBy:'GroupRole.IsLeader desc,Person.FirstName' %}{% for item in items %}{{ item.Person.FirstName }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByIntegerPropertyAscending()
        {
            var mergeValues = new LavaDictionary { { "Items", GetOrderByTestCollection() } } ;

            TestHelper.AssertTemplateOutput( "A;B;C;D;",
                "{% assign items = Items | OrderBy:'Order' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByIntegerPropertyDescending()
        {
            var mergeValues = new LavaDictionary { { "Items", GetOrderByTestCollection() } };

            TestHelper.AssertTemplateOutput( "D;C;B;A;",
                "{% assign items = Items | OrderBy:'Order DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByMultipleObjectProperties()
        {
            var mergeValues = new LavaDictionary { { "Items", GetOrderByTestCollection() } };

            TestHelper.AssertTemplateOutput( "A;B;C;D;",
                "{% assign items = Items | OrderBy:'Order, SecondOrder DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Verify ordering filter applied to a list of dynamically-typed objects.
        /// </summary>
        [TestMethod]
        public void OrderBy_ExpandoObjectList_CanSortByNestedObjectProperty()
        {
            var mergeValues = new LavaDictionary { { "Items", GetOrderByTestCollection() } };

            TestHelper.AssertTemplateOutput( "A;B;C;D;",
                "{% assign items = Items | OrderBy:'Order, Nested.Order DESC' %}{% for item in items %}{{ item.Title }};{% endfor %}",
                mergeValues );
        }

        private List<ExpandoObject> GetOrderByTestCollection()
        {
            var json = @"[
    { ""Title"": ""D"", ""Order"": 4, ""SecondOrder"": 1, ""Nested"": { ""Order"": 1 } },
    { ""Title"": ""A"", ""Order"": 1, ""SecondOrder"": 2, ""Nested"": { ""Order"": 2 } },
    { ""Title"": ""C"", ""Order"": 3, ""SecondOrder"": 2, ""Nested"": { ""Order"": 2 } },
    { ""Title"": ""B"", ""Order"": 2, ""SecondOrder"": 1, ""Nested"": { ""Order"": 1 } }
]";

            var converter = new ExpandoObjectConverter();
            var input = JsonConvert.DeserializeObject<List<ExpandoObject>>( json, converter );

            return input;
        }

        #endregion

        /// <summary>
        /// Shuffle applied to an ordered list returns an unordered list.
        /// </summary>
        [TestMethod]
        public void Shuffle_AppliedToOrderedList_ReturnsUnorderedList()
        {
            var mergeValues = new LavaDictionary { { "OrderedList", _TestOrderedList } };

            var orderedOutput = _TestOrderedList.JoinStrings( ";" ) + ";";

            // First, verify that the unshuffled lists are equal.
            var orderedResult = TestHelper.GetTemplateOutput( "{% assign items = OrderedList %}{% for item in items %}{{ item }};{% endfor %}", mergeValues );

            Assert.That.Equal( orderedOutput, orderedResult );

            // Next, verify that the shuffled lists are not equal.
            // The Shuffle filter can, mathmatically, actually return the same ordered result.
            // To offset this, attempt the shuffle 10 times. If all 10 times we still get the same
            // ordered result back, then go ahead and error as something must be wrong. -dsh
            // [2020-05-05] DJL
            // Perhaps we should fix the Shuffle implementation instead? Is it ever desirable to return an unshuffled result if shuffling is at all possible?
            string shuffledResult = string.Empty;
            for ( int i = 0; i < 10; i++ )
            {
                shuffledResult = TestHelper.GetTemplateOutput( "{% assign items = OrderedList | Shuffle %}{% for item in items %}{{ item }};{% endfor %}", mergeValues );

                if ( orderedOutput != shuffledResult )
                {
                    break;
                }
            }

            Assert.That.NotEqual( orderedOutput, shuffledResult );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Select_ValidItemPropertyFromItemCollection_ReturnsValueCollection()
        {
            var personList = TestHelper.GetTestPersonCollectionForDecker();

            var mergeValues = new LavaDictionary { { "People", personList } };

            TestHelper.AssertTemplateOutput( "Edward;Cindy;Noah;Alex;",
                "{% assign names = People | Select:'FirstName' %}{% for name in names %}{{ name }};{% endfor %}",
                mergeValues );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Size_ForArrayTarget_ReturnsItemCount()
        {
            var mergeValues = new LavaDictionary { { "TestList", _TestNameList } };

            TestHelper.AssertTemplateOutput( _TestNameList.Count.ToString(), "{{ TestList | Size }}", mergeValues );
        }

        /// <summary>
        /// Selecting an existing property from a collection returns a list of values.
        /// </summary>
        [TestMethod]
        public void Size_ForStringTarget_ReturnsCharacterCount()
        {
            var testString = "123456789";

            var mergeValues = new LavaDictionary { { "TestString", testString } };

            TestHelper.AssertTemplateOutput( testString.Length.ToString(), "{{ TestString | Size }}", mergeValues );
        }
    }
}
