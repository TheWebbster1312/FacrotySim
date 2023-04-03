// See https://aka.ms/new-console-template for more information

using System.Net.Security;
using System.Runtime.InteropServices;
using System.Xml;

Console.WriteLine("Hello, World!");

Item Iron = new Item("Iron", ItemType.Raw);
Item Copper = new Item("Copper", ItemType.Raw);

Container Fe1 = new Container();
Container Fe2 = new Container();
Container Cu1 = new Container();

Fe1.AddItem(Iron, 20);
Fe2.AddItem(Iron, 40);
Cu1.AddItem(Copper, 20);


Network network = new Network();

network.Regester(Fe1);
network.Regester(Fe2);
network.Regester(Cu1);

network.printInventory();
network.ScanNetwork();
network.printInventory();

// testing item request

Fe1.RequestItem(Iron, 40);

//Fe1.AddItem(Iron, 25);

Fe1.ToAString();
Fe2.ToAString();




public class Recipie
{
    // the tabel is a 3x3 grid with item 0 being in the top left and 8 being in the bottom right.
    private Inventory Input = new Inventory(9);
    public Item OutputItem;
    public int OutputQuantity = 1;
    private static int currentId = 0;
    public int id;

    private static Dictionary<int, Recipie> Instances = new Dictionary<int, Recipie>();

    public Recipie(Item[] items, Item product)
    {
        foreach(Item item in items)
        {
            Input.AddItem(item, 1);
        }
        OutputItem = product;
        id = currentId++;

        Instances.Add(id, this);
    }

    public Recipie(Item[] items, int[] itemCounts, Item product, int productCount)
    {
        int i = 0;
        foreach(Item item in items)
        {
            Input.AddItem(item, itemCounts[i]);
            i++;
        }
        OutputItem = product;
        OutputQuantity = productCount;
        id = currentId++;

        Instances.Add(id, this);
    }

    public static Recipie GetInstance(int id)
    {
        return Instances[id];
    }
}

public enum ItemType
{
    Raw,
    Craftable
}

public enum BoxType
{
    Machine,
    Container
}

public class Item
{
    // Feilds
    public string Name { get; set; }
    public int Id { get; set; }
    public ItemType ItemType { get; set; }

    private Recipie Recipie {
        get
        {
            if(this.ItemType == ItemType.Raw)
            {
                Console.WriteLine("Cannot get recipie of raw item");
                return null;
            }
            else
            {
                return this.Recipie;
            }
        }
        set
        {
            this.Recipie = value;
        }
    }

    private static int currentId = 0;

    // constructor

    public Item(string name, ItemType type)
    {
        // this is for creation of an item with no recipie, ie raw
        Name = name;
        Id = currentId++;
        ItemType = type;
    }

    public Item(string name, ItemType itemType, Recipie recipie)
    {
        // this is for craftable items that take a recipie as an input
        Name = name;
        Id = currentId++;
        ItemType = itemType;
        Recipie = recipie;
    }   
}

public class Container
{
    private static int currentId = 0;

    private static Dictionary<int, Container> Instances = new Dictionary<int, Container>();
    
    public string Name { get; set; }
    public int id { get; set; }

    public int Quantity = 0;

    public Inventory Inventory = new Inventory(1);

    public Item Item
    {
        get
        {
            if(Inventory.GetItems().Count > 0)
            {
                return Inventory.GetItems()[0];
            }
            return null;
            
        }
    }

    public static Container GetInstance(int id)
    {
        return Instances[id];
    }

    public Network Network { get; set; }


    // constructor
    public Container(Item item)
    {
        id = currentId++;
        Inventory.AddItem(item, 0);
        Instances.Add(id, this);
    }
    public Container()
    {
        id = currentId++;
        Instances.Add(id, this);
    }

    public string ToAString()
    {
        string returnString;
        if (Item == null)
        {
            returnString = ("Name: " + Name + ", ID: " + id + ", Item: Null , Quantity: N\\A");
        }
        else
        {
            returnString = ("Name: " + Name + ", ID: " + id + ", Item: " + Item.Name + ", Quantity: " + Quantity);
        }
        
        Console.WriteLine(returnString);
        return returnString;
    }

    public int AddItem(Item item, int amount)
    {
        if(Inventory.AddItem(item, amount) == 0)
        {
            Quantity = Quantity + amount;
            return 0;
        }
        else
        {
            return -1;
        }
    }

    public int RemoveItem(Item item, int amount)
    {
        int successCase = Inventory.RemoveItem(item, amount);
        if (successCase == 0)
        {
            Quantity = Quantity - amount;
            return 0;
        }
        else if(successCase == 1)
        {
            Quantity = 0;
            return 0;
        }
        else
        {
            return -1;
        }
    }
    
    public int RequestItem(Item item, int quantity)
    {
        if (Network == null)
        {
            return -1;
        }
        
        if (Network.ItemRequest(item, quantity, this))
        {
            return 0;
        }

        return -1;
    }
}
public class Machine
{
    private Inventory InputInventory = new Inventory(9);
    private Inventory OutputInventory = new Inventory(1);

    private static int currentId = 0;
    public int id;

    private static Dictionary<int, Machine> Instances = new Dictionary<int, Machine>();

    public static Machine GetInstance(int id)
    {
        return Instances[id];
    }

