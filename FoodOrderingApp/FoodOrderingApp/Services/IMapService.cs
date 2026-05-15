using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FoodOrderingApp.Models;

namespace FoodOrderingApp.Services;

public interface IMapService
{
    /// <summary>
    /// Get mock delivery partner information for an order
    /// </summary>
    Task<DeliveryPartner> GetDeliveryPartnerAsync(int orderId);

    /// <summary>
    /// Get mock delivery route and current location
    /// </summary>
    Task<DeliveryRoute> GetDeliveryRouteAsync(int orderId);

    /// <summary>
    /// Simulate location updates for delivery partner (polls/updates position)
    /// </summary>
    Task<LocationUpdate> GetLocationUpdateAsync(int orderId);

    /// <summary>
    /// Get estimated delivery time based on current location and distance
    /// </summary>
    Task<TimeSpan> GetEstimatedDeliveryTimeAsync(int orderId);
}

public class DeliveryPartner
{
    public int PartnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = "Bike"; // Bike, Scooter, Car
    public string VehicleNumber { get; set; } = string.Empty;
    public double Rating { get; set; } = 4.5;
    public int TotalDeliveries { get; set; } = 1250;
}

public class DeliveryRoute
{
    public int OrderId { get; set; }
    public Location RestaurantLocation { get; set; } = new();
    public Location CustomerLocation { get; set; } = new();
    public Location CurrentPartnerLocation { get; set; } = new();
    public double DistanceInKm { get; set; }
    public string RoutePath { get; set; } = string.Empty;
}

public class LocationUpdate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;

    public double GetDistanceTo(Location other)
    {
        // Haversine formula for approximate distance
        const double R = 6371; // Earth's radius in km
        var lat1Rad = Math.PI * Latitude / 180;
        var lat2Rad = Math.PI * other.Latitude / 180;
        var deltaLat = Math.PI * (other.Latitude - Latitude) / 180;
        var deltaLon = Math.PI * (other.Longitude - Longitude) / 180;

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }
}
