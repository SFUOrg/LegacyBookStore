using System;
using System.ComponentModel.DataAnnotations;

namespace LegacyBookStore.DTO;

public record BookRequest(
    [Required(ErrorMessage = "Название книги обязательно")]
    string Title,
    [Required(ErrorMessage = "Название автора обязательно")]
    string Author,
    [Range(0,(double)decimal.MaxValue)]
    decimal Price,
    [Required(ErrorMessage = "Описание обязательно")]
    string Description
);