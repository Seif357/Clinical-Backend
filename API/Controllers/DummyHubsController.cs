using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// SignalR real-time hubs
/// </summary>
[ApiController]
[Route("hubs")]
[Tags("SignalR Hubs")]
public class DummyHubsController : ControllerBase
{
    /// <summary>Notification Hub — SignalR WebSocket endpoint</summary>
    /// <remarks>
    /// **Cannot be called as a REST endpoint.**
    /// Connect from your frontend using a SignalR client:
    ///
    ///     const connection = new HubConnectionBuilder()
    ///         .withUrl("/hubs/notifications")
    ///         .build();
    /// Send the message {"protocol":"json","version":1} to complete the handshake
    /// Supports WebSockets, Server-Sent Events, and Long Polling transports.
    /// </remarks>
    /// <response code="101">WebSocket connection established</response>
    /// <response code="200">Fallback transport connected</response>
    [HttpGet("notifications")]
    [ProducesResponseType(101)]
    [ProducesResponseType(200)]
    public IActionResult Notifications() =>
        Ok("Connect via a SignalR client to wss://clinical.runasp.net/hubs/notifications?access_token=YOUR_TOKEN \n https://clinical.runasp.net/hubs/notifications?access_token=YOUR_TOKEN \n Send the message \n\"{\"protocol\":\"json\",\"version\":1}\"\n to complete the handshake — see description above.");
}