using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит данные 123/135 форм для банка на дату
/// </summary>
public partial class DataNor
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int IdNor { get; set; }

    /// <summary>
    /// Id шапки, т.е., для какого банка и на какую дату
    /// </summary>
    public int? IdInfo { get; set; }

    /// <summary>
    /// Id строки из 123/135 форм (код норматива)
    /// </summary>
    public int? IdTnor { get; set; }

    /// <summary>
    /// Значение норматива
    /// </summary>
    public decimal? Val { get; set; }

    public virtual FormInfo? IdInfoNavigation { get; set; }

    public virtual TemplatesNor? IdTnorNavigation { get; set; }
}
