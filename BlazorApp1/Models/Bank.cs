using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит список банков и их регистрационные номера
/// </summary>
public partial class Bank
{
    /// <summary>
    /// Регистрационный номер банка
    /// </summary>
    public int Regnum { get; set; }

    /// <summary>
    /// Наименование банка
    /// </summary>
    public string Name { get; set; } = null!;

    public virtual ICollection<FormInfo> FormInfos { get; set; } = new List<FormInfo>();
}
