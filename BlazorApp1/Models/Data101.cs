using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит данные 101 формы для банка на дату
/// </summary>
public partial class Data101
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int Id101 { get; set; }

    /// <summary>
    /// Id шапки, т.е., для какого банка и на какую дату
    /// </summary>
    public int? IdInfo { get; set; }

    /// <summary>
    /// Id строки из 101 формы (БС2)
    /// </summary>
    public int? IdT101 { get; set; }

    /// <summary>
    /// Входящие остатки итого, тыс. руб
    /// </summary>
    public decimal? Vint { get; set; }

    /// <summary>
    /// Исходящие остатки итого, тыс. руб
    /// </summary>
    public decimal? Iitg { get; set; }

    public virtual FormInfo? IdInfoNavigation { get; set; }

    public virtual Templates101? IdT101Navigation { get; set; }
}
