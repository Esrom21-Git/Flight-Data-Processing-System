using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// =============================================================================
// INTERFACES
// =============================================================================

public interface IFlightProcessor
{
    void AddFlight(Flight flight);
    List<Flight> GetFlightsByAirline(string airline);
    List<Flight> GetFlightsByDestination(string destination);
    List<Flight> GetDelayedFlights();
    List<Flight> GetFlightsOnDate(DateTime date);
    void DisplayFlightStatistics();
    List<Flight> GetAllFlights();
}

public interface IFlightDataValidator
{
    bool ValidateFlight(Flight flight);
    string GetValidationErrors(Flight flight);
}

// =============================================================================
// FLIGHT MODEL
// =============================================================================

public class Flight
{
    public string FlightNumber { get; set; }
    public string Airline { get; set; }
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ActualDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime ActualArrival { get; set; }
    public FlightStatus Status { get; set; }
    public int PassengerCount { get; set; }
    public string Aircraft { get; set; }

    public TimeSpan DepartureDelay => ActualDeparture - ScheduledDeparture;
    public TimeSpan ArrivalDelay => ActualArrival - ScheduledArrival;
    public bool IsDelayed => DepartureDelay.TotalMinutes > 15 || ArrivalDelay.TotalMinutes > 15;
    public TimeSpan FlightDuration => ActualArrival - ActualDeparture;

    public override string ToString()
    {
        return $"{FlightNumber} | {Airline} | {Origin} → {Destination} | " +
               $"{ScheduledDeparture:MM/dd HH:mm} | {Status} | " +
               $"Delay: {(IsDelayed ? $"{Math.Max(DepartureDelay.TotalMinutes, ArrivalDelay.TotalMinutes):F0}min" : "On-time")}";
    }
}

public enum FlightStatus
{
    Scheduled,
    OnTime,
    Delayed,
    Departed,
    Arrived,
    Cancelled,
    Diverted
}

// =============================================================================
// FLIGHT DATA VALIDATOR
// =============================================================================

public class FlightDataValidator : IFlightDataValidator
{
    public bool ValidateFlight(Flight flight)
    {
        return string.IsNullOrEmpty(GetValidationErrors(flight));
    }

    public string GetValidationErrors(Flight flight)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(flight.FlightNumber))
            errors.Add("Flight number is required");

        if (string.IsNullOrWhiteSpace(flight.Airline))
            errors.Add("Airline is required");

        if (string.IsNullOrWhiteSpace(flight.Origin))
            errors.Add("Origin is required");

        if (string.IsNullOrWhiteSpace(flight.Destination))
            errors.Add("Destination is required");

        if (flight.Origin?.ToUpper() == flight.Destination?.ToUpper())
            errors.Add("Origin and destination cannot be the same");

        if (flight.ScheduledDeparture >= flight.ScheduledArrival)
            errors.Add("Scheduled departure must be before scheduled arrival");

        if (flight.PassengerCount < 0)
            errors.Add("Passenger count cannot be negative");

        if (flight.PassengerCount > 800)
            errors.Add("Passenger count seems unrealistic (>800)");

        return string.Join("; ", errors);
    }
}

// =============================================================================
// FLIGHT PROCESSOR
// =============================================================================

public class FlightProcessor : IFlightProcessor
{
    private readonly List<Flight> _flights;
    private readonly IFlightDataValidator _validator;

    public FlightProcessor(IFlightDataValidator validator)
    {
        _flights = new List<Flight>();
        _validator = validator;
    }

    public void AddFlight(Flight flight)
    {
        if (_validator.ValidateFlight(flight))
        {
            _flights.Add(flight);
            Console.WriteLine($"✓ Added: {flight.FlightNumber} - {flight.Airline}");
        }
        else
        {
            Console.WriteLine($"✗ Invalid flight data: {_validator.GetValidationErrors(flight)}");
        }
    }

    public List<Flight> GetFlightsByAirline(string airline)
    {
        return _flights.Where(f => f.Airline.ToLower().Contains(airline.ToLower())).ToList();
    }

    public List<Flight> GetFlightsByDestination(string destination)
    {
        return _flights.Where(f => f.Destination.ToLower().Contains(destination.ToLower())).ToList();
    }

    public List<Flight> GetDelayedFlights()
    {
        return _flights.Where(f => f.IsDelayed).ToList();
    }

