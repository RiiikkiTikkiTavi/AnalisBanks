using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит струкутру 101 формы
/// </summary>
public partial class Templates101
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int IdT101 { get; set; }

    /// <summary>
    /// Глава плана счетов (А, Б, В, Г, Д)
    /// </summary>
    public char? Plan { get; set; }

    /// <summary>
    /// Номер счета 1-го, 2-го порядка или суммы БС2
    /// </summary>
    /// не используется
    public string? NumSc { get; set; }

    /// <summary>
    /// Наименование БС2
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Признак счета (А/П)
    /// </summary>
    public string? AP { get; set; }

    public virtual ICollection<ArgConsist> ArgConsists { get; set; } = new List<ArgConsist>();

    public virtual ICollection<Data101> Data101s { get; set; } = new List<Data101>();
}
