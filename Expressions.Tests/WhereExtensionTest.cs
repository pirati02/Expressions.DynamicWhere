using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Expression.Extensions;
using FluentAssertions;
using Xunit;

namespace Expressions.Tests
{
    public class WhereExtensionTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void SHOULD_RETURN_EMPTY_WHEN_TERM_IS_NULL_OR_EMPTY(string term)
        {
            //Arrange
            var list = GetUserList();
            var properties = new[] {"Address", "Position"};

            //Act
            var filtered = list.Filter(term, properties).ToList();

            //Assert
            filtered.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData("Unkown", "Unkown", "Unkown")]
        public void SHOULD_RETURN_EMPTY_WHEN_KEY_PROPERTIES_IS_EMPTY_OR_IVALID_PROPERTIES(string prop1, string prop2,
            string prop3)
        {
            //Arrange
            var list = GetUserList();
            var term = "address 2";
            var keyProperties = new List<string>
            {
                prop1, prop2, prop3
            }.Where(a => !string.IsNullOrEmpty(a));

            //Act
            var filtered = list.Filter(term, keyProperties).ToList();

            //Assert
            filtered.Should().NotBeNull();
            filtered.Should().BeEmpty();
        }

        [Theory]
        [InlineData("address 2", "Address", "Position", "WorkStatus", 2)]
        [InlineData("user4", "Address", "Position", "Name", 2)]
        [InlineData("user4", "", "", "Name", 1)]
        [InlineData("user1", "Address", "Position", "Name", 1)]
        public void DYNAMIC_WHERE_SHOULD_RETURN_DATA(string query, string prop1, string prop2,
            string prop3, int dataCount)
        {
            //Arrange
            var list = GetUserList();
            var keyProperties = new[]
            {
                prop1, prop2, prop3
            }.Where(a => !string.IsNullOrEmpty(a)).ToList();

            //Act
            var filteredList = list.Filter(query, keyProperties).ToList();

            //Assert
            dataCount.Should().BeGreaterThan(0);
            filteredList.Should().NotBeEmpty();
            filteredList.Should().HaveCount(dataCount);
        }

        [Theory]
        [InlineData(4, "Address", "Age", "WorkStatus", 1)]
        [InlineData(12, "Address", "Age", "Unknown", 3)]
        [InlineData(1, "Address", "Age", "Name", 0)]
        public void DYNAMIC_WHERE_SHOULD_RETURN_DATA_ON_VALUE_TYPES(int ageQuery, string prop1, string prop2,
            string prop3, int dataCount)
        {
            //Arrange
            var list = GetUserList();
            var keyProperties = new[]
            {
                prop1, prop2, prop3
            };

            //Act
            var filteredList = list.Filter(ageQuery, keyProperties).ToList();

            //Assert 
            filteredList.Should().HaveCount(dataCount);
        }

        [Theory]
        [InlineData(5, 5,  "WorkStatus", "Location", 1)]
        public void DYNAMIC_WHERE_SHOULD_RETURN_DATA_ON_CUSTOM_REFERENCE_TYPES(int x, int y,
            string prop3, string prop4, int dataCount)
        {
            //Arrange
            var list = GetUserList();
            var keyProperties = new[]
            {
                prop3, prop4
            };

            //Act
            var filteredList = list.Filter(new Location
            {
                X = x, Y = y
            }, keyProperties).ToList();

            //Assert 
            filteredList.Should().HaveCount(dataCount);
        }

        [Theory]
        [InlineData("address 2", 2)]
        [InlineData("user4", 2)]
        [InlineData("user1", 1)]
        public void DYNAMIC_WHERE_SHOULD_RETURN_DATA_WITH_LAMBDA_SELECTORS(string query, int dataCount)
        {
            //Arrange
            var list = GetUserList();
            var keyProperties = new Expression<Func<User, string>>[]
            {
                x => x.Name,
                x => x.Address,
                x => x.WorkStatus,
                x => x.Position,
            };

            //Act
            var filteredList = list.Filter(query, keyProperties).ToList();

            //Assert 
            filteredList.Should().NotBeEmpty();
            filteredList.Should().HaveCount(dataCount);
        }

        private static IEnumerable<User> GetUserList()
        {
            List<User> list = new()
            {
                new()
                {
                    Age = 12,
                    Name = "user1",
                    Position = "position1",
                    Address = "user1 address 1",
                    WorkStatus = "status 1"
                },
                new()
                {
                    Age = 12,
                    Name = "user2",
                    Position = "position2",
                    Address = "user4 address 12",
                    WorkStatus = "status 2"
                },
                new()
                {
                    Age = 12,
                    Name = "user3",
                    Position = "position3",
                    Address = "address 2",
                    WorkStatus = "status 3"
                },
                new()
                {
                    Age = 4,
                    Name = "user4",
                    Position = "position4",
                    Address = "user4 address 4",
                    WorkStatus = "address 2"
                },
                new()
                {
                    Age = 7,
                    Name = "user5",
                    Position = "position5",
                    Address = "address 5",
                    WorkStatus = "address 5",
                    Location = new Location
                    {
                        X = 5,
                        Y = 5
                    }
                }
            };
            return list;
        }
    }
}