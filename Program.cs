using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Bogus;

// TODO Data object
class Data
{
    public readonly int id;
    public string author { get; set; }
    public string data { get; set; }

    public Data(int id, string author, string data)
    {
        this.id = id;
        this.author = author;
        this.data = data;
    }

    public int GetId()
    {
        return id;
    }
}

// TODO Data handling

class WebDataService
{
    private List<Data> dataRecords;
    private WebClient client;
    private string serviceUrl;
    private string webContent;

    private string[] contentRows;

    public WebDataService(string serviceUrl)
    {
        this.serviceUrl = serviceUrl;
        client = new WebClient();
        dataRecords = [];
        webContent = string.Empty;
        contentRows = [];
    }

    public string DownloadSync()
    {
        int nextId = dataRecords.Count == 0 ? 1 : dataRecords.Max(item => item.GetId()) + 1;

        try
        {
            webContent = client.DownloadString(serviceUrl);
            if (!string.IsNullOrEmpty(webContent))
            {
                contentRows = webContent.Split(
                    new char[] { '.' },
                    StringSplitOptions.RemoveEmptyEntries
                );
                for (int i = 0; i < (contentRows.Length - 1); i++)
                {
                    dataRecords.Add(
                        new Data(
                            nextId,
                            FakeName(),
                            (Regex.Replace(contentRows[i], @"\s{2,}", " ")).Trim()
                        )
                    );
                    nextId++;
                }
            }
            return "Data was downloaded successfully.";
        }
        catch (WebException ex)
        {
            return "An error occurred: " + ex.Message;
        }
    }

    private string FakeName()
    {
        var faker = new Faker();
        return faker.Name.FullName();
    }

    public void PrintData()
    {
        foreach (var item in dataRecords)
        {
            Console.WriteLine(
                $"ID: {item.GetId()}\nAuthor: {item.author}\nData: {item.data}\n"
            );
        }
    }

    public string RemoveRecord(int id)
    {
        var itemExists = dataRecords.Any(item => item.GetId() == id);

        if (itemExists)
        {
            dataRecords.RemoveAll(item => item.GetId() == id);
            return "The record was removed.";
        }
        else
        {
            return $"No record found with ID: {id}.";
        }
    }

    public string AddRecord(string name, string data)
    {
        int nextId = dataRecords.Count == 0 ? 1 : dataRecords.Max(item => item.GetId()) + 1;

        dataRecords.Add(new Data(nextId, name, data));
        return $"Record with ID {nextId} was created";
    }

    public string UpdateRecord(int id, string newName, string newData)
    {
        var record = dataRecords.FirstOrDefault(item => item.GetId() == id);
        if (record != null)
        {
            record.author = newName;
            record.data = newData;
            return $"Record with ID {id} has been updated.";
        }
        else
        {
            return $"No record found with ID {id}.";
        }
    }
}

// TODO App
class App
{
    WebDataService service;

    public App()
    {
        service = new("http://mvi.mechatronika.cool/sites/default/files/berces.html");
    }

    public void Run()
    {
        bool exit = false;

        while (!exit)
        {
            Console.WriteLine("\nConsole App Menu:");
            Console.WriteLine("1. Download data.");
            Console.WriteLine("2. Display all records.");
            Console.WriteLine("3. Delete a record.");
            Console.WriteLine("4. Edit a record.");
            Console.WriteLine("5. Add a record.");
            Console.WriteLine("6. Exit");
            Console.Write("Select an option: ");

            switch (Console.ReadLine())
            {
                case "1":
                    Console.WriteLine(service.DownloadSync());
                    break;

                case "2":
                    service.PrintData();
                    break;

                case "3":
                    Console.Write("Enter id: ");
                    string? idInput = Console.ReadLine();
                    if (!string.IsNullOrEmpty(idInput))
                    {
                        if (int.TryParse(idInput, out int id))
                        {
                            Console.WriteLine(service.RemoveRecord(id));
                        }
                        else
                        {
                            Console.WriteLine("Invalid ID format. Please enter a valid integer.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The ID must be provided.");
                    }
                    break;

                case "4":
                    Console.Write("Enter the ID of the record to update: ");
                    string? updateIdInput = Console.ReadLine();
                    if (
                        int.TryParse(updateIdInput, out int updateRecordId)
                        && !string.IsNullOrEmpty(updateIdInput)
                    )
                    {
                        Console.Write("Enter new name: ");
                        string? newName = Console.ReadLine();

                        Console.Write("Enter new data: ");
                        string? newData = Console.ReadLine();

                        if (!string.IsNullOrEmpty(newName) && !string.IsNullOrEmpty(newData))
                        {
                            string result = service.UpdateRecord(updateRecordId, newName, newData);
                            Console.WriteLine(result);
                        }
                        else
                        {
                            Console.WriteLine("Name and data cannot be empty.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid ID format. Please enter a valid integer.");
                    }
                    break;

                case "5":
                    Console.Write("Enter Name: ");
                    string? author = Console.ReadLine();
                    Console.Write("Enter data: ");
                    string? data = Console.ReadLine();
                    if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(data))
                    {
                        Console.WriteLine(service.AddRecord(author, data));
                    }
                    else
                    {
                        Console.WriteLine("Both author and data must be provided.");
                    }
                    break;
                case "6":
                    exit = true;
                    break;

                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        App app = new();
        app.Run();
    }
}
