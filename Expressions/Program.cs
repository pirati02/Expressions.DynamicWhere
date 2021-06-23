using System;
using System.Collections.Generic;
using System.Linq;
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
                    Address = "user4 address 2",
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
                }
            };

            var filtered1 = list.Filter(12, new[]
            {
                "Name",
                "Position",
                "Age",
            }).ToList();

            foreach (var user in filtered1)
            {
                Console.WriteLine(user.ToString());
            }
            //
            // var filtered2 =
            //     list.Filter("address 2", x => x.Name, x => x.Position, x => x.Address, x => x.WorkStatus).ToList();
            //
            // foreach (var user in filtered2)
            // {
            //     Console.WriteLine(user.ToString());
            // }
        }
    }

    public class User
    {
        public string Name { get; init; }
        public string Position { get; init; }
        public string Address { get; init; }
        public string WorkStatus { get; init; }
        public int Age { get; init; }
        public Location Location { get; set; }

        public override string ToString()
        {
            return
                $"[\n\tName = {Name},\n\tPosition = {Position},\n\tAge = {Age},\n\tAddress = {Address},\n\tWorkStatus = {WorkStatus}\n]";
        }
    }

    public class Location
    {
        public int X { get; init; }
        public int Y { get; init; }

        public override bool Equals(object obj)
        {
            if (obj is Location location)
            {
                return location.X == X && location.Y == Y;
            }
            return base.Equals(obj);
        }
    }
}