    public Recipie CurrentRecipie;

    Item OutputItem
    {
        get
        {
            if (OutputInventory.Count > 0)
            {
                return OutputInventory.GetItems()[0];
            }
            else
            {
                return null;
            }
        }
    }

    int OutputQuantity
    {
        get
        {
            if (OutputInventory.Count > 0)
            {
                return OutputInventory.GetCounts()[0];
            }
            else
            {
                return 0;
            }
        }
    }

    public Machine()
    {
        id = currentId++;
        Instances.Add(id, this);
    }
}

public class Inventory
{
    private List<Item> Items = new List<Item>();
    private List<int> Counts = new List<int>();
    public int MaxItems;
    public int Count = 0;

    public Inventory()
    {
        MaxItems = -1;
    }

    public Inventory(int maxItems)
    {
        MaxItems = maxItems;
    }

    public int AddItem(Item item, int quantity)
    {
        // is item in here already
        if (!Items.Contains(item))
        {
            if(Items.Count >= MaxItems && MaxItems != -1)
            {
                // already full on the amount of item types it can contain
                return -1;
            }
            Items.Add(item);
            Counts.Add(quantity);
            Count++;
            return 0;
        }
        else
        {
            // get current count
            int CurrentQuantity = GetQuantity(item);
            int index  = Items.IndexOf(item);
            quantity = quantity + CurrentQuantity;
            Counts[index] = quantity;
            return 0;
        }
    }

    public int GetQuantity(Item item)
    {
        if (Items.Contains(item))
        {
            return Counts[Items.IndexOf(item)];
        }
        else
        {
            return 0;
        }
    }

    public int RemoveItem(Item item, int quantity)
    {
        //return numbers 
        // 1: removed all
        // 0 success
        // -1: too many asked to be removed

        // check if item is here
        if (Items.Contains(item))
        {
            int index = Items.IndexOf(item);
            int CurrentQuantity = GetQuantity(item);


            if(CurrentQuantity < quantity)
            {
                return -1;
            }

            if(CurrentQuantity == quantity)
            {
                Items.RemoveAt(index);
                Counts.RemoveAt(index);
                Count--;
                return 1;
            }

            Counts[index] = CurrentQuantity - quantity;

            return 0;
            
        }
        else
        {
            return -1;
        }
    }

    public List<Item> GetItems()
    {
        return Items;
    }

    public List<int> GetCounts()
    {
        return Counts;
    }

}



public class NetworkNode
{
    public Type Type;
    private static int currentId = 0;

    public Container container;
    public Machine machine;

    public int NodeId { get; set; }

    public NetworkNode(Container _container)
    {
        Type = typeof(Container);
        NodeId = currentId++;
        container = _container;
    }

    public NetworkNode(Machine _machine)
    {
        Type = typeof(Machine);
        NodeId = currentId++;
        machine = _machine;

    }
}


public class Network
{
    private static int currentId = 0;
    private static int NetworkCapacity = 5;
    public int NodeCount = 0;
    private Inventory Inventory = new Inventory();

    public int id { get; set; }

    public List<NetworkNode> Nodes = new List<NetworkNode>();

    public Network()
    {
        id = currentId++;
    }

    public Container getContainer(Item item, Container excluded)
    {
        foreach(NetworkNode x in Nodes)
        {
            if (x.Equals(excluded) || x.Type != typeof(Container))
            {
                continue;
            }
            Container container = x.container;
            if(container.Item == item)
            {
                return container;
            }
        }
        return null;
    }
    public Container getContainer(Item item)
    {
        foreach (NetworkNode x in Nodes)
        {
            if(x.Type != typeof(Container))
            {
                continue;
            }
            Container container = x.container;
            if (container.Item == item)
            {
                return container;
            }
        }
        return null;
    }

    public bool Regester(Container container)
    {
        if (Nodes.Count + 1 >= NetworkCapacity)
        {
            return false;
        }

        container.Network = this;

        Nodes.Add(new NetworkNode(container));


        return true;
    }
    public bool ScanNetwork()
    {
        Inventory = new Inventory();
        foreach(NetworkNode x in Nodes)
        {
            if(x.Type == typeof(Container))
            {
                Container container = x.container;
                Inventory.AddItem(container.Item, container.Quantity);
            }
            
        }
        return true;
    }

    public void printInventory()
    {
        foreach(Item x in Inventory.GetItems())
        {
            Console.Write(x.Name + ", ");
        }
        Console.WriteLine();
        foreach (int x in Inventory.GetCounts())
        {
            Console.Write(x + ", ");
        }
        Console.WriteLine();
    }

    public bool ItemRequest(Item item, int quantity, Container to)
    {
        ScanNetwork();
        if(Inventory.GetQuantity(item) > quantity)
        {
            // do the item request
            bool satisfied = true;

            while (satisfied)
            {
                Container from = getContainer(item, to);
                int fromQuantity = from.Quantity;
                from.RemoveItem(item, quantity); //  remove from container
                if (fromQuantity >= quantity)
                {
                    satisfied = false;
                }
                else
                {
                    // continues to grab from other containers. not sure what to put here
                }
            }
            to.AddItem(item, quantity);

            return true;
        }
        else
        {
            return false;
        }
    }
}