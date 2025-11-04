using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using MagicOnion.Client;
using Grpc.Net.Client;
using MapService.Interfaces;
using MapService.DTO.Requests;
using MapService.DTO.Responses;

namespace MapService.Tests;

public class MapHubIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _httpClient;
    private GrpcChannel? _channel;
    private IMapHub?_client;

    public MapHubIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        _httpClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost:5001"),
            AllowAutoRedirect = false,
            HandleCookies = false
        });
    }
    
    [Fact]
    public async Task FullScenario_SubscribePlaceDeleteUnsubscribe_WorksCorrectly()
    {
        // Arrange 
        _channel = GrpcChannel.ForAddress(_httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = _httpClient,
            DisposeHttpClient = false 
        });

        _client = await StreamingHubClient.ConnectAsync<IMapHub, IMapHubReceiver>(
            _channel,
            new TestMapHubReceiver());
        
        if (_client == null)
            throw new InvalidOperationException("MagicOnion client failed to connect");
        
        // Act & Assert

        // Subscribe
        var subscribeResponse = await _client.SubscribeAsync(new SubscribeRequest
        {
            EventTypes = { "object_added", "object_deleted" }
        });
        
        Assert.True(subscribeResponse.Success);
        Assert.NotEmpty(subscribeResponse.SubscriptionId);

        // PlaceObject
        var placeResponse = await _client.PlaceObjectAsync(new PlaceObjectRequest()
            {
                X = 500,
                Y= 500,
                ObjectType = 1
            });
        Assert.NotEmpty(placeResponse.ObjectId);

        // GetObjectsInArea
        var getObjectsResponse = await _client.GetObjectsInAreaAsync(new GetObjectsInAreaRequests
        {
            X1 = 300, Y1 = 300,
            X2 = 600, Y2 = 600
        });
        Assert.NotEmpty(getObjectsResponse.Items);

        // DeleteObject
        var deleteResponse = await _client.DeleteObjectAsync(new DeleteObjectRequest
        {
            ObjectId = placeResponse.ObjectId
        });
        Assert.Equal(placeResponse.ObjectId, deleteResponse.ObjectId);

        // Unsubscribe
        await _client.UnsubscribeAsync();
    }

    private class TestMapHubReceiver : IMapHubReceiver
    {
        public void OnObjectPlaced(PlaceObjectResponse response)
        {
            Console.WriteLine($"[EVENT] Object placed: {response.ObjectId}");
        }

        public void OnObjectDeleted(DeleteObjectResponse response)
        {
            Console.WriteLine($"[EVENT] Object deleted: {response.ObjectId}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client != null)
            await _client.DisposeAsync();
        
        _channel?.Dispose();
        _httpClient?.Dispose();
    }
}