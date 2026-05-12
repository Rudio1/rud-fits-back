using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RudFitAI.Application.DTOs.Meals.Requests;
using RudFitAI.Application.DTOs.Meals.Responses;
using RudFitAI.Application.Services.Interfaces.Meals;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/meal-logs")]
[Authorize]
public sealed class MealLogsController : ControllerBase
{
    private readonly IMealLogService _mealLogService;
    private readonly IMealPhotoAnalysisService _mealPhotoAnalysisService;
    private readonly IMealDetectedFoodsNutritionEstimationService _mealDetectedFoodsNutritionEstimationService;

    public MealLogsController(
        IMealLogService mealLogService,
        IMealPhotoAnalysisService mealPhotoAnalysisService,
        IMealDetectedFoodsNutritionEstimationService mealDetectedFoodsNutritionEstimationService)
    {
        _mealLogService = mealLogService;
        _mealPhotoAnalysisService = mealPhotoAnalysisService;
        _mealDetectedFoodsNutritionEstimationService = mealDetectedFoodsNutritionEstimationService;
    }

    [HttpPost]
    public async Task<ActionResult<MealLogResponseDto>> CreateManual(
        CreateMealLogRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            MealLogResponseDto response =
                await _mealLogService.CreateManualAsync(userId, request, cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lista refeições do usuário na data civil informada (mesmo critério dos horários gravados no banco).
    /// Query obrigatória: <c>date=YYYY-MM-DD</c>.
    /// Rotas equivalentes: <c>GET /api/meal-logs?date=...</c> e <c>GET /api/meal-logs/by-date?date=...</c>.
    /// </summary>
    [HttpGet]
    [HttpGet("by-date")]
    public async Task<ActionResult<IReadOnlyCollection<MealLogResponseDto>>> ListByDate(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (date is null)
        {
            return BadRequest(new { message = "Informe a data no formato YYYY-MM-DD." });
        }

        IReadOnlyCollection<MealLogResponseDto> response =
            await _mealLogService.ListByDateAsync(userId, date.Value, cancellationToken);

        return Ok(response);
    }

    [HttpPost("analyze-photo")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MealPhotoAnalysisResponseDto>> AnalyzePhoto(
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid _))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        if (image is null || image.Length == 0)
        {
            return BadRequest(new { message = "Envie uma imagem no campo image." });
        }

        await using MemoryStream buffer = new();
        await image.CopyToAsync(buffer, cancellationToken);
        byte[] bytes = buffer.ToArray();

        string? detectedMimeType = DetectImageMimeType(bytes);
        if (detectedMimeType is null)
        {
            return BadRequest(new { message = "Formato de imagem inválido. Use JPEG, PNG ou WebP." });
        }

        try
        {
            MealPhotoAnalysisResponseDto response =
                await _mealPhotoAnalysisService.AnalyzePhotoAsync(bytes, detectedMimeType, cancellationToken);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(408, new { message = "Tempo esgotado ao analisar a imagem. Tente novamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("estimate-detected-foods-nutrition")]
    public async Task<ActionResult<EstimateDetectedFoodsNutritionResponseDto>> EstimateDetectedFoodsNutrition(
        [FromBody] EstimateDetectedFoodsNutritionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid _))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            EstimateDetectedFoodsNutritionResponseDto response =
                await _mealDetectedFoodsNutritionEstimationService.EstimateAsync(request, cancellationToken);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(408, new { message = "Tempo esgotado ao estimar nutrição. Tente novamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("from-detected-foods")]
    public async Task<ActionResult<MealLogResponseDto>> CreateFromDetectedFoods(
        [FromBody] CreateMealLogFromDetectedFoodsRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out Guid userId))
        {
            return Unauthorized(new { message = "Usuário não autenticado." });
        }

        try
        {
            MealLogResponseDto response =
                await _mealLogService.CreateFromDetectedFoodsAsync(userId, request, cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetUserId(out Guid userId)
    {
        string? userIdRaw =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdRaw, out userId);
    }

    /// <summary>
    /// Detecta o tipo real da imagem a partir dos magic bytes do arquivo,
    /// ignorando o Content-Type enviado pelo cliente (que pode vir como
    /// application/octet-stream em uploads vindos de apps mobile).
    /// </summary>
    private static string? DetectImageMimeType(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (bytes.Length >= 8
            && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
            && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
        {
            return "image/png";
        }

        if (bytes.Length >= 12
            && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
            && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
        {
            return "image/webp";
        }

        return null;
    }
}
