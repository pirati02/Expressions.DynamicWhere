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
                    Position = "position1",
                    Address = "user1 address 1",
                    WorkStatus = "status 1"
                },
                new()
                {
                    Name = "user2",
                    Position = "position2",
                    Address = "address 2",
                    WorkStatus = "status 2"
                },
                new()
                {
                    Name = "user3",
                    Position = "position3",
                    Address = "address 2",
                    WorkStatus = "status 3"
                },
                new()
                {
                    Name = "user4",
                    Position = "position4",
                    Address = "user4 address 4",
                    WorkStatus = "address 2"
                }
            };

            var someFiltering = list.DynamicWhere("address 2", new[]
            {
                "WorkStatus",
                "Address",
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
        public string Address { get; set; }
        public string WorkStatus { get; set; }

        public Location Location { get; init; }

        public override string ToString()
        {
            return $"[\n\tName = {Name},\n\tPosition = {Position},\n\tLocation = {Location},\n\tAddress = {Address},\n\tWorkStatus = {WorkStatus}\n]";
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