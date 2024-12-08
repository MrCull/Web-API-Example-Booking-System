using Api;
using Api.Routes;
using Api.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


builder.AddAzureCosmosClient("cosmos");
builder.Services.AddCosmosRepository("TheaterChainDB", "TheaterChain");
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

const string Expire60Seconds = "Expire60Seconds";
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder =>
        builder.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy(Expire60Seconds, builder =>
        builder.Expire(TimeSpan.FromSeconds(60)));
});
builder.AddRedisOutputCache("cache");

builder.Services.AddSingleton<ITheaterChainDtoMapperService, TheaterChainDtoMapperService>();


WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseOutputCache();
app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();

// Map feature-specific routes
app.MapTheaterChainEndpoints();
app.MapTheaterChainMovieEndpoints();
app.MapTheaterManagementEndpoints();
app.MapTheaterShowtimeManagementEndpoints();
app.MapTheaterListingsEndpoints();
app.MapReservationManagementEndpoints();


app.Run();
