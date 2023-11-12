using System;

namespace Parallel_And_Distributed_Systems_3
{
    internal class Person
    {
        public Person(string nickname)
        {
            Nickname = nickname ?? throw new ArgumentNullException(nameof(nickname));
        }

        public string Nickname { get; }

        public void Subscribe(Room room)
        {
            room.OnPersonJoining += OnPersonJoining;
            room.OnPersonLeaving += OnPersonLeaving;
        }

        public void Unsubscribe(Room room)
        {
            room.OnPersonJoining -= OnPersonJoining;
            room.OnPersonLeaving -= OnPersonLeaving;
        }

        private void OnPersonJoining(object? sender, Person person)
        {
            var room = sender as Room;

            Console.WriteLine($">>>>{Nickname}: New user has joined the room {room?.Name} - {person.Nickname}");
        }

        private void OnPersonLeaving(object? sender, Person person)
        {
            var room = sender as Room;

            Console.WriteLine($">>>>{Nickname}: New user has joined the room {room?.Name} - {person.Nickname}");
        }
    }

    internal class Room
    {
        public EventHandler<Person>? OnPersonCollision;
        public EventHandler<Person>? OnPersonMissing;
        public EventHandler<Person>? OnPersonJoining;
        public EventHandler<Person>? OnPersonLeaving;
        public Room(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }

        public IList<Person> People { get; } = new List<Person>();

        public Room Join(string name)
        {
            if (People.Any(e => e.Nickname == name))
            {
                OnPersonCollision?.Invoke(this, new Person(name));

                return;
            }

            var person = new Person(name);
            People.Add(person);

            OnPersonJoining?.Invoke(this, person);

            return this;
        }

        public void Leave(string name)
        {
            if (People.Any(e => e.Nickname == name))
            {
                OnPersonCollision?.Invoke(this, new Room(name));

                return;
            }

            var person = new Person(name);
            People.Add(person);

            OnPersonJoining?.Invoke(this, person);
        }
    }

    internal class Chat
    {
        private Monitor _monitor;
        public EventHandler<Room>? OnRoomCreated;
        public EventHandler<Room>? OnRoomCollision;
        public IList<Room> Rooms { get; } = new List<Room>();

        public void CreateRoom(string name)
        {
            if (Rooms.Any(e => e.Name == name))
            {
                OnRoomCollision?.Invoke(this, new Room(name));

                return null;
            }

            var room = new Room(name);
            Rooms.Add(room);

            _monitor.Subscribe(room);
            OnRoomCreated?.Invoke(this, room);
        }
    }

    internal class Monitor
    {
        public void Subscribe(Chat chat)
        {
            chat.OnRoomCreated += (sender, arg) =>
            {
                Console.WriteLine($"New chat room created. Chat room name is {arg.Name}");
            };

            chat.OnRoomCollision += (sender, arg) =>
            {
                Console.WriteLine($"Chat room with name {arg.Name} already exists.");
            };
        }

        public void Subscribe(Room room)
        {
            var room = sender as Room;
            room.OnPersonMissing += (sender, arg) =>
            {
                Console.WriteLine($"The user with name {arg.Nickname} can't join room {room.Name} as it already exists.");
            };

            room.OnPersonCollision += (sender, arg) =>
            {
                Console.WriteLine($"Chat room with name {arg.Name} already exists.");
            };
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var monitor = new Monitor();
            var chat = new Chat();
            monitor.Subscribe(chat);

            chat.CreateRoom("Room 1");
            Console.ReadKey();
            chat.CreateRoom("Room 2");
            Console.ReadKey();
            chat.CreateRoom("Room 2");
            Console.ReadKey();
            chat.CreateRoom("Room 4");
            Console.ReadKey();
        }
    }
}