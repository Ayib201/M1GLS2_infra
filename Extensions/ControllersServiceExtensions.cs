namespace M1GLS2_infra.Extensions;

public static class ControllersServiceExtensions
{
    /// <summary>
    /// Active le support des contrôleurs MVC (attributs [ApiController],
    /// [Route], [HttpGet]/[HttpPost], [Authorize]...). Nécessaire pour que
    /// Controllers/ServicesController.cs et Controllers/ProfilsController.cs
    /// soient détectés et exposés comme endpoints HTTP.
    /// </summary>
    public static IServiceCollection AddControllersSupport(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}
