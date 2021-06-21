### Expressions.DynamicWhere

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
