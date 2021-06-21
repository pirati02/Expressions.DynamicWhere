using System;
using System.Collections.Generic;
using Expression.Extensions;

namespace Expressions
{
    class Program
    {
        static void Main()
        { 
            var list = new List<User>
            {
                new()
                {
                    Name = "user1",
                    Position = "user66"
                },
                new()
                {
                    Name = "user1",
                    Position = "user3"
                },
                new()
                {
                    Name = "user3",
                    Position = "user3"
                },
                new()
                {
                    Name = "user4",
                    Position = "user4",
                    Location = new Location
                    {
                        X = 1,
                        Y = 2
                    }
                },
                new()
                {
                    Name = "user5",
                    Location = new Location
                    {
                        X = 1,
                        Y = 2
                    },
                    Position = "user5"
                },
            };

            var someFiltering = list.DynamicWhere(new Location
            {
                X = 1,
                Y = 2
            }, new[]
            {
                "Location",
                "Name",
                "Position"
            });

            foreach (var user in someFiltering)
            {
                Console.WriteLine(user.ToString());
            }
        }
    }

    public class User
    {
        public string Name { get; init; }
        public string Position { get; init; }

        public Location Location { get; init; }

        public override string ToString()
        {
            return $"[Name = {Name},\nPosition = {Position},\nLocation = {Location}]";
        }
    }

    public class Location
    {
        public int X { get; init; }
        public int Y { get; init; }

        public override string ToString()
        {
            return $"[X = {X}\nY = {Y}]";
        }

        public override bool Equals(object obj)
        {
            if (obj is Location location)
            {
                return location.X == X && location.Y == Y;
            }

            return obj == this;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}