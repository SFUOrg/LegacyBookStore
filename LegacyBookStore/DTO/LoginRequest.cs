using System;

namespace LegacyBookStore.DTO;

public record LoginRequest(
 string Username,
 string Password
);
