# Flight Data Processing System

A sophisticated C# console application demonstrating advanced software engineering principles through flight data management, featuring real-time analytics, comprehensive validation, and intelligent data generation.

## Technical Highlights

### Advanced C# Language Features
- **Computed Properties**: Dynamic calculations for delays and flight duration using expression-bodied members
- **LINQ Integration**: Complex data querying and aggregation throughout the system
- **Nullable Reference Types**: Defensive programming with null-conditional operators (`?.`)
- **String Interpolation**: Advanced formatting with culture-specific date/time representations
- **Enum-based State Management**: Type-safe flight status tracking

### Architectural Excellence

#### Design Patterns Implemented
- **Dependency Injection**: Constructor injection for loose coupling between components
- **Interface Segregation Principle**: Focused interfaces (`IFlightProcessor`, `IFlightDataValidator`)
- **Single Responsibility Principle**: Each class has one clear purpose
- **Repository Pattern**: Centralized data access through `FlightProcessor`
- **Strategy Pattern**: Pluggable validation through `IFlightDataValidator`

#### Object-Oriented Design
```csharp
// Polymorphic behavior through interfaces
public FlightProcessor(IFlightDataValidator validator)
{
    _validator = validator; // Dependency injection
}

// Computed properties with business logic
public bool IsDelayed => DepartureDelay.TotalMinutes > 15 || ArrivalDelay.TotalMinutes > 15;
```

### Intelligent Data Generation

The `FlightDataGenerator` class showcases sophisticated randomization:

```csharp
// Realistic delay simulation with flight time recovery
var departureDelay = TimeSpan.FromMinutes(_random.Next(-5, 180));
var arrivalDelay = departureDelay + TimeSpan.FromMinutes(_random.Next(-30, 60));
```

**Why This Matters**: Airlines can recover time during flight, so arrival delays aren't always equal to departure delays.

### Advanced LINQ Usage

The system demonstrates complex query operations:

```csharp
// Multi-level grouping and statistical analysis
var airlineStats = _flights.GroupBy(f => f.Airline)
                          .OrderByDescending(g => g.Count())
                          .Take(5);

// Conditional aggregation
var averageDelay = _flights.Where(f => f.IsDelayed)
                          .Average(f => Math.Max(f.DepartureDelay.TotalMinutes, 
                                               f.ArrivalDelay.TotalMinutes));
```

### Robust Error Handling

Multi-layered validation approach:
1. **Compile-time**: Strong typing prevents many errors
2. **Runtime**: Try-catch blocks for user input parsing
3. **Business Logic**: Custom validation rules in `FlightDataValidator`
4. **Data Integrity**: Logical constraint checking

### Performance Considerations

- **Lazy Evaluation**: LINQ queries execute only when enumerated
- **Efficient Filtering**: Uses `Where()` before expensive operations
- **Memory Management**: Returns new collections to prevent external mutation
- **String Operations**: Case-insensitive searches using `ToLower()`

## Features

### Core Functionality
- **Flight Management**: Add, validate, and store flight information
- **Advanced Search**: Multi-criteria filtering with partial matching
- **Real-time Statistics**: Dynamic analytics with percentage calculations
- **Data Validation**: Comprehensive business rule enforcement
- **Sample Data Generation**: Realistic test data with proper distributions

### User Experience Features
- **Interactive Console UI**: Clean, formatted menus with box-drawing characters
- **Graceful Error Recovery**: User-friendly error messages with guidance
- **Flexible Input**: Accepts various exit commands ("9", "q", "quit", "exit")
- **Formatted Output**: Aligned columns and professional data presentation

## Requirements

- **.NET 8.0** or later
- **Console/Terminal** environment
- **System.Linq** for advanced querying

## Installation & Setup

### Quick Start
```bash
# Create project
mkdir FlightDataSystem && cd FlightDataSystem
dotnet new console

# Replace Program.cs with the flight system code
# Run the application
dotnet run
```

### Visual Studio Setup
1. Create new **Console App (.NET)**
2. Replace default `Program.cs` with provided code
3. Build and run (F5)

## Usage Examples

### Statistical Analysis
```
Total Flights: 50
Delayed Flights: 18 (36.0%)
Average Delay (delayed flights): 67.3 minutes
Total Passengers: 8,750

Top Airlines by Flight Count:
  Delta Air Lines: 12 flights
  American Airlines: 9 flights
```

