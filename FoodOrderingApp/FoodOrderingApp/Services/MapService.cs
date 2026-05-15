using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodOrderingApp.Services;

public class MapService : IMapService
{
    private readonly Random _random = new();
    private readonly Dictionary<int, (DeliveryPartner Partner, Location CurrentLocation)> _activeDeliveries = new();

    public async Task<DeliveryPartner> GetDeliveryPartnerAsync(int orderId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(200, 500));

        var partnerNames = new[] { "Raj Kumar", "Priya Singh", "Amit Patel", "Kavya Sharma", "Rohan Verma" };
        var vehicleTypes = new[] { "Bike", "Scooter", "Bike" };

        var partnerId = _random.Next(1001, 1100);
        var partner = new DeliveryPartner
        {
            PartnerId = partnerId,
            Name = partnerNames[_random.Next(partnerNames.Length)],
            PhoneNumber = GeneratePhoneNumber(),
            VehicleType = vehicleTypes[_random.Next(vehicleTypes.Length)],
            VehicleNumber = GenerateVehicleNumber(),
            Rating = Math.Round(3.5 + (_random.NextDouble() * 1.5), 1), // 3.5-5.0
            TotalDeliveries = _random.Next(500, 5000)
        };

        // Cache for location tracking
        _activeDeliveries[orderId] = (partner, GetMockRestaurantLocation());

        return partner;
    }

    public async Task<DeliveryRoute> GetDeliveryRouteAsync(int orderId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(200, 500));

        var restaurantLocation = GetMockRestaurantLocation();
        var customerLocation = GetMockCustomerLocation();
        var distance = restaurantLocation.GetDistanceTo(customerLocation);

        // Interpolate current location (simulating progress)
        var currentLocation = InterpolateLocation(restaurantLocation, customerLocation, _random.NextDouble() * 0.3);

        return new DeliveryRoute
        {
            OrderId = orderId,
            RestaurantLocation = restaurantLocation,
            CustomerLocation = customerLocation,
            CurrentPartnerLocation = currentLocation,
            DistanceInKm = Math.Round(distance, 2),
            RoutePath = $"ROUTE_{orderId}_PATH"
        };
    }

    public async Task<LocationUpdate> GetLocationUpdateAsync(int orderId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(100, 300));

        var restaurantLocation = GetMockRestaurantLocation();
        var customerLocation = GetMockCustomerLocation();

        // Simulate progressive delivery (increase progress over time)
        var progress = (DateTime.UtcNow.Second % 60) / 100.0; // 0-1 based on seconds
        var currentLocation = InterpolateLocation(restaurantLocation, customerLocation, Math.Min(progress, 0.95));

        return new LocationUpdate
        {
            Latitude = currentLocation.Latitude,
            Longitude = currentLocation.Longitude,
            Timestamp = DateTime.UtcNow,
            Status = progress < 0.5 ? "Heading to your location" : "Almost there!"
        };
    }

    public async Task<TimeSpan> GetEstimatedDeliveryTimeAsync(int orderId)
    {
        // Simulate network delay
        await Task.Delay(_random.Next(100, 300));

        // Mock: estimated 10-25 minutes remaining
        var minutesRemaining = _random.Next(10, 26);
        return TimeSpan.FromMinutes(minutesRemaining);
    }

    private string GeneratePhoneNumber()
    {
        return $"98{_random.Next(10000000, 99999999)}";
    }

    private string GenerateVehicleNumber()
    {
        var stateCode = new[] { "DL", "MH", "KA", "TN", "GJ" };
        var state = stateCode[_random.Next(stateCode.Length)];
        var district = _random.Next(1, 26).ToString("D2");
        var letters = new string(Enumerable.Range(0, 2).Select(_ => (char)('A' + _random.Next(26))).ToArray());
        var number = _random.Next(1000, 9999);
        return $"{state}{district}{letters}{number}";
    }

    private Location GetMockRestaurantLocation()
    {
        // Mock restaurant locations in Mumbai area
        var restaurants = new[]
        {
            new Location { Latitude = 19.0760, Longitude = 72.8777, Address = "Burger Loft, Bandra" },
            new Location { Latitude = 19.0896, Longitude = 72.8656, Address = "Pizzeria, Andheri" },
            new Location { Latitude = 19.0176, Longitude = 72.8479, Address = "Sakura Sushi, Fort" },
            new Location { Latitude = 19.1136, Longitude = 72.8697, Address = "Spice House, Borivali" }
        };

        return restaurants[_random.Next(restaurants.Length)];
    }

    private Location GetMockCustomerLocation()
    {
        // Mock customer locations in Mumbai area
        var customers = new[]
        {
            new Location { Latitude = 19.0825, Longitude = 72.8830, Address = "Bandra East" },
            new Location { Latitude = 19.1136, Longitude = 72.8697, Address = "Borivali West" },
            new Location { Latitude = 19.0176, Longitude = 72.8479, Address = "Fort, Downtown" },
            new Location { Latitude = 19.1000, Longitude = 72.8500, Address = "Mahim" }
        };

        return customers[_random.Next(customers.Length)];
    }

    private Location InterpolateLocation(Location from, Location to, double progress)
    {
        // Clamp progress to 0-1
        progress = Math.Max(0, Math.Min(1, progress));

        return new Location
        {
            Latitude = from.Latitude + (to.Latitude - from.Latitude) * progress,
            Longitude = from.Longitude + (to.Longitude - from.Longitude) * progress,
            Address = $"En route from {from.Address} to {to.Address}"
        };
    }
}
