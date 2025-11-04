using System.Threading.Tasks.Dataflow;
using Cysharp.Runtime.Multicast;
using MagicOnion.Server;
using MagicOnion.Client;
using MagicOnion.Server.Hubs;
using MapLib.Core.Interfaces;
using MapLib.Core.Models.ObjectModel;
using MapService.DTO.Requests;
using MapService.DTO.Responses;
using MapService.Interfaces;

namespace MapService.Services;

public class MapHubService : StreamingHubBase<IMapHub, IMapHubReceiver>, IMapHub
{
    private readonly IGameService gameService;
    
    private string? subscriptionId;
    private IGroup<IMapHubReceiver>? subscriberGroup;
    private SubscribeRequest? clientSubscription;

    
    public MapHubService(IGameService gameService)
    {
        this.gameService = gameService;
    }

    public async Task<SubscribeResponse> SubscribeAsync(SubscribeRequest request)
    {
        try
        {
            subscriptionId = Guid.NewGuid().ToString();
            subscriberGroup = await Group.AddAsync(subscriptionId);
            
            clientSubscription = request;
            
            return new SubscribeResponse()
            {
                Success = true,
                SubscriptionId = subscriptionId,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            return new SubscribeResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PlaceObjectResponse> PlaceObjectAsync(PlaceObjectRequest request)
    {
        var result = await gameService.PlaceObject(request.X, request.Y, (ObjectType)request.ObjectType);

        var response = new PlaceObjectResponse()
        {
            ObjectId = result.Item1,
            ObjectType = (int)result.Item2,
        };

        Client.OnObjectPlaced(response);
        return response;
    }

    public async Task<GetObjectsInAreaResponse> GetObjectsInAreaAsync(GetObjectsInAreaRequests request)
    {
        var result = await gameService.GetObjectsInArea(request.X1, request.X2, request.Y1, request.Y2);
        var items = result.Select(m => new GetObjectsInAreaResponse.ObjectItem()
        {
            Id = m.Id,
            Type = (int)m.Type,
            X = m.X,
            Y = m.Y,
        }).ToList();

        return new GetObjectsInAreaResponse()
        {
            Items = items,
        };
    }

    public Task<GetTerritoriesInAreaResponse> GetTerritoriesInAreaAsync(GetTerritoriesInAreaRequest request)
    {
        return Task.Run(() =>
        {
            var items = gameService.GetTerritoriesInArea(request.X1, request.X2, request.Y1, request.Y2)
                .Select(m => new GetTerritoriesInAreaResponse.TerritoryItem()
                {
                    Id = m.Id,
                    BordersInfo = new GetTerritoriesInAreaResponse.TerritoryItem.BordersInfoItem()
                    {
                        Height = m.BordersInfo.Height,
                        Width = m.BordersInfo.Width,
                        OffsetX = m.BordersInfo.OffsetX,
                        OffsetY = m.BordersInfo.OffsetY,
                    }
                }).ToList();

            return new GetTerritoriesInAreaResponse()
            {
                Items = items
            };
        });
    }

    public async Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request)
    {
        var objectId = await gameService.DeleteObject(request.ObjectId);
        
        var response =  new DeleteObjectResponse()
        {
            ObjectId = objectId
        };

        Client.OnObjectDeleted(response);
        
        return response;

    }

    public async Task UnsubscribeAsync()
    {
        if (subscriberGroup != null)
        {
            await subscriberGroup.RemoveAsync(Context);
            subscriberGroup = null;
            subscriptionId = string.Empty;
            clientSubscription = null;
        }
    }
}