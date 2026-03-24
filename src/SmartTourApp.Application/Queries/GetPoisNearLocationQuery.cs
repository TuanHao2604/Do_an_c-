using MediatR;
using SmartTourApp.Domain.Interfaces;

namespace SmartTourApp.Application.Queries;

/// <summary>
/// CQRS Query: Find POIs near a given location using PostGIS.
/// </summary>
public record GetPoisNearLocationQuery(double Latitude, double Longitude, double RadiusKm) 
    : IRequest<List<PoiDetailDto>>;
