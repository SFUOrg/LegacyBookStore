using System;
using System.ComponentModel.DataAnnotations;

namespace LegacyBookStore.DTO;

public record BookRequest(
    [Required(ErrorMessage = "�������� ����� �����������")]
    string Title,
    [Required(ErrorMessage = "�������� ������ �����������")]
    string Author,
    [Range(0,(double)decimal.MaxValue)]
    decimal Price,
    [Required(ErrorMessage = "�������� �����������")]
    string Description
);