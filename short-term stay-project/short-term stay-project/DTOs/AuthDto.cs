namespace short_term_stay_project.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Username, string Role);
public record RegisterRequest(string Username, string Password, string Role);