    public List<Flight> GetFlightsOnDate(DateTime date)
    {
        return _flights.Where(f => f.ScheduledDeparture.Date == date.Date).ToList();
    }

    public void DisplayFlightStatistics()
    {
        if (!_flights.Any())
        {
            Console.WriteLine("No flight data available for statistics.");
            return;
        }

        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("FLIGHT STATISTICS");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine($"Total Flights: {_flights.Count}");
        Console.WriteLine($"Delayed Flights: {_flights.Count(f => f.IsDelayed)} ({(_flights.Count(f => f.IsDelayed) * 100.0 / _flights.Count):F1}%)");
        Console.WriteLine($"On-time Flights: {_flights.Count(f => !f.IsDelayed)} ({(_flights.Count(f => !f.IsDelayed) * 100.0 / _flights.Count):F1}%)");

        var averageDelay = _flights.Where(f => f.IsDelayed).Average(f => Math.Max(f.DepartureDelay.TotalMinutes, f.ArrivalDelay.TotalMinutes));
        Console.WriteLine($"Average Delay (delayed flights): {averageDelay:F1} minutes");

        Console.WriteLine($"Total Passengers: {_flights.Sum(f => f.PassengerCount):N0}");
        Console.WriteLine($"Average Passengers per Flight: {_flights.Average(f => f.PassengerCount):F0}");

        Console.WriteLine("\nTop Airlines by Flight Count:");
        var airlineStats = _flights.GroupBy(f => f.Airline)
                                  .OrderByDescending(g => g.Count())
                                  .Take(5);
        foreach (var group in airlineStats)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()} flights");
        }

        Console.WriteLine("\nTop Destinations:");
        var destinationStats = _flights.GroupBy(f => f.Destination)
                                      .OrderByDescending(g => g.Count())
                                      .Take(5);
        foreach (var group in destinationStats)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()} flights");
        }

        Console.WriteLine("\nFlight Status Distribution:");
        var statusStats = _flights.GroupBy(f => f.Status)
                                 .OrderByDescending(g => g.Count());
        foreach (var group in statusStats)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()} flights ({group.Count() * 100.0 / _flights.Count:F1}%)");
        }
    }

    public List<Flight> GetAllFlights()
    {
        return _flights.ToList();
    }
}

// =============================================================================
// SAMPLE DATA GENERATOR
// =============================================================================

public class FlightDataGenerator
{
    private readonly Random _random = new Random();
    private readonly string[] _airlines = { "American Airlines", "Delta Air Lines", "United Airlines", "Southwest Airlines", "JetBlue Airways", "Alaska Airlines", "Spirit Airlines" };
    private readonly string[] _airports = { "JFK", "LAX", "ORD", "DFW", "DEN", "ATL", "PHX", "SEA", "MIA", "BOS", "LAS", "SFO", "MSP", "CLT", "EWR" };
    private readonly string[] _aircraft = { "Boeing 737", "Airbus A320", "Boeing 777", "Airbus A330", "Boeing 787", "Embraer E175", "CRJ-900" };

    public List<Flight> GenerateSampleFlights(int count)
    {
        var flights = new List<Flight>();
        var baseDate = DateTime.Today.AddDays(-2);

        for (int i = 0; i < count; i++)
        {
            var scheduledDeparture = baseDate.AddHours(_random.Next(0, 72)).AddMinutes(_random.Next(0, 60));
            var flightDuration = TimeSpan.FromHours(_random.Next(1, 8)) + TimeSpan.FromMinutes(_random.Next(0, 60));
            var scheduledArrival = scheduledDeparture + flightDuration;

            var departureDelay = TimeSpan.FromMinutes(_random.Next(-5, 180)); // -5 to 180 minutes
            var arrivalDelay = departureDelay + TimeSpan.FromMinutes(_random.Next(-30, 60)); // Can recover some time in flight

            var actualDeparture = scheduledDeparture + departureDelay;
            var actualArrival = scheduledArrival + arrivalDelay;

            var origin = _airports[_random.Next(_airports.Length)];
            var destination = _airports.Where(a => a != origin).OrderBy(x => _random.Next()).First();

            var flight = new Flight
            {
                FlightNumber = $"{GetRandomAirlineCode()}{_random.Next(1000, 9999)}",
                Airline = _airlines[_random.Next(_airlines.Length)],
                Origin = origin,
                Destination = destination,
                ScheduledDeparture = scheduledDeparture,
                ActualDeparture = actualDeparture,
                ScheduledArrival = scheduledArrival,
                ActualArrival = actualArrival,
                Status = GetFlightStatus(scheduledDeparture, actualDeparture, DateTime.Now),
                PassengerCount = _random.Next(50, 300),
                Aircraft = _aircraft[_random.Next(_aircraft.Length)]
            };

            flights.Add(flight);
        }

        return flights;
    }

