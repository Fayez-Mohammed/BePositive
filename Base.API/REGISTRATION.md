═══════════════════════════════════════════════════════════════
MESSAGING — REGISTRATION GUIDE
═══════════════════════════════════════════════════════════════

STEP 1 — AppDbContext.cs
─────────────────────────────────────────────────────────────
Add these two DbSet properties:

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ChatMessage>  ChatMessages  { get; set; }

Also add this in OnModelCreating to enforce uniqueness:

    modelBuilder.Entity<Conversation>()
        .HasIndex(c => new { c.HospitalId, c.DonorId })
        .IsUnique();


STEP 2 — Migration
─────────────────────────────────────────────────────────────
    add-migration AddMessaging
    update-database


STEP 3 — ServiceCollectionExtensions.cs → AddInfrastructureServices()
─────────────────────────────────────────────────────────────
Add inside AddInfrastructureServices():

    // FCM (if not already added)
    services.AddScoped<IFcmService, FcmService>();
    services.AddScoped<FindAndNotifyDonorsJob>();

    // Messaging
    services.AddScoped<IMessagingService, MessagingService>();
    services.AddSingleton<PresenceTracker>();

Add SignalR inside AddApiBehavior() (after AddControllers()):

    services.AddSignalR();


STEP 4 — Program.cs — Map the Hub
─────────────────────────────────────────────────────────────
Add AFTER app.MapControllers():

    app.MapHub<MessagingHub>("/hubs/messaging");


STEP 5 — Program.cs — CORS fix for SignalR
─────────────────────────────────────────────────────────────
SignalR requires AllowCredentials() — replace the AllowAll CORS
policy with this in ServiceCollectionExtensions.AddApiBehavior():

    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",   // dev React
                    "http://localhost:5173",   // dev Vite
                    "https://bepositive.runasp.net" // production
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // ← required for SignalR
        });
    });

NOTE: AllowAnyOrigin() and AllowCredentials() cannot be combined.
You MUST list origins explicitly when using SignalR.


STEP 6 — Flutter (donor app) SignalR connection
─────────────────────────────────────────────────────────────
Package: signalr_netcore (pub.dev)

    final connection = HubConnectionBuilder()
        .withUrl(
            "https://bepositive.runasp.net/hubs/messaging",
            options: HttpConnectionOptions(
                accessTokenFactory: () async => jwtToken,
            ),
        )
        .build();

    await connection.start();

    // Listen
    connection.on("message:new", (args) { ... });
    connection.on("presence:update", (args) { ... });
    connection.on("conversation:typing", (args) { ... });
    connection.on("conversation:read", (args) { ... });

    // Send
    await connection.invoke("SendMessage", args: [conversationId, text]);
    await connection.invoke("MarkRead",    args: [conversationId]);
    await connection.invoke("Typing",      args: [conversationId, true]);


STEP 7 — React (hospital web) SignalR connection
─────────────────────────────────────────────────────────────
Package: @microsoft/signalr

    import * as signalR from "@microsoft/signalr";

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("https://bepositive.runasp.net/hubs/messaging", {
            accessTokenFactory: () => localStorage.getItem("access_token"),
        })
        .withAutomaticReconnect()
        .build();

    await connection.start();

    connection.on("message:new",         (payload) => { ... });
    connection.on("message:deleted",     (payload) => { ... });
    connection.on("presence:update",     (payload) => { ... });
    connection.on("conversation:typing", (payload) => { ... });
    connection.on("conversation:read",   (payload) => { ... });

    // Send
    await connection.invoke("SendMessage", conversationId, text);
    await connection.invoke("MarkRead",    conversationId);
    await connection.invoke("Typing",      conversationId, true);


═══════════════════════════════════════════════════════════════
FILE PLACEMENT SUMMARY
═══════════════════════════════════════════════════════════════

Base.DAL/Models/MessagingModels/
  └── ConversationModels.cs

Base.Shared/DTOs/MessagingDTOs/
  └── MessagingDTOs.cs

Base.Services/Interfaces/HospitalInterfaces/
  └── IMessagingService.cs

Base.Services/Implementations/HospitalImplementations/
  └── MessagingService.cs

Base.Services/Implementations/
  └── PresenceTracker.cs

Base.API/Hubs/
  └── MessagingHub.cs

Base.API/Controllers/Hospital/
  └── HospitalMessagingController.cs

═══════════════════════════════════════════════════════════════
SIGNALR ENDPOINTS SUMMARY
═══════════════════════════════════════════════════════════════

WS   wss://bepositive.runasp.net/hubs/messaging   ← SignalR hub

REST (fallback):
GET  /api/hospital/conversations
POST /api/hospital/conversations
GET  /api/hospital/conversations/:id/messages
POST /api/hospital/conversations/:id/messages
DEL  /api/hospital/messages/:messageId
POST /api/hospital/conversations/:id/read
GET  /api/hospital/messages/unread-count

═══════════════════════════════════════════════════════════════
