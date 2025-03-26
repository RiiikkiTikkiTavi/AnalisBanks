using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит струкутру 123 и 135 форм
/// </summary>
public partial class TemplatesNor
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int IdTnor { get; set; }

    /// <summary>
    /// Номер формы (123/135)
    /// </summary>
    public string? Form { get; set; }

    /// <summary>
    /// Код показателя
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Наименование показателя
    /// </summary>
    public string? Name { get; set; }

    public virtual ICollection<ArgConsist> ArgConsists { get; set; } = new List<ArgConsist>();

    public virtual ICollection<DataNor> DataNors { get; set; } = new List<DataNor>();
}