    private string GetRandomAirlineCode()
    {
        var codes = new[] { "AA", "DL", "UA", "WN", "B6", "AS", "NK" };
        return codes[_random.Next(codes.Length)];
    }

    private FlightStatus GetFlightStatus(DateTime scheduled, DateTime actual, DateTime now)
    {
        if (_random.Next(100) < 5) return FlightStatus.Cancelled; // 5% chance of cancellation

        if (actual < now.AddHours(-2))
            return FlightStatus.Arrived;
        else if (actual < now)
            return FlightStatus.Departed;
        else if (actual - scheduled > TimeSpan.FromMinutes(15))
            return FlightStatus.Delayed;
        else
            return FlightStatus.OnTime;
    }
}

// =============================================================================
// CONSOLE APPLICATION
// =============================================================================

public class FlightDataConsoleApp
{
    private readonly IFlightProcessor _flightProcessor;
    private readonly FlightDataGenerator _dataGenerator;

    public FlightDataConsoleApp()
    {
        var validator = new FlightDataValidator();
        _flightProcessor = new FlightProcessor(validator);
        _dataGenerator = new FlightDataGenerator();
    }

    public void Run()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 FLIGHT DATA PROCESSING SYSTEM                ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        while (true)
        {
            DisplayMenu();
            var choice = Console.ReadLine();

            switch (choice?.ToLower())
            {
                case "1":
                    AddFlightManually();
                    break;
                case "2":
                    LoadSampleData();
                    break;
                case "3":
                    SearchFlightsByAirline();
                    break;
                case "4":
                    SearchFlightsByDestination();
                    break;
                case "5":
                    ShowDelayedFlights();
                    break;
                case "6":
                    ShowFlightsOnDate();
                    break;
                case "7":
                    DisplayAllFlights();
                    break;
                case "8":
                    _flightProcessor.DisplayFlightStatistics();
                    break;
                case "9":
                case "q":
                case "quit":
                case "exit":
                    Console.WriteLine("Thank you for using Flight Data Processing System!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void DisplayMenu()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                        MAIN MENU                           │");
        Console.WriteLine("├─────────────────────────────────────────────────────────────┤");
        Console.WriteLine("│ 1. Add Flight Manually                                     │");
        Console.WriteLine("│ 2. Load Sample Data                                        │");
        Console.WriteLine("│ 3. Search Flights by Airline                              │");
        Console.WriteLine("│ 4. Search Flights by Destination                          │");
        Console.WriteLine("│ 5. Show Delayed Flights                                   │");
        Console.WriteLine("│ 6. Show Flights on Specific Date                          │");
        Console.WriteLine("│ 7. Display All Flights                                    │");
        Console.WriteLine("│ 8. Show Flight Statistics                                 │");
        Console.WriteLine("│ 9. Exit                                                   │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘");
        Console.Write("Enter your choice: ");
    }

    private void AddFlightManually()
    {
        Console.WriteLine("\n=== ADD FLIGHT MANUALLY ===");

        try
        {
            Console.Write("Flight Number: ");
            var flightNumber = Console.ReadLine();

            Console.Write("Airline: ");
            var airline = Console.ReadLine();

            Console.Write("Origin Airport (3-letter code): ");
            var origin = Console.ReadLine()?.ToUpper();

            Console.Write("Destination Airport (3-letter code): ");
            var destination = Console.ReadLine()?.ToUpper();

            Console.Write("Scheduled Departure (MM/dd/yyyy HH:mm): ");
            var scheduledDeparture = DateTime.ParseExact(Console.ReadLine(), "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);

            Console.Write("Actual Departure (MM/dd/yyyy HH:mm): ");
            var actualDeparture = DateTime.ParseExact(Console.ReadLine(), "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);

            Console.Write("Scheduled Arrival (MM/dd/yyyy HH:mm): ");
            var scheduledArrival = DateTime.ParseExact(Console.ReadLine(), "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);

            Console.Write("Actual Arrival (MM/dd/yyyy HH:mm): ");
            var actualArrival = DateTime.ParseExact(Console.ReadLine(), "MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture);

            Console.Write("Passenger Count: ");
            var passengerCount = int.Parse(Console.ReadLine());

            Console.Write("Aircraft: ");
            var aircraft = Console.ReadLine();

            var flight = new Flight
            {
                FlightNumber = flightNumber,
                Airline = airline,
                Origin = origin,
                Destination = destination,
                ScheduledDeparture = scheduledDeparture,
                ActualDeparture = actualDeparture,
                ScheduledArrival = scheduledArrival,
                ActualArrival = actualArrival,
                Status = FlightStatus.Arrived,
                PassengerCount = passengerCount,
                Aircraft = aircraft
            };

            _flightProcessor.AddFlight(flight);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding flight: {ex.Message}");
        }
    }

    private void LoadSampleData()
    {
        Console.WriteLine("\n=== LOADING SAMPLE DATA ===");
        Console.Write("How many sample flights to generate? (default: 50): ");
        var input = Console.ReadLine();
        int count = string.IsNullOrWhiteSpace(input) ? 50 : int.Parse(input);

        var sampleFlights = _dataGenerator.GenerateSampleFlights(count);

        Console.WriteLine($"Generated {sampleFlights.Count} sample flights...");

        foreach (var flight in sampleFlights)
        {
            _flightProcessor.AddFlight(flight);
        }

        Console.WriteLine($"\n✓ Successfully loaded {sampleFlights.Count} sample flights!");
    }

    private void SearchFlightsByAirline()
    {
        Console.WriteLine("\n=== SEARCH BY AIRLINE ===");
        Console.Write("Enter airline name (partial match): ");
        var airline = Console.ReadLine();

        var flights = _flightProcessor.GetFlightsByAirline(airline);
        DisplayFlights(flights, $"Flights for '{airline}'");
    }

    private void SearchFlightsByDestination()
    {
        Console.WriteLine("\n=== SEARCH BY DESTINATION ===");
        Console.Write("Enter destination airport code: ");
        var destination = Console.ReadLine();

        var flights = _flightProcessor.GetFlightsByDestination(destination);
        DisplayFlights(flights, $"Flights to '{destination}'");
    }

    private void ShowDelayedFlights()
    {
        Console.WriteLine("\n=== DELAYED FLIGHTS ===");
        var delayedFlights = _flightProcessor.GetDelayedFlights();
        DisplayFlights(delayedFlights, "Delayed Flights (>15 minutes)");
    }

    private void ShowFlightsOnDate()
    {
        Console.WriteLine("\n=== FLIGHTS ON SPECIFIC DATE ===");
        Console.Write("Enter date (MM/dd/yyyy): ");

        try
        {
            var date = DateTime.ParseExact(Console.ReadLine(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            var flights = _flightProcessor.GetFlightsOnDate(date);
            DisplayFlights(flights, $"Flights on {date:MM/dd/yyyy}");
        }
        catch (Exception)
        {
            Console.WriteLine("Invalid date format. Please use MM/dd/yyyy.");
        }
    }

    private void DisplayAllFlights()
    {
        Console.WriteLine("\n=== ALL FLIGHTS ===");
        var allFlights = _flightProcessor.GetAllFlights();
        DisplayFlights(allFlights, "All Flights");
    }

    private void DisplayFlights(List<Flight> flights, string title)
    {
        Console.WriteLine($"\n{title}");
        Console.WriteLine(new string('-', Math.Max(title.Length, 80)));

        if (!flights.Any())
        {
            Console.WriteLine("No flights found.");
            return;
        }

        Console.WriteLine($"Found {flights.Count} flight(s):\n");
        Console.WriteLine("Flight   | Airline          | Route      | Departure     | Status    | Delay");
        Console.WriteLine(new string('-', 80));

        foreach (var flight in flights.OrderBy(f => f.ScheduledDeparture))
        {
            var delayText = flight.IsDelayed
                ? $"{Math.Max(flight.DepartureDelay.TotalMinutes, flight.ArrivalDelay.TotalMinutes):F0}min"
                : "On-time";

            Console.WriteLine($"{flight.FlightNumber,-8} | {flight.Airline,-16} | {flight.Origin}->{flight.Destination,-6} | " +
                            $"{flight.ScheduledDeparture:MM/dd HH:mm} | {flight.Status,-9} | {delayText}");
        }
    }
}

// =============================================================================
// PROGRAM ENTRY POINT
// =============================================================================

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var app = new FlightDataConsoleApp();
            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}