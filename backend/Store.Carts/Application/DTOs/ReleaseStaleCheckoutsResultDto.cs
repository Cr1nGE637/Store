namespace Store.Carts.Application.DTOs;

public record ReleaseStaleCheckoutsResultDto(
    int CheckedCount,
    int ReleasedCount,
    int SkippedBecauseOrderExists);