### Smart Search
```csharp
// Partial airline matching (case-insensitive)
GetFlightsByAirline("delta") // Matches "Delta Air Lines"

// Flexible destination search
GetFlightsByDestination("JFK") // Finds all JFK flights
```

## Code Architecture Deep Dive

### Data Model Design
```csharp
public class Flight
{
    // Core properties
    public string FlightNumber { get; set; }
    public DateTime ScheduledDeparture { get; set; }
    
    // Computed properties (no storage overhead)
    public TimeSpan DepartureDelay => ActualDeparture - ScheduledDeparture;
    public bool IsDelayed => DepartureDelay.TotalMinutes > 15;
}
```

**Design Decision**: Computed properties ensure data consistency and reduce storage requirements.

### Validation Strategy
```csharp
public string GetValidationErrors(Flight flight)
{
    var errors = new List<string>();
    
    // Business rule validation
    if (flight.Origin?.ToUpper() == flight.Destination?.ToUpper())
        errors.Add("Origin and destination cannot be the same");
        
    return string.Join("; ", errors);
}
```

**Benefits**: Centralized validation logic, detailed error reporting, easy extensibility.

### Statistical Calculations
```csharp
// Percentage calculations with proper formatting
var delayPercentage = _flights.Count(f => f.IsDelayed) * 100.0 / _flights.Count;
Console.WriteLine($"Delayed: {delayPercentage:F1}%");
```

**Technical Note**: Uses `100.0` to force floating-point division, avoiding integer truncation.

## Interesting Implementation Details

### Date/Time Handling
- **Culture-Invariant Parsing**: `DateTime.ParseExact()` with `CultureInfo.InvariantCulture`
- **Time Zone Awareness**: All times treated as local for simplicity
- **Duration Calculations**: Proper `TimeSpan` arithmetic for delays

### String Processing
- **Unicode Support**: Box-drawing characters for professional UI
- **Format Specifiers**: Custom number formatting (`{value:F1}`, `{value:N0}`)
- **Alignment**: Fixed-width columns using negative field widths (`{value,-8}`)

### Random Data Generation
The sample data generator creates realistic scenarios:
- **Temporal Distribution**: Flights spread across 72-hour window
- **Realistic Constraints**: Origin ≠ destination enforcement
- **Probability Modeling**: 5% cancellation rate matches industry averages

### Memory Efficiency
- **Defensive Copying**: `GetAllFlights()` returns `ToList()` to prevent external modification
- **Lazy Evaluation**: LINQ queries don't execute until results are needed
- **Immutable Strings**: Efficient string concatenation in `ToString()` override

## Extension Points

The architecture supports easy enhancement:

### Adding New Airlines
```csharp
private readonly string[] _airlines = { "Your Airline", /* existing airlines */ };
```

### Custom Validation Rules
```csharp
// Add to FlightDataValidator.GetValidationErrors()
if (flight.Aircraft?.Contains("747") == true && flight.PassengerCount > 400)
    errors.Add("747 capacity exceeded");
```

### New Search Criteria
```csharp
public List<Flight> GetFlightsByAircraft(string aircraft)
{
    return _flights.Where(f => f.Aircraft.Contains(aircraft, StringComparison.OrdinalIgnoreCase))
                   .ToList();
}
```

## Performance Characteristics

- **Time Complexity**: O(n) for most operations, O(n log n) for sorted output
- **Space Complexity**: O(n) storage with minimal overhead
- **Scalability**: Suitable for datasets up to ~10,000 flights in memory

## Testing Approach

The system includes built-in testing through:
- **Sample Data Generation**: Automated test case creation
- **Validation Testing**: Error condition verification
- **Statistical Verification**: Data integrity through calculated metrics

## Educational Value

This codebase demonstrates:
- **Professional C# Practices**: Modern language features and conventions
- **Software Architecture**: Clean separation of concerns
- **Data Processing**: Real-world business logic implementation
- **User Experience**: Console application best practices

Perfect for learning advanced C# concepts, LINQ operations, and object-oriented design principles in a practical context.

---

**Built with C# 12 and .NET 8 - Showcasing modern development practices**